<p align="center">
  <img src="https://raw.githubusercontent.com/deltalazer/delta/experimental/assets/osudelta.png" width="180" alt="DeltaLazer logo" />
</p>

# Delta (DeltaLazer)

DeltaLazer is a community-driven fork of **osu!lazer**, made to be a playground for custom implementations with the lazer client!

Join our community discord!! (have fun and share stuff)
https://discord.gg/dfPwhRtGVZ

## What this fork adds (in simplest terms)

Modified In-game Logo

### (osu!standard only) 
Advanced Section-based Gimmicks and Hitobject-specific Attributes with editor UI

Such as:
- Stack Leniency, and Slider Tick Rate Overrides
- Dynamic Flashlight Mod Sizing
- Independent Beatmap Attribute Overrides (Including Approach Rate, Circle Size, and Overall Difficulty [Accuracy])

Manual control of HP with Advanced Judgment/Accuracy limits
Force-Apply Mods with deep customization [Like Difficulty Increase mods]
Fake hitobjects [Including hitcircles and sliders]
Extended **1000x** Slider Velocity Cap

# Which means you can do a lot of things!!!

## project base

- upstream project: https://github.com/ppy/osu
- this repository tracks and extends upstream osu!lazer behavior

## important server note

- **debug builds** connect to osu!dev server
- **release builds** connect to official osu! server

i **DO NOT** recommend building for release as this currently doesn't have systems in place that make it safe to use in the official osu! server

## quick build (desktop)

from repository root:

```bash
dotnet build osu.Desktop/osu.Desktop.csproj -c Debug
```

publish debug builds:

```bash
dotnet publish osu.Desktop/osu.Desktop.csproj -c Debug -r win-x64 --self-contained false -o ../builds/windows-debug-v2
dotnet publish osu.Desktop/osu.Desktop.csproj -c Debug -r linux-x64 --self-contained false -o ../builds/linux-debug-v2
```

## Bug Reports

If your issue is replicable in the standard client, Please [open an issue upstream instead.](https://github.com/ppy/osu/issues/new/choose)

Do not mention "Deltalazer" on your upstream bug report, as deltalazer (currently) has no affiliation with the osu!team. Doing so may be potentially disrespectful to them OR this fork.

If your issue is NOT replicable by the standard client, you may report the issue [here](https://github.com/deltalazer/delta/issues/new/choose), however keep in mind that:

1. This client is very experimental
2. Most issues are likely to already have a report, please check the Reports tab 

### Please include:

- Your beatmap file and exact section/hitobject settings used
- The expected behavior compared to the actual behavior
- Your client's crash logs/stack trace if available
- Your build type [Debug or Release]

## Contributing

Community contributions are welcome and encouraged!!

- Open an issue for any bug reports
- Open a discussion for feature and/or design proposals
- Open a pull request for any changes to the code base
For Developers: Please include tests where possible for regression safety

## License

This fork uses the MIT license, which matches the license of the upstream client (osu!lazer)

See `LICENCE` for more information

## Additional Resources

In-Game Asset Overrides: [delta/delta-resources](https://github.com/deltalazer/delta-resources/tree/master)

Please note: DeltaLazer has it's own branding guidelines!