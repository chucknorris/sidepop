@echo off

::Project UppercuT - http://uppercut.googlecode.com

if '%2' NEQ '' goto usage
if '%3' NEQ '' goto usage
if '%1' == '/?' goto usage
if '%1' == '-?' goto usage
if '%1' == '?' goto usage
if '%1' == '/help' goto usage

SET APP_BDDDOC="..\lib\bdddoc\bdddoc.console.exe"
SET TEST_ASSEMBLY_NAME="sidepop.tests.dll"

SET DIR=%~d0%~p0%

call "%DIR%test.bat"

if %ERRORLEVEL% NEQ 0 goto errors

%NANT% /f:.\build.custom\bdddoc.test.step  -D:build.config.settings=%build.config.settings% -D:app.bdddoc=%APP_BDDDOC% -D:test_assembly=%TEST_ASSEMBLY_NAME%
%NANT% /f:.\build.custom\bdddoc.test.step open_results

if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:errors
EXIT /B %ERRORLEVEL%

:finish