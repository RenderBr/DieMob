# DieMob Plugin (TShock V5)
This plugin allows you to define regions on your TShock server where mobs will be auto-killed when they enter the "diemob region".

## Installation

1. Download the plugin from the GitHub releases page.
2. Place the plugin in your TShock root folder.
3. Restart the server.

## Usage

1. Firstly define your region using TShock's built in region system. If you are unsure how this is done, [visit this guide](https://tshock.readme.io/docs/basic-region-management).
2. Once defined, simply add it to Diemob's database with /diemob add <tshock region name>
3. Profit! 
  
## Commands

* `/diemob create/add <name>` - Marks a TShock region as a Diemob region.
* `/diemob list` - List all defined regions.
* `/diemob del/remove <name>` - Removes specified TShock region from Diemob.

## Permissions

* `diemob.use` - Permission to use the `/diemob` command.

## Notes

* The plugin does not work with mods that change the mob spawning mechanics.

## Contact

If you have any questions or feedback, please contact me.
