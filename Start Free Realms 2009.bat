@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start Free Realms 2009.ps1"
if errorlevel 1 (
  echo.
  echo Startup failed. See the message above.
  pause
)
