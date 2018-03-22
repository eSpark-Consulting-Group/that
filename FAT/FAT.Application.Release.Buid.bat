@echo off

rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
rem set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
set msBuildDir="%PROGRAMFILES(X86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"

call %msBuildDir%\nuget.exe restore FAT.sln
call %msBuildDir%\MSBuild.exe FAT.sln /t:rebuild /p:Configuration=FATRelease /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=FAT.Application.Release.Build_LOG.log
set msBuildDir=

pause > nul