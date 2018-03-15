@echo off

@setlocal enableextensions
@cd /d "%~dp0"

echo %cd%

set xUnitDir="..\packages\xunit.runner.console.2.2.0\tools"

call %xUnitDir%\xunit.console.exe APTAT.Valkyrie.Tests\bin\UFHRelease\APTAT.Valkyrie.Tests.dll^
								  UFH.Application.Tests\bin\UFHRelease\UFH.Application.Tests.dll^
							      -xml UFH.Application.Tests.Run.Results.xml
								  
cd ..
echo %cd%
set xUnitDir="packages\xunit.runner.console.2.2.0\tools"

call %xUnitDir%\xunit.console.exe APTAT\APTAT.Core.UnitTests\bin\UFHRelease\APTAT.Core.UnitTests.dll -xml APTAT.Core.Tests.Run.Results.xml
									
set xUnitDir=

pause > nul