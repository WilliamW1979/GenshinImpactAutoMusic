namespace GenshinImpactAutoMusic;

public sealed class AppSettings
{
    private const string Category = "Engine";
    private readonly SettingsFile File;

    public AppSettings(string path) => File = new SettingsFile(path);

    public int GoldTolerance { get => File.Get(Category, nameof(GoldTolerance), 50); set => File.Set(Category, nameof(GoldTolerance), value); }
    public int BlueTolerance { get => File.Get(Category, nameof(BlueTolerance), 75); set => File.Set(Category, nameof(BlueTolerance), value); }
    public int HitOffset { get => File.Get(Category, nameof(HitOffset), 0); set => File.Set(Category, nameof(HitOffset), value); }
    public int TimingOffsetMs { get => File.Get(Category, nameof(TimingOffsetMs), -100); set => File.Set(Category, nameof(TimingOffsetMs), value); }
    public bool AutoPlayActive { get => File.Get(Category, nameof(AutoPlayActive), true); set => File.Set(Category, nameof(AutoPlayActive), value); }

    public void ResetToDefaults() { GoldTolerance = 50; BlueTolerance = 75; HitOffset = 0; TimingOffsetMs = -100; AutoPlayActive = true; }

    public void Apply(AutoMusicEngine engine)
    {
        engine.GoldTolerance = GoldTolerance;
        engine.BlueTolerance = BlueTolerance;
        engine.HitOffset = HitOffset;
        engine.TimingOffsetMs = TimingOffsetMs;
        engine.AutoPlayActive = AutoPlayActive;
    }
}
