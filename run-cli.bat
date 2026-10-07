@echo off
title Smart Anti-Sanction & Latency Router (CLI)
cd /d "%~dp0"

echo =======================================================================
echo   Smart Anti-Sanction & Latency Router (CLI Daemon)
echo =======================================================================

dotnet run --project src\SmartRouter.Cli\SmartRouter.Cli.csproj --configuration Release -- %*
