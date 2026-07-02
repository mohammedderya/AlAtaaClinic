# Download SQL Server Express installer
$downloadUrl = "https://go.microsoft.com/fwlink/?linkid=2215260"
$outputPath = "$PSScriptRoot\redist\SQL2022-SSEI-Expr.exe"

Write-Host "Downloading SQL Server Express installer..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $outputPath -UseBasicParsing

if (Test-Path $outputPath) {
    Write-Host "Download completed: $outputPath"
    $size = (Get-Item $outputPath).Length / 1MB
    Write-Host "Size: $([math]::Round($size, 2)) MB"
} else {
    Write-Host "Download failed!"
    exit 1
}
