# AutomaticBeaverMigration
For those colony builders that would like to streamline the migration proces, there now is AutomaticBeaverMigration.
This plugin adds functionality to Districts so that they automatically migrate your beavers to a desired amount. This plugins hopes to better optimise how you use your districts.

## Disclaimer
This plugin is currently only supported in the **experimental** version of Timberborn.

## How to use
The will add a simple UI fragment on the District UI. Example image below.

![DistrictUI](https://github.com/TobbyTheBobby/TobbertMods/blob/main/AutomaticBeaverMigration/attachments/DistrictCenterUI.png?raw=true)

### Main toggle
The toggle at the top lets you enable or disable the automatic migration in case of emergency.

### Priority
The priority buttons indicate the priority. The higher priority districts will be first to get beavers from other districts.

### Setting the desired amount of beavers
You can set the amount of beavers you desire per type.
* If the district has more beaver than it desires, it will try to migrate them to a district that has less beavers than it desires.
* If a district has less beavers than it desires, it will try to receive beavers based on the priority.
* In case all districts have more beavers than they desires, nothing will happen and all beavers will stay in their district.

### Force migration
There is a button to force the migration based on the desired beavers in all districts if you ever need it :D

## Installing
Recommended way to install this mod is through [Thunderstore](https://timberborn.thunderstore.io/). You can install this plugin manually by cloning the repo, building it
and adding the dll to your bepinex plugins folder. This plugin is dependent on the magnificent [TimberAPI](https://github.com/Timberborn-Modding-Central/TimberAPI).

## Problems?
In case you experience problems, message me in modding channel of the the Timberborn discord or message me directly. I will try to fix it as soon as possible. :D

## Changelog

### 0.1.0 - 24.6.2022
- Released the plugin.