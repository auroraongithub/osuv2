#!/usr/bin/env bash

buildPath="./bin/Release"
source="$buildPath/osu.Game.Rulesets.AuthlibInjection.source.dll"
output="$buildPath/osu.Game.Rulesets.AuthlibInjection.dll"

echo "Running dotnet build..."
dotnet build -c Release -o $buildPath
if [ $? -ne 0 ]; then
    echo "Build failed"
    exit 1
fi

# Rename the original build output for backup purposes
mv $output $source

echo "Restoring tools..."
dotnet tool restore

echo "Running ILRepack..."
# Change the path if needed
HarmonyPath="$HOME/.nuget/packages/lib.harmony/2.4.1/lib/net8.0/0Harmony.dll"
dotnet tool run ilrepack -out:"$output" \
    "$source" \
    "$HarmonyPath" \
    -lib:./fakelib \
    /internalize
