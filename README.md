# StardewBot

A Stardew Valley mod that adds bots that can be programmed in Blockly

## Setup

Make sure you have submodules installed and updated.

```
git submodule init
git submodule update
```

Then copy `StardewBot/assets/config.example.json` to `config.json` and modify `"blocklyPath"` to point to the `Farmbot-Blockly/step-execution.html` file.

Make sure that you initialize the Farmbot-Blockly folder with npm

```
npm install
```

## Dependencies

Ensure all mod dependencies are in the mod folder:

> Content Patcher
https://www.nexusmods.com/stardewvalley/mods/1915
> Dynamic Game Assets
https://www.nexusmods.com/stardewvalley/mods/9365
> Mail Framework Mod
https://www.nexusmods.com/stardewvalley/mods/1536
> SpaceCore
https://www.nexusmods.com/stardewvalley/mods/1348
> (Optional) Skip Intro
https://www.nexusmods.com/stardewvalley/mods/533

Ensure that CsvHelper.dll 30.0.0 is installed in your projects bin folder. 
(Windows - (solution path)/StardewBot/bin/Debug/net5.0)