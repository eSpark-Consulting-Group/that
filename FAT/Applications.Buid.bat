@echo off

rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set msBuildDir="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
rem call %msBuildDir%\MSBuild.exe  FAT.sln /p:Configuration=Release /p:Platform=Any CPU /l:FileLogger,Microsoft.Build.Engine;logfile=Applications.Build_LOG.log
call %msBuildDir%\MSBuild.exe  FAT.sln /p:Configuration=Release /p:Platform="Any CPU" /l:FileLogger,Microsoft.Build.Engine;logfile=Applications.Build_LOG.log
set msBuildDir=

rem pause > nul