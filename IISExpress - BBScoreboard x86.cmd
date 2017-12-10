@echo off

set BBScoreboardPath=%cd%
echo %BBScoreboardPath%

if exist "%ProgramFiles(x86)%" (
	REM "_IISExpress - Portal x86.lnk"
	CD /D "%ProgramFiles(x86)%\IIS Express"
) else (
	REM "_IISExpress - Portal.lnk"
	CD /D "%ProgramFiles%\IIS Express"
)

iisexpress.exe /config:%BBScoreboardPath%\IISExpress\applicationhost.config /siteid:1