@echo off


@setlocal enableextensions
@cd /d "%~dp0"

echo echo %cd%

set currentVersion=%1
echo currentVersion

rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
rem set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"

call APTAT.VersionChanger.exe ../../UFH_build %currentVersion%
call %msBuildDir%\nuget.exe restore FAT.sln
call %msBuildDir%\MSBuild.exe FAT.sln /t:rebuild /p:Configuration=UFHRelease /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=UFH.Application.Release.Build_LOG.log
set msBuildDir=

cd %cd%\UFH.Application\bin\UFHRelease
call "buildScript.bat"

rem pause > nul

