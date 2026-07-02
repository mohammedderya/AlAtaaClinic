param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = "publish/Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $root

Write-Host "Restoring packages..."
dotnet restore AlAtaaClinic.sln

Write-Host "Building $Configuration..."
dotnet build AlAtaaClinic.sln -c $Configuration --no-restore

Write-Host "Publishing to $OutputPath..."
dotnet publish "src/AlAtaaClinic.Desktop/AlAtaaClinic.Desktop.csproj" `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -p:PublishReadyToRun=true `
    -o $OutputPath

Write-Host "Publish completed: $((Resolve-Path $OutputPath).Path)"
Write-Host "Next step: compile installer/AlAtaaClinic.iss with Inno Setup 6."
