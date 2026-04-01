$buildPath = "./bin/Release"
$sourceFilename = "osu.Game.Rulesets.AuthlibInjection.source.dll"
$source = "$buildPath/$sourceFilename"
$output = "$buildPath/osu.Game.Rulesets.AuthlibInjection.dll"

try
{
    Write-Output "Running dotnet build..."
    dotnet build -c Release -o $buildPath

    # Rename the original build output for backup purposes
    if (Test-Path $source) {
        Remove-Item -Path $source
    }
    Rename-Item $output $sourceFilename

    dotnet tool restore

    Write-Output "Running ILRepack..."
    # Change the path if needed 
    $HarmonyPath = "$HOME/.nuget/packages/lib.harmony/2.4.1/lib/net8.0/0Harmony.dll"

    dotnet tool run ilrepack -out:$output `
    $source `
    $HarmonyPath `
    -lib:./fakelib `
    /internalize

    Write-Output "Build success"
    exit 0
}
catch
{
    Write-Output "Build failed"
    
    # Must present the exception
    throw
    exit 1
}
