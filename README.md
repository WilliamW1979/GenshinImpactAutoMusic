[Ko-Fi](https://ko-fi.com/williamw1979) - Please help support my work.<br>
[Github](https://github.com/WilliamW1979) - My current projects<br>
[Github Sponsor](https://github.com/sponsors/WilliamW1979) - If you enjoy my programs and want to help, please sponsor me.<br>
# Genshin Impact Auto Music

## How to Use

I wrote this program in 3 days because I was frustrated with another program that was written in Chinese that I couldn't read and get working, so I made my own version that worked ...

To use this program, all you need to do is run it (must be in Administrator Mode) and it will run in your System Tray (bottom right of the screen). Here is how it works ...

### System Tray Icon
> Right clicking allows you to access the menu to Activate / Deactivate it or close it out completely.
> Left clicking will open the settings window

### Settings Window
> Genshin Game: tells you if the game is in focus
> Music Playing: Detects is you are in performing mode
> Admin Mode: Tells you if this program is running in Administrator Mode
> Auto Play Active: Tells you if you have the Auto Playing active (Activate/Deactivate from System Tray menu)

> Gold Tolerance / Blue Tolerance: These settings allow you to adjust the color matching tolerance. The system pulls the pixel colors in RGB and the tolerance allows you to be off on each color by this amount to make a match. The higher the number, the better it will hit buttons but the higher the risk of hitting other colors you don't want to to hit. Playing with these numbers, the defaults worked pretty well for me but I wanted people to have the option to adjust these numbers just in case their graphics were different.
> Hit Offset: This adjust the detection zone for the buttons. A positive number moves it lower on the screen while a negative number moves it higher.
> Timing Offset: This is the ms offset for pressing keys. If you find you are hitting a lot of Good instead of Perfects, adjusting this will help.

## How it Works
> The program will watch for specific points in the game to see if the music program is active. Once it detects the program is active and all conditions are met, it will start reading the notes as they drop. Once they past a specific point, they will detect the note and fire a Task that we call Fire and Forget. This task times the hits for when they hit the line. The reason for this is because animations on the line itself can interfere with the program. The program reads the colors on the screen so it isn't intrusive at all. Settings will help adjusting for individual cases, the defaults are what worked for me when I ran my tests.

## Future Plans
> The blue buttons are not 100% hits. They work well enough to keep in the 90%+ range but it could use some fine tuning. I probably won't play with it for a while since being in the 90% is pretty decent already.
> I also considered adding more features to this like auto looting (pressing F when the option appeared on the screen automatically).
> If you have a suggestion, please feel free to suggest it through Github.