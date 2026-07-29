# GenshinImpactAutoMusic

## What changed in the cleanup
`AutoMusicEngine.cs` replaces `Program.cs`. Line count went from 292 to ~215 despite
adding a window-monitoring loop the old one-shot `Main` didn't need. Changes:
- `IsGoldButton` / `IsBlueButton` merged into one `IsButton(target, test, tolerance)`.
- `ReleaseAfterDelay` removed — it was never called anywhere in the original file.
- `Timestamp()` and the `Console.WriteLine` diagnostics removed since this is now a
  tray app with no console.
- `InitializeFaceMask` / `InitializeBarMask` merged into one `BuildMasks()` that
  builds both arrays in a single pass.
- Everything moved from `static` fields on `Program` to instance members on
  `AutoMusicEngine`, since the app now needs to start/stop/reconfigure it at runtime
  instead of running once and exiting.
- `Program.Main`'s one-shot window lookup became `MonitorWindowLoopAsync`, which
  keeps retrying every second so the app can sit in the tray before Genshin is even
  open, rather than requiring the game to already be running.

Detection logic (gate timing, blob scheduling, hold/release for blue notes) is
untouched — same tolerances, same offsets, same behavior.

## New pieces for the Windows app
| File | Purpose |
|---|---|
| `App.xaml` / `App.xaml.cs` | Shows the splash screen for 5s, then builds the tray icon. No window opens automatically. |
| `SettingsWindow.xaml(.cs)` | The window that opens when you click the tray icon. Status readout + editable tolerances/offsets + links. |
| `AppSettings.cs` | Thin wrapper around your `SettingsFile` for persisting the four tunables and the Auto Play toggle. |
| `AdminHelper.cs` | `WindowsPrincipal` elevation check for the Admin Mode indicator. |
| `GitHubUpdateChecker.cs` | Hits `GitHub`'s releases API and compares `tag_name` against the running assembly version. |

### One thing worth flagging
Rule 13 says never use JSON, but GitHub's releases API only returns JSON — that's
their contract, not something under our control. Rather than pull in
`System.Text.Json`, I did a plain string extraction of `tag_name` and `html_url`
out of the raw response body. It works for GitHub's stable response shape, but it's
not a real parser — if you'd rather just use `System.Text.Json` for this one
external-API case, say the word and I'll swap it in.

### The four status fields
- **Genshin Game**: raw foreground-window check.
- **Music Playing**: the existing pause-button detection, undebounced by the tray
  toggle — this is the "is a song actually running" signal.
- **Admin Mode**: elevation check; the window shows a red warning banner when false,
  since `SendInput` silently no-ops against an elevated game otherwise.
- **Auto Play Active**: the tray right-click toggle.

Key presses only fire when **both** Music Playing and Auto Play Active are true —
that's the "another check" you described, just applied where the presses actually
get scheduled rather than inside `GameActive()` itself, so Music Playing can still
report the true detection state independent of the toggle.

## Resources you still need to supply
- `Resources/GitHub.png` and `Resources/Sponsor.png` are generic placeholder
  circles I generated — I didn't use GitHub's real logo since it's trademarked.
  Drop in your own icons (or the official GitHub mark if you have a license to use
  it) whenever you're ready.
- `Resources/Kofi.png` and `Resources/Ward_Shield_Transparent_Background.png` are
  the files you uploaded, copied in as-is.
- `Resources/Ward_Shield_Transparent_Background.ico` was generated from your PNG
  for the tray icon (multi-size: 16–256px).

## Wiring up as a solution
This project references `..\WardShieldSplashWindow\WardShieldSplashWindow.csproj`
and `..\SettingsFile\SettingsFile.csproj` by relative path. Put all three project
folders as siblings:

```
/YourSolutionFolder
  /GenshinImpactAutoMusic
  /WardShieldSplashWindow
  /SettingsFile
```

or swap the `ProjectReference` entries in the `.csproj` for whatever path your repo
actually uses.

## GitHub repo assumption
`GitHubUpdateChecker` points at
`https://api.github.com/repos/WilliamW1979/GenshinImpactAutoMusic/releases/latest`,
matching the repo name you said you're going to create. Tag your releases like
`v1.0.0` so the version comparison works.

## Not verified by compiling
I don't have a Windows/.NET SDK environment here to build a WPF project against, so
this hasn't been compiled — I've been careful with the API surfaces (`SendInput`,
`DXGI`, `NotifyIcon`, `DispatcherTimer`, etc.) but you should do a build pass before
trusting it fully.
