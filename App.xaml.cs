using System.IO;
using System.Windows;
using WardShieldSplashWindow;
using Application = System.Windows.Application;

namespace GenshinImpactAutoMusic;

public sealed partial class App : Application
{
    private readonly AutoMusicEngine Engine = new();
    private readonly AppSettings Settings = new(SettingsPath());
    private NotifyIcon? TrayIcon;
    private SettingsWindow? SettingsWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath())!);
        await AppSplashScreen.ShowAsync(InitializeAsync);
        BuildTrayIcon();
    }

    private static string SettingsPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GenshinImpactAutoMusic", "settings.ini");

    private Task InitializeAsync()
    {
        Settings.Apply(Engine);
        Engine.Start();
        return Task.CompletedTask;
    }

    private void BuildTrayIcon()
    {
        ToolStripMenuItem toggle = new("Active") { Checked = Engine.AutoPlayActive, CheckOnClick = true };
        toggle.Click += (_, _) => { Engine.AutoPlayActive = toggle.Checked; Settings.AutoPlayActive = toggle.Checked; };
        ToolStripMenuItem exit = new("Exit");
        exit.Click += (_, _) => Shutdown();
        ContextMenuStrip menu = new();
        menu.Items.Add(toggle);
        menu.Items.Add(exit);
        TrayIcon = new NotifyIcon { Icon = LoadTrayIcon(), Visible = true, Text = "Genshin Impact Auto Music", ContextMenuStrip = menu };
        TrayIcon.Click += (_, _) => ShowSettingsWindow();
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        using Stream stream = typeof(App).Assembly.GetManifestResourceStream("GenshinImpactAutoMusic.Resources.Ward_Shield_Transparent_Background.ico") ?? throw new InvalidOperationException("Tray icon resource missing.");
        return new System.Drawing.Icon(stream);
    }

    private void ShowSettingsWindow()
    {
        SettingsWindow ??= new SettingsWindow(Engine, Settings);
        SettingsWindow.Show();
        SettingsWindow.WindowState = WindowState.Normal;
        SettingsWindow.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        TrayIcon?.Dispose();
        base.OnExit(e);
    }
}
