rem @echo off

echo Starting post build batch file: %~nx0
echo.

setlocal enableextensions

set TargetName=%~1
set TargetPath=%~2
set ProjectName=%~3
set ProjectPath=%~4
set SrcDir=%ProjectPath%plgx\%TargetName%

echo TargetPath  = %TargetPath%
echo ProjectPath = %ProjectPath%
echo SrcDir      = %SrcDir%
echo.

rem The plgx file name will be the same as the name of the directory containing the source
if exist "%SrcDir%.plgx" del /q /f "%SrcDir%.plgx"
if exist "%SrcDir%" rd /s /q "%SrcDir%"

md "%SrcDir%"

copy /y "%ProjectPath%%ProjectName%.csproj" "%SrcDir%\%ProjectName%.csproj"
xcopy /y /s /i "%ProjectPath%Source" "%SrcDir%\Source"
xcopy /y /s /i "%ProjectPath%Resources" "%SrcDir%\Resources"
xcopy /y /s /i "%ProjectPath%Properties" "%SrcDir%\Properties"
 
start /wait "" "%TargetPath%KeePass.exe" --plgx-create --plgx-prereq-net:4.8 --plgx-prereq-kp:2.41 "%SrcDir%"

rd /s /q "%SrcDir%"

echo finished...