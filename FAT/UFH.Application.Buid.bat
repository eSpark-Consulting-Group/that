@echo off

rem set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
set msBuildDir="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin"
call %msBuildDir%\MSBuild.exe  UFH.Application\UFH.Application.csproj /p:Configuration=Debug /p:Platform=AnyCPU /l:FileLogger,Microsoft.Build.Engine;logfile=UFH_Manual_MSBuild_ReleaseVersion_LOG.log
set msBuildDir=

pause > nul