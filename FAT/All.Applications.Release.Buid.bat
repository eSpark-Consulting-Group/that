@echo off

rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
rem set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"
rem call %msBuildDir%\MSBuild.exe  FAT.sln /p:Configuration=Release /p:Platform=Any CPU /l:FileLogger,Microsoft.Build.Engine;logfile=Applications.Build_LOG.log
call %msBuildDir%\MSBuild.exe  FAT.sln /t:rebuild /p:Configuration=Release /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=All.Applications.Release.Build_LOG.log
set msBuildDir=

rem pause > nul