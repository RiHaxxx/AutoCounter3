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
- `DealWindow`: The time window for deals (e.g., Morning(0), Afternoon(1), Night(2), LateNight(3 Default)).
- `AutoCounterInterval`: The interval (in seconds) for automatic counteroffers. Set to `-1` to disable. Press f4 for Manual. 5 Default
- `PricePerUnit`: Fixed price per unit. Leave as `null` to calculate dynamically.
- 'RoundTo': Set Round Value (Default 5 (Jars), 1 to disable)
- 'EnableCounter': disable this to just accept the deals automaticly
- 'HotKey': HotKey 285 (f4) default.  (see this for other: https://gist.github.com/Extremelyd1/4bcd495e21453ed9e1dffa27f6ba5f69)
- 'choosetimemanual': choose the time yourself

## How to Use
1. Install the mod using MelonLoader.
2. Upload mod in mod folder
3. Launch the game and enjoy automated counteroffers!

replace config after installing new version 
