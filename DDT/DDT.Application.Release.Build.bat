@echo off

@setlocal enableextensions
@cd /d "%~dp0"

echo echo %cd%

set currentVersion=%1
echo %currentVersion%

rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
rem set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"

call APTAT.VersionChanger.exe %cd% %currentVersion%
call %msBuildDir%\nuget.exe restore DDT.sln
call %msBuildDir%\MSBuild.exe DDT.sln /t:rebuild /p:Configuration=Release /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=DDT.Application.Release.Build_LOG.log
set msBuildDir=

cd %cd%\DDT.Application\bin\Release
call "CreateSfx.bat"

rem pause > nul