$ErrorActionPreference = 'Stop'

$workspace = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$server = Join-Path $workspace 'server-2009-work'
$bin = Join-Path $server 'src\bin\Debug'
$client = Join-Path $workspace 'client-2009-test'
$gameExe = Join-Path $client 'FreeRealms.exe'
$assetScript = Join-Path $server 'serve_2009_assets.py'
$geometryPatch = Join-Path $server 'client-patches\patch_2009_geometry_zero_divisor.py'

function Test-LocalPort([int]$Port) {
    if ($Port -in @(20042, 20260)) {
        return [bool](Get-NetUDPEndpoint -LocalPort $Port -ErrorAction SilentlyContinue)
    }
    return [bool](Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
}

function Wait-LocalPort([int]$Port, [int]$Seconds) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-LocalPort $Port) { return }
        Start-Sleep -Milliseconds 250
    }
    throw "Service did not start on 127.0.0.1:$Port. Check logs in $bin."
}

if (-not (Test-Path -LiteralPath $gameExe)) { throw "Missing client: $gameExe" }
if (-not (Test-Path -LiteralPath $assetScript)) { throw "Missing asset server: $assetScript" }

$running = Get-CimInstance Win32_Process -Filter "Name = 'FreeRealms.exe'" | Where-Object { $_.ExecutablePath -eq $gameExe }
if ($running) {
    $current = Get-Process -Id $running.ProcessId -ErrorAction SilentlyContinue
    if ($current -and $current.Responding -and $current.MainWindowHandle -ne 0) {
        Add-Type -AssemblyName Microsoft.VisualBasic
        [Microsoft.VisualBasic.Interaction]::AppActivate([int]$current.Id)
        Write-Host "2009 client was already running; brought PID $($current.Id) to the front."
        exit 0
    }
    Write-Host 'Closing an unresponsive 2009 client before relaunching...'
    $current | Stop-Process -Force -ErrorAction SilentlyContinue
}

& python.exe $geometryPatch
if ($LASTEXITCODE -ne 0) { throw 'Failed to apply the 2009 geometry guard.' }

if (-not (Test-LocalPort 20043)) {
    $python = (Get-Command python.exe -ErrorAction Stop).Source
    Write-Host 'Starting 2009 asset server...'
    Start-Process -FilePath $python -ArgumentList @('serve_2009_assets.py') -WorkingDirectory $server -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $bin 'Assets.launch.stdout.log') `
        -RedirectStandardError (Join-Path $bin 'Assets.launch.stderr.log') | Out-Null
    Wait-LocalPort 20043 15
}

Copy-Item -LiteralPath (Join-Path $server 'src\Sanctuary.WebAPI\appsettings.json') -Destination (Join-Path $bin 'appsettings.json') -Force

$gameServices = Get-Process -Name Sanctuary.Login,Sanctuary.Gateway -ErrorAction SilentlyContinue
if ($gameServices) {
    Write-Host 'Clearing the previous game session...'
    $gameServices | Stop-Process -Force
    $gameServices | Wait-Process -ErrorAction SilentlyContinue
}

$services = @(
    @{ Name='WebAPI'; Port=20040 },
    @{ Name='Login'; Port=20042 },
    @{ Name='Gateway'; Port=20260 }
)
foreach ($service in $services) {
    if (Test-LocalPort $service.Port) { continue }
    $exe = Join-Path $bin ('Sanctuary.' + $service.Name + '.exe')
    if (-not (Test-Path -LiteralPath $exe)) { throw "Missing server executable: $exe. Build src\Sanctuary.sln first." }
    Get-Process -Name ('Sanctuary.' + $service.Name) -ErrorAction SilentlyContinue | Stop-Process -Force
    Write-Host "Starting Sanctuary $($service.Name)..."
    Start-Process -FilePath $exe -WorkingDirectory $bin -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $bin ("Sanctuary.$($service.Name).launch.stdout.log")) `
        -RedirectStandardError (Join-Path $bin ("Sanctuary.$($service.Name).launch.stderr.log")) | Out-Null
}
foreach ($service in $services) { Wait-LocalPort $service.Port 20 }

$arguments = @(
    'inifile=ClientConfig.ini',
    'Guid=1',
    'Server=127.0.0.1:20260',
    'Ticket=Eden',
    'Internationalization:Locale=8',
    'ShowMemberLoadingScreen=0',
    'Country=US',
    'key=m80HqsRO9i4PjJSCOasVMg==',
    'CasSessionId=Jk6TeiRMc4Ba38NO'
)
Write-Host 'Starting Free Realms 2009 as Eden...'
$process = Start-Process -FilePath $gameExe -WorkingDirectory $client -ArgumentList $arguments -PassThru
Write-Host "Client started (PID $($process.Id))."
