# Genshin Impact Auto Music

[![Ko-Fi](https://img.shields.io/badge/Ko-Fi-F16061?style=flat-square&logo=ko-fi&logoColor=white)](https://ko-fi.com/williamw1979)
[![GitHub Sponsors](https://img.shields.io/badge/GitHub_Sponsors-EA4AAA?style=flat-square&logo=github-sponsors&logoColor=white)](https://github.com/sponsors/WilliamW1979)
[![GitHub Projects](https://img.shields.io/badge/GitHub-181717?style=flat-square&logo=github&logoColor=white)](https://github.com/WilliamW1979)

An automated music performance tool for Genshin Impact, built as a reliable alternative to existing solutions.

---

## Prerequisites

* **Game Resolution:** Set Genshin Impact to **1920 x 1080** resolution.
* **Administrator Privileges:** The application must run with administrator rights to interact with the game client.

### Setting Permanent Administrator Mode

1. Right-click the executable (`.exe`) and select **Properties**.
2. Navigate to the **Compatibility** tab.
3. Check the box labeled **Run this program as an administrator**.
4. Click **Apply** and **OK**.

---

## Usage Guide

### System Tray Controls

* **Left-Click:** Opens the settings window.
* **Right-Click:** Opens the context menu to **Activate / Deactivate** auto-playing or close the application.

### Settings Window Reference

* **Genshin Game:** Indicates whether the game client is currently in focus.
* **Music Playing:** Detects when the in-game performance mode is active.
* **Admin Mode:** Verifies if the application is running with administrative privileges.
* **Auto Play Active:** Displays the current activation status of the automation.

### Configuration Parameters

* **Gold / Blue Tolerance:** Adjusts the RGB color-matching threshold. Higher values increase button-hit consistency at the risk of matching unintended colors. Default values work for most standard configurations.
* **Hit Offset:** Adjusts the vertical detection zone for buttons. Positive values shift the zone downward; negative values shift it upward.
* **Timing Offset:** Adjusts keypress timing in milliseconds to help fine-tune "Good" hits into "Perfects."

---

## How It Works

The program monitors specific screen regions to detect when the in-game music minigame is active. Once all conditions are met, it reads falling notes and utilizes non-intrusive screen color reading. Upon detecting a note past a specific threshold, it triggers an asynchronous "Fire and Forget" task that calculates the precise timing for the keypress, bypassing visual interference from line animations.

---

## Future Plans

* Refine blue note detection algorithms to improve hit rates beyond the current 90%+ baseline.
* Implement automated features, such as auto-looting (`F` key interaction).

> Contributions, bug reports, and feature requests are welcome via [GitHub](https://github.com/WilliamW1979).
