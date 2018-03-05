@echo off


rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"
rem set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
call %msBuildDir%\MSBuild.exe  FAT.sln /t:rebuild /p:Configuration=UFHRelease /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=UFH.Application.Release.Build_LOG.log
set msBuildDir=

rem pause > nul