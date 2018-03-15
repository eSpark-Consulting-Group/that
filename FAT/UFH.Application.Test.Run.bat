@echo off

set msBuildDir="..\packages\xunit.runner.console.2.2.0\tools"

@setlocal enableextensions
@cd /d "%~dp0"

echo %cd%
call %msBuildDir%\xunit.console.exe APTAT.Valkyrie.Tests\bin\UFHRelease\APTAT.Valkyrie.Tests.dll^
								    UFH.Application.Tests\bin\UFHRelease\UFH.Application.Tests.dll^
							        -xml UFH.Application.Tests.Run.Results.xml
									
set msBuildDir=

pause > nul