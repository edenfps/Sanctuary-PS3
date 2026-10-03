param(
    [switch]$SkipLaunch,
    [int]$WaitForWindowSeconds = 300,
    [int]$WaitForGatewaySeconds = 300,
    [int]$XHoldMilliseconds = 300
)

$ErrorActionPreference = 'Stop'
$rpcs3Path = 'C:\Users\bobya\Downloads\rpcs3\rpcs3.exe'
$elfPath = 'D:\RPCS3\dev_hdd0\game\NPUA30048\USRDIR\game.elf'
$gatewayLogDir = Join-Path $PSScriptRoot '..\src\bin\Debug'

Add-Type @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class Ps3WindowInput {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr extra);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int command);
    [DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extra);
    public static IntPtr FindGameWindow(uint processId) {
        IntPtr found = IntPtr.Zero;
        EnumWindows((hWnd, extra) => {
            uint owner;
            GetWindowThreadProcessId(hWnd, out owner);
            if (owner != processId || !IsWindowVisible(hWnd)) return true;
            var title = new StringBuilder(512);
            GetWindowText(hWnd, title, title.Capacity);
            if (!title.ToString().Contains("Free Realms [NPUA30048]")) return true;
            found = hWnd;
            return false;
        }, IntPtr.Zero);
        return found;
    }
}
'@

$gatewayLog = Get-ChildItem -LiteralPath $gatewayLogDir -Filter 'PS3.Gateway.*.stdout.log' |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (!$gatewayLog) { throw 'No PS3 gateway log was found.' }
$gatewayLogPath = $gatewayLog.FullName
$gatewayLogBaseline = $gatewayLog.Length

if ($SkipLaunch) {
    $process = Get-Process rpcs3 -ErrorAction Stop | Select-Object -First 1
} else {
    if (!(Test-Path -LiteralPath $rpcs3Path) -or !(Test-Path -LiteralPath $elfPath)) {
        throw 'RPCS3 or NPUA30048 game.elf is missing.'
    }
    # RPCS3 keeps its main process open after Stop. A second copy fails to boot.
    $old = @(Get-Process rpcs3 -ErrorAction SilentlyContinue)
    foreach ($item in $old) { [void]$item.CloseMainWindow() }
    $closeDeadline = [DateTime]::UtcNow.AddSeconds(8)
    while ((Get-Process rpcs3 -ErrorAction SilentlyContinue) -and [DateTime]::UtcNow -lt $closeDeadline) {
        Start-Sleep -Milliseconds 250
    }
    $stillRunning = @(Get-Process rpcs3 -ErrorAction SilentlyContinue)
    if ($stillRunning.Count) {
        throw 'An earlier RPCS3 process is still running; close it before launching another test.'
    }
    $process = Start-Process -FilePath $rpcs3Path -ArgumentList ('"' + $elfPath + '"') -WorkingDirectory (Split-Path $rpcs3Path) -WindowStyle Normal -PassThru
}

$deadline = [DateTime]::UtcNow.AddSeconds($WaitForWindowSeconds)
do {
    $gameWindow = [Ps3WindowInput]::FindGameWindow([uint32]$process.Id)
    if ($gameWindow -ne [IntPtr]::Zero) { break }
    Start-Sleep -Milliseconds 500
} while ([DateTime]::UtcNow -lt $deadline -and !$process.HasExited)

if ($gameWindow -eq [IntPtr]::Zero) { throw 'The NPUA30048 game window did not appear.' }
$loginDeadline = [DateTime]::UtcNow.AddSeconds($WaitForGatewaySeconds)
$attempts = 0
do {
    $process.Refresh()
    if ($process.HasExited) { throw 'RPCS3 exited before gateway connection.' }

    # A connection means Cross reached the character prompt. Stop pressing it.
    $log = [IO.File]::Open($gatewayLogPath, 'Open', 'Read', 'ReadWrite')
    try {
        if ($log.Length -gt $gatewayLogBaseline) {
            $log.Position = $gatewayLogBaseline
            $reader = [IO.StreamReader]::new($log)
            $newText = $reader.ReadToEnd()
            if ($newText -match 'Sanctuary\.Gateway\.GatewayServer\|127\.0\.0\.1:\d+ connected\.') {
                Write-Output "Character login reached the PS3 gateway after $attempts X press(es)."
                return
            }
        }
    } finally { $log.Dispose() }

    [Ps3WindowInput]::ShowWindow($gameWindow, 9) | Out-Null
    # Windows restricts background processes from stealing focus. An Alt tap
    # allows this test runner to focus the visible game window explicitly.
    [Ps3WindowInput]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
    [Ps3WindowInput]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
    [Ps3WindowInput]::SetForegroundWindow($gameWindow) | Out-Null
    Start-Sleep -Milliseconds 200
    if ([Ps3WindowInput]::GetForegroundWindow() -ne $gameWindow) {
        Start-Sleep -Seconds 1
        continue
    }
    # RPCS3 global keyboard profile maps the PS3 Cross button to X.
    [Ps3WindowInput]::keybd_event(0x58, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds $XHoldMilliseconds
    [Ps3WindowInput]::keybd_event(0x58, 0, 2, [UIntPtr]::Zero)
    $attempts++
    Start-Sleep -Seconds 2
} while ([DateTime]::UtcNow -lt $loginDeadline)
throw "No PS3 gateway connection after $attempts X press(es)."
