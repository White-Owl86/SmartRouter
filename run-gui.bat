@echo off
title Smart Anti-Sanction & Latency Router (GUI)
cd /d "%~dp0"

echo =======================================================================
echo   Smart Anti-Sanction & Latency Router (GUI)
echo   Starting Avalonia Desktop Interface...
echo =======================================================================

dotnet run --project src\SmartRouter.UI\SmartRouter.UI.csproj --configuration Release
pause
