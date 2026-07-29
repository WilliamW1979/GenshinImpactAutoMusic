using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GenshinImpactAutoMusic;

public sealed class AutoMusicEngine
{
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint nInputs, ref Input pInputs, int cbSize);
    [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    private const int GateY = 300;
    private const double FallSpeedPxPerMs = 0.655;
    private const double NoteLengthPx = 64.0;
    private const int BlueGapMergeMs = 40;
    private const uint MonitorDefaultToNearest = 2;
    private const uint InputKeyboard = 1;
    private const uint KeyEventFKeyDown = 0x0000;
    private const uint KeyEventFKeyUp = 0x0002;
    private const int FixedWidth = 1920;
    private const int FixedHeight = 1080;
    private const int PauseRegionStartX = 1813;
    private const int PauseRegionStartY = 19;
    private const int PauseRegionSize = 58;
    private const int FaceRadius = 20;
    private const double FaceStdDevThreshold = 18.0;
    private const double FaceBarContrastThreshold = 30.0;

    private static readonly int[] PressingPoints = { 418, 635, 852, 1068, 1285, 1501 };
    private static readonly int LineY = 919;
    private static readonly Color ButtonGold = Color.FromArgb(220, 178, 85);
    private static readonly Color ButtonBlue = Color.FromArgb(114, 100, 250);
    private static readonly ushort[] VirtualKeys = { 0x41, 0x53, 0x44, 0x4A, 0x4B, 0x4C };

    private readonly DxgiCapturer Capturer = new();
    private readonly bool[] FaceMask;
    private readonly bool[] BarMask;
    private readonly bool[] Holding = new bool[6];

    private IntPtr GenshinWindow = IntPtr.Zero;
    private int WindowScreenLeft;
    private int WindowScreenTop;
    private bool PendingMusicState;
    private int PendingMusicCount;

    public int GoldTolerance { get; set; } = 50;
    public int BlueTolerance { get; set; } = 75;
    public int HitOffset { get; set; } = 0;
    public int TimingOffsetMs { get; set; } = -100;
    public bool AutoPlayActive { get; set; } = true;
    public bool GenshinGameActive { get; private set; }
    public bool MusicPlaying { get; private set; }
    public bool AdminModeActive { get; } = AdminHelper.IsElevated();

    public AutoMusicEngine() => (FaceMask, BarMask) = BuildMasks();

    public void Start()
    {
        _ = MonitorWindowLoopAsync();
        _ = CaptureLoopAsync();
        _ = DetectPauseButtonLoopAsync();
        for (int lane = 1; lane <= 6; lane++) _ = RunGoldGateAsync(lane);
        for (int lane = 1; lane <= 6; lane++) _ = RunBlueGateAsync(lane);
    }

    private async Task MonitorWindowLoopAsync()
    {
        while (true)
        {
            if (GenshinWindow == IntPtr.Zero || !IsWindow(GenshinWindow))
            {
                IntPtr candidate = Process.GetProcessesByName("GenshinImpact").Select(p => p.MainWindowHandle).FirstOrDefault(h => h != IntPtr.Zero);
                if (candidate != IntPtr.Zero) TryAcquireWindow(candidate);
            }
            await Task.Delay(1000);
        }
    }

    private void TryAcquireWindow(IntPtr window)
    {
        GetClientRect(window, out Rect clientRect);
        if (clientRect.Right - clientRect.Left != FixedWidth || clientRect.Bottom - clientRect.Top != FixedHeight) return;
        Point origin = new() { X = 0, Y = 0 };
        ClientToScreen(window, ref origin);
        if (!Capturer.Initialize(window, MonitorFromWindow(window, MonitorDefaultToNearest))) return;
        GenshinWindow = window;
        WindowScreenLeft = origin.X;
        WindowScreenTop = origin.Y;
    }

    private async Task CaptureLoopAsync() { while (true) { Capturer.CaptureFrame(); await Task.Delay(5); } }

    private async Task RunGoldGateAsync(int lane)
    {
        int gateX = PressingPoints[lane - 1];
        bool wasPresent = false;
        long blobStartTick = 0;
        while (true)
        {
            if (MusicPlaying && AutoPlayActive)
            {
                bool isPresent = IsButton(ButtonGold, GetPixelColor(new() { X = gateX, Y = GateY }), GoldTolerance);
                if (isPresent && !wasPresent) blobStartTick = Environment.TickCount64;
                else if (!isPresent && wasPresent) ScheduleGoldBlob(lane, blobStartTick, Environment.TickCount64);
                wasPresent = isPresent;
            }
            else { wasPresent = false; await Task.Delay(99); }
            await Task.Delay(1);
        }
    }

    private async Task RunBlueGateAsync(int lane)
    {
        int gateX = PressingPoints[lane - 1];
        bool confirmedPresent = false;
        long lastPresentTick = 0;
        while (true)
        {
            if (MusicPlaying && AutoPlayActive)
            {
                bool isPresent = IsButton(ButtonBlue, GetPixelColor(new() { X = gateX, Y = GateY }), BlueTolerance);
                if (isPresent)
                {
                    lastPresentTick = Environment.TickCount64;
                    if (!confirmedPresent)
                    {
                        confirmedPresent = true;
                        ScheduleBlueBlob(lane, Environment.TickCount64, Holding[lane - 1]);
                        Holding[lane - 1] = !Holding[lane - 1];
                    }
                }
                else if (confirmedPresent && Environment.TickCount64 - lastPresentTick > BlueGapMergeMs) confirmedPresent = false;
            }
            else confirmedPresent = false;
            await Task.Delay(1);
        }
    }

    private void ScheduleGoldBlob(int lane, long startTick, long endTick)
    {
        double durationMs = endTick - startTick;
        if (durationMs <= 0) return;
        int noteCount = Math.Max(1, (int)Math.Round(durationMs * FallSpeedPxPerMs / NoteLengthPx));
        double perNoteDurationMs = durationMs / noteCount;
        double travelMs = (LineY - HitOffset - GateY) / FallSpeedPxPerMs + TimingOffsetMs;
        for (int i = 0; i < noteCount; i++)
        {
            long targetTick = startTick + (long)(perNoteDurationMs * (i + 0.5)) + (long)travelMs;
            _ = ScheduleGoldPressAsync(lane, targetTick - Environment.TickCount64);
        }
    }

    private void ScheduleBlueBlob(int lane, long startTick, bool holding)
    {
        double travelMs = (LineY - HitOffset - GateY) / FallSpeedPxPerMs + TimingOffsetMs;
        _ = ScheduleBluePressAsync(lane, startTick + (long)travelMs - Environment.TickCount64, holding);
    }

    private async Task ScheduleGoldPressAsync(int lane, long delayMs)
    {
        if (Holding[lane - 1]) _ = ReleaseHeldGoldAsync(lane, delayMs);
        if (delayMs > 0) await Task.Delay((int)delayMs);
        await PressKeyAsync(lane);
    }

    private async Task ReleaseHeldGoldAsync(int lane, long delayMs)
    {
        if (delayMs > 50) await Task.Delay((int)delayMs - 50);
        KeyRelease(lane);
        Holding[lane - 1] = false;
    }

    private async Task ScheduleBluePressAsync(int lane, long delayMs, bool holding)
    {
        if (delayMs > 0) await Task.Delay((int)delayMs);
        if (holding) KeyRelease(lane); else KeyHold(lane);
    }

    private async Task DetectPauseButtonLoopAsync()
    {
        while (true)
        {
            bool isForeground = GenshinWindow != IntPtr.Zero && GenshinWindow == GetForegroundWindow();
            GenshinGameActive = isForeground;
            bool current = isForeground && IsPauseButtonVisible();
            if (current == PendingMusicState) PendingMusicCount++;
            else { PendingMusicState = current; PendingMusicCount = 1; }
            if (PendingMusicCount >= 2 && MusicPlaying != PendingMusicState)
            {
                MusicPlaying = PendingMusicState;
                if (!MusicPlaying) for (int lane = 1; lane <= 6; lane++) { Holding[lane - 1] = false; KeyRelease(lane); }
            }
            await Task.Delay(50);
        }
    }

    private static bool IsBarPixel(int x, int y) => (x >= 18 && x <= 25 || x >= 32 && x <= 39) && y >= 17 && y <= 40;

    private static (bool[] Face, bool[] Bar) BuildMasks()
    {
        bool[] face = new bool[PauseRegionSize * PauseRegionSize];
        bool[] bar = new bool[PauseRegionSize * PauseRegionSize];
        double cx = (PauseRegionSize - 1) / 2.0;
        double cy = (PauseRegionSize - 1) / 2.0;
        double r2 = FaceRadius * (double)FaceRadius;
        for (int y = 0; y < PauseRegionSize; y++)
            for (int x = 0; x < PauseRegionSize; x++)
            {
                bool isBar = IsBarPixel(x, y);
                double dx = x - cx;
                double dy = y - cy;
                face[y * PauseRegionSize + x] = dx * dx + dy * dy <= r2 && !isBar;
                bar[y * PauseRegionSize + x] = isBar;
            }
        return (face, bar);
    }

    private bool IsPauseButtonVisible()
    {
        byte[]? buffer = Capturer.FrameBuffer;
        if (buffer is null) return false;
        double faceSum = 0, faceSumSq = 0, barSum = 0;
        int faceCount = 0, barCount = 0;
        for (int y = 0; y < PauseRegionSize; y++)
            for (int x = 0; x < PauseRegionSize; x++)
            {
                int idx = y * PauseRegionSize + x;
                bool isFace = FaceMask[idx];
                bool isBar = BarMask[idx];
                if (!isFace && !isBar) continue;
                int px = PauseRegionStartX + x;
                int py = PauseRegionStartY + y;
                if (px < 0 || px >= FixedWidth || py < 0 || py >= FixedHeight) continue;
                int frameIdx = (py * FixedWidth + px) * 4;
                double luma = 0.299 * buffer[frameIdx + 2] + 0.587 * buffer[frameIdx + 1] + 0.114 * buffer[frameIdx];
                if (isFace) { faceSum += luma; faceSumSq += luma * luma; faceCount++; } else { barSum += luma; barCount++; }
            }
        if (faceCount == 0 || barCount == 0) return false;
        double faceMean = faceSum / faceCount;
        double faceStdDev = Math.Sqrt(Math.Max(faceSumSq / faceCount - faceMean * faceMean, 0));
        double contrast = Math.Abs(faceMean - barSum / barCount);
        return faceStdDev <= FaceStdDevThreshold && contrast >= FaceBarContrastThreshold;
    }

    private void SendKeyEvent(int lane, bool down)
    {
        if (lane < 1 || lane > VirtualKeys.Length) return;
        if (GenshinWindow == IntPtr.Zero || GetForegroundWindow() != GenshinWindow) return;
        Input input = new() { Type = InputKeyboard, U = new InputUnion { Ki = new KeybdInput { WVk = VirtualKeys[lane - 1], WScan = 0, DwFlags = down ? KeyEventFKeyDown : KeyEventFKeyUp, Time = 0, DwExtraInfo = IntPtr.Zero } } };
        SendInput(1, ref input, Marshal.SizeOf<Input>());
    }

    private Color GetPixelColor(Point pt) => Capturer.GetPixelColor(new() { X = pt.X + WindowScreenLeft, Y = pt.Y + WindowScreenTop });
    private static bool IsButton(Color target, Color test, int tolerance) => Math.Abs(target.R - test.R) <= tolerance && Math.Abs(target.G - test.G) <= tolerance && Math.Abs(target.B - test.B) <= tolerance;
    private async Task PressKeyAsync(int lane) { SendKeyEvent(lane, true); await Task.Delay(50); SendKeyEvent(lane, false); }
    private void KeyRelease(int lane) => SendKeyEvent(lane, false);
    private void KeyHold(int lane) => SendKeyEvent(lane, true);
}
