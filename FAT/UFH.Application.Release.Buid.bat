@echo off


rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
rem set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"

REM REM @echo off
REM REM for /d %%a in ("%programfiles(x86)%\Microsoft Visual Studio*") do (
REM REM for /f "tokens=3 delims=\" %%x in ("%%a") do echo %%x
REM REM )
@setlocal enableextensions

for /f "usebackq tokens=1* delims=: " %%a in (`!vswbin! -latest -requires Microsoft.Component.MSBuild`) do (
    if /i "%%a"=="installationPath" set vspath=%%b
    if /i "%%a"=="installationVersion" set vsver=%%b
)
...
for /f "tokens=1,2 delims=." %%a in ("!vsver!") do (
    set vsver=%%a.%%b
)
set msbuildPath=!vspath!\MSBuild\!vsver!\Bin

echo %msbuildPath%

pause > nul

set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"

call %msBuildDir%\nuget.exe restore FAT.sln
call %msBuildDir%\MSBuild.exe FAT.sln /t:rebuild /p:Configuration=UFHRelease /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=UFH.Application.Release.Build_LOG.log
set msBuildDir=s

rem pause > nul

