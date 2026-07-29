using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace GenshinImpactAutoMusic;

public sealed partial class SettingsWindow : Window
{
    private readonly AutoMusicEngine Engine;
    private readonly AppSettings Settings;
    private readonly DispatcherTimer StatusTimer;
    private readonly GitHubUpdateChecker UpdateChecker = new();

    public SettingsWindow(AutoMusicEngine engine, AppSettings settings)
    {
        InitializeComponent();
        Engine = engine;
        Settings = settings;
        LoadSettingsIntoBoxes();
        AdminModeText.Text = Engine.AdminModeActive ? "True" : "False";
        AdminWarning.Visibility = Engine.AdminModeActive ? Visibility.Collapsed : Visibility.Visible;
        StatusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        StatusTimer.Tick += (_, _) => RefreshStatus();
        StatusTimer.Start();
    }

    private void LoadSettingsIntoBoxes()
    {
        GoldToleranceBox.Text = Settings.GoldTolerance.ToString();
        BlueToleranceBox.Text = Settings.BlueTolerance.ToString();
        HitOffsetBox.Text = Settings.HitOffset.ToString();
        TimingOffsetBox.Text = Settings.TimingOffsetMs.ToString();
    }

    private void RefreshStatus()
    {
        GenshinGameText.Text = Engine.GenshinGameActive ? "Active" : "Inactive";
        MusicPlayingText.Text = Engine.MusicPlaying ? "True" : "False";
        AutoPlayText.Text = Engine.AutoPlayActive ? "True" : "False";
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(GoldToleranceBox.Text, out int gold) || !int.TryParse(BlueToleranceBox.Text, out int blue) || !int.TryParse(HitOffsetBox.Text, out int hit) || !int.TryParse(TimingOffsetBox.Text, out int timing)) { MessageBox.Show("All values must be whole numbers."); return; }
        Settings.GoldTolerance = gold;
        Settings.BlueTolerance = blue;
        Settings.HitOffset = hit;
        Settings.TimingOffsetMs = timing;
        Settings.Apply(Engine);
    }

    private void OnReset(object sender, RoutedEventArgs e)
    {
        Settings.ResetToDefaults();
        Settings.Apply(Engine);
        LoadSettingsIntoBoxes();
    }

    private async void OnCheckUpdate(object sender, RoutedEventArgs e)
    {
        (bool updateAvailable, string latestVersion, string releaseUrl) = await UpdateChecker.CheckAsync();
        MessageBox.Show(updateAvailable ? $"Update {latestVersion} is available." : "You are on the latest version.");
        if (updateAvailable) Process.Start(new ProcessStartInfo(releaseUrl) { UseShellExecute = true });
    }

    private void OnNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
