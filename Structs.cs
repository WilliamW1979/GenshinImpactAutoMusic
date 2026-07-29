using System.Runtime.InteropServices;

namespace GenshinImpactAutoMusic;

internal struct Point
{
    public int X;
    public int Y;
}

internal struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

internal struct Input
{
    public uint Type;
    public InputUnion U;
}

[StructLayout(LayoutKind.Explicit)]
internal struct InputUnion
{
    [FieldOffset(0)] public MouseInput Mi;
    [FieldOffset(0)] public KeybdInput Ki;
    [FieldOffset(0)] public HardwareInput Hi;
}

internal struct MouseInput
{
    public int Dx;
    public int Dy;
    public uint MouseData;
    public uint DwFlags;
    public uint Time;
    public IntPtr DwExtraInfo;
}

internal struct HardwareInput
{
    public uint UMsg;
    public ushort WParamL;
    public ushort WParamH;
}

internal struct KeybdInput
{
    public ushort WVk;
    public ushort WScan;
    public uint DwFlags;
    public uint Time;
    public IntPtr DwExtraInfo;
}
