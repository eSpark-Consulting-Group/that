@echo off

@setlocal enableextensions
@cd /d "%~dp0"

echo %cd%

set xUnitDir="..\packages\xunit.runner.console.2.2.0\tools"

call %xUnitDir%\xunit.console.exe APTAT.Valkyrie.Tests\bin\FATRelease\APTAT.Valkyrie.Tests.dll^
								  FAT.Application.Tests\bin\FATRelease\FAT.Application.Tests.dll^
							      -xml FAT.Application.Tests.Run.Results.xml
								  
cd ..
echo %cd%
set xUnitDir="packages\xunit.runner.console.2.2.0\tools"

call %xUnitDir%\xunit.console.exe APTAT\APTAT.Core.UnitTests\bin\FATRelease\APTAT.Core.UnitTests.dll -xml APTAT.Core.Tests.Run.Results.xml
									
set xUnitDir=

pause > nul