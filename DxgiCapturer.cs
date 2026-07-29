using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace GenshinImpactAutoMusic;

internal sealed class DxgiCapturer : IDisposable
{
    private SharpDX.Direct3D11.Device? Device;
    private OutputDuplication? Duplication;
    private Texture2D? CpuTexture;
    private byte[]? FrameBufferField;
    private int Width;
    private int Height;
    private int DesktopLeft;
    private int DesktopTop;

    public byte[]? FrameBuffer => FrameBufferField;

    public bool Initialize(IntPtr gameWindow, IntPtr monitorHandle)
    {
        try
        {
            using Factory1 factory = new();
            for (int adapterIndex = 0; ; adapterIndex++)
            {
                Adapter1 adapter;
                try { adapter = factory.GetAdapter1(adapterIndex); } catch (SharpDXException) { break; }
                if (TryInitializeFromAdapter(adapter, monitorHandle)) { adapter.Dispose(); return true; }
                adapter.Dispose();
            }
            return false;
        }
        catch (SharpDXException) { return false; }
    }

    private bool TryInitializeFromAdapter(Adapter1 adapter, IntPtr monitorHandle)
    {
        for (int outputIndex = 0; ; outputIndex++)
        {
            Output output;
            try { output = adapter.GetOutput(outputIndex); } catch (SharpDXException) { return false; }
            if (output.Description.MonitorHandle != monitorHandle) { output.Dispose(); continue; }
            using Output1 output1 = output.QueryInterface<Output1>();
            Device = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.None);
            Duplication = output1.DuplicateOutput(Device);
            OutputDescription desc = output.Description;
            DesktopLeft = desc.DesktopBounds.Left;
            DesktopTop = desc.DesktopBounds.Top;
            Width = desc.DesktopBounds.Right - desc.DesktopBounds.Left;
            Height = desc.DesktopBounds.Bottom - desc.DesktopBounds.Top;
            Texture2DDescription texDesc = new() { CpuAccessFlags = CpuAccessFlags.Read, BindFlags = BindFlags.None, Format = Format.B8G8R8A8_UNorm, Width = Width, Height = Height, OptionFlags = ResourceOptionFlags.None, MipLevels = 1, ArraySize = 1, SampleDescription = new SampleDescription(1, 0), Usage = ResourceUsage.Staging };
            CpuTexture = new Texture2D(Device, texDesc);
            FrameBufferField = new byte[Width * Height * 4];
            output.Dispose();
            return true;
        }
    }

    public bool CaptureFrame()
    {
        if (Duplication is null || Device is null || CpuTexture is null) return false;
        SharpDX.Result result = Duplication.TryAcquireNextFrame(10, out OutputDuplicateFrameInformation _, out SharpDX.DXGI.Resource desktopResource);
        if (result.Failure) return false;
        using (Texture2D desktopTexture = desktopResource.QueryInterface<Texture2D>()) Device.ImmediateContext.CopyResource(desktopTexture, CpuTexture);
        Duplication.ReleaseFrame();
        DataBox mapSource = Device.ImmediateContext.MapSubresource(CpuTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
        byte[] localBuffer = new byte[Width * Height * 4];
        Marshal.Copy(mapSource.DataPointer, localBuffer, 0, localBuffer.Length);
        Device.ImmediateContext.UnmapSubresource(CpuTexture, 0);
        FrameBufferField = localBuffer;
        return true;
    }

    public Color GetPixelColor(Point pt)
    {
        int localX = pt.X - DesktopLeft;
        int localY = pt.Y - DesktopTop;
        if (FrameBufferField is null || localX < 0 || localX >= Width || localY < 0 || localY >= Height) return Color.Empty;
        int index = (localY * Width + localX) * 4;
        return Color.FromArgb(FrameBufferField[index + 2], FrameBufferField[index + 1], FrameBufferField[index]);
    }

    public void Dispose()
    {
        CpuTexture?.Dispose();
        Duplication?.Dispose();
        Device?.Dispose();
    }
}
