# AutoCounter Mod

## Description
AutoCounter is a mod for automating counteroffers in the game. It simplifies the process of responding to customer messages by automatically sending counteroffers based on configurable settings. !! Rounds Up to 5 !!

## Features
- **Automated Counteroffers**: Responds to customer messages every X seconds (configurable).
- **Customizable Deal Windows**: Choose from Morning, Afternoon, Night, or LateNight.
- **Dynamic or Fixed Pricing**: Use a fixed price per unit or calculate dynamically based on the customer's offer.
- **Easy Configuration**: Modify settings via the `AutoCounterConfig.cfg` file.

## Configuration
The mod uses a configuration file located at `UserData/AutoCounterConfig.cfg`. You can customize:
- `DealWindow`: The time window for deals (e.g., Morning, Afternoon, Night, LateNight(Default)).
- `AutoCounterInterval`: The interval (in seconds) for automatic counteroffers. Set to `-1` to disable. Press f4 for Manual. 5 Default
- `PricePerUnit`: Fixed price per unit. Leave as `-1` to calculate dynamically.
- `RoundTo`: Set Round Value (Default 5 (Jars), 1 to disable)
- `EnableCounter`: disable this to just accept the deals automaticly
- `HotKey`: HotKey F4 default.  (see [this](https://gist.github.com/Extremelyd1/4bcd495e21453ed9e1dffa27f6ba5f69) for other)
- `choosetimemanual`: choose the time yourself
- `GoToMaxSpend` : (experimental may work may not)
- `MinSuccessProbability`: probability ig

## How to Use
1. Install the mod using MelonLoader.
2. Upload mod in mod folder
3. Launch the game and enjoy automated counteroffers!

replace config after installing new version 

## Credits
Uses Code from:
- [Deal-Optimizer-Mod](https://github.com/xyrilyn/Deal-Optimizer-Mod) from xyrilyn
- Config-file code: [NanobotZ](https://github.com/NanobotZ)



first mod, prolly the worst code. suggestions would be good
