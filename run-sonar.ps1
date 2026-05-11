<#
.SYNOPSIS
    Run SonarQube analysis for the EatOClock .NET backend.

.DESCRIPTION
    1. Begins SonarQube scan (dotnet-sonarscanner begin)
    2. Builds the solution
    3. Runs all NUnit tests with OpenCover coverage export
    4. Ends the scan and uploads results

.PREREQUISITES
    - dotnet SDK 8  (dotnet --version)
    - SonarQube server running locally on http://localhost:9000
      OR change $SonarUrl below to your remote server
    - dotnet-sonarscanner tool:
        dotnet tool install --global dotnet-sonarscanner
    - A SonarQube token (generate at http://localhost:9000 → My Account → Security)

.USAGE
    .\run-sonar.ps1 -Token "your-token-here"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Token,

    [string]$SonarUrl    = "http://localhost:9000",
    [string]$ProjectKey  = "eatoclock-backend",
    [string]$ProjectName = "EatOClock Backend"
)

$ErrorActionPreference = "Stop"
$SolutionDir = $PSScriptRoot          # same folder as EatOClock.sln
$TestProject = "$SolutionDir\Tests\EatOClock.Tests\EatOClock.Tests.csproj"
$CoverageReport = "$SolutionDir\Tests\EatOClock.Tests\TestResults\coverage.opencover.xml"

Write-Host "`n=== Step 1: SonarQube Begin ===" -ForegroundColor Cyan
dotnet sonarscanner begin `
    /k:"$ProjectKey" `
    /n:"$ProjectName" `
    /d:sonar.host.url="$SonarUrl" `
    /d:sonar.token="$Token" `
    /d:sonar.cs.opencover.reportsPaths="$CoverageReport" `
    /d:sonar.exclusions="**/Migrations/**,**/bin/**,**/obj/**,**/*.Designer.cs"

if ($LASTEXITCODE -ne 0) { throw "SonarScanner Begin failed." }

Write-Host "`n=== Step 2: Build Solution ===" -ForegroundColor Cyan
dotnet build "$SolutionDir\EatOClock.sln" --configuration Release --no-restore

if ($LASTEXITCODE -ne 0) { throw "Build failed." }

Write-Host "`n=== Step 3: Run Tests + Generate Coverage ===" -ForegroundColor Cyan
dotnet test "$TestProject" `
    --configuration Release `
    --no-build `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=opencover `
    /p:CoverletOutput="$SolutionDir\Tests\EatOClock.Tests\TestResults\coverage.opencover.xml" `
    --logger "trx;LogFileName=$SolutionDir\Tests\EatOClock.Tests\TestResults\TestResults.trx"

if ($LASTEXITCODE -ne 0) { throw "Tests failed. Fix failing tests before publishing Sonar results." }

Write-Host "`n=== Step 4: SonarQube End (upload results) ===" -ForegroundColor Cyan
dotnet sonarscanner end /d:sonar.token="$Token"

if ($LASTEXITCODE -ne 0) { throw "SonarScanner End failed." }

Write-Host "`n✅  SonarQube analysis complete!" -ForegroundColor Green
Write-Host "    Open $SonarUrl/dashboard?id=$ProjectKey to view the report." -ForegroundColor Green
