param(
    [string]$Configuration = "Release"
)

$projectPath = "IBNRCalculator.csproj"
$publishProfile = "Properties/PublishProfiles/Win-x64.pubxml"

Write-Host "Publishing $projectPath using profile $publishProfile in $Configuration mode..." -ForegroundColor Cyan

$publishArgs = @(
    "publish",
    $projectPath,
    "-p:PublishProfile=$publishProfile",
    "-c", $Configuration
)

# Invoke dotnet publish so users get a single-file, self-contained EXE under bin/Release/net10.0-windows10.0.19041.0/win-x64/publish/
dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed. Ensure the .NET SDK is installed and try again."
    exit $LASTEXITCODE
}

$publishDir = Join-Path -Path "bin" -ChildPath "${Configuration}/net10.0-windows10.0.19041.0/win-x64/publish"
$exePath = Join-Path -Path $publishDir -ChildPath "IBNRCalculator.exe"

Write-Host "Publish succeeded." -ForegroundColor Green
Write-Host "Executable path: $exePath"
Write-Host "Double-click the EXE to launch the UI, or run it from PowerShell with arguments (e.g., --console)."
