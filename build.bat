@echo off

"%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe" /p:Configuration=Release ".\ThemeParkResourceViewer.sln" /t:Rebuild
if %ERRORLEVEL% NEQ 0 pause

"%PROGRAMFILES%\7-Zip\7z" a -tzip "ThemeParkResourceViewer.zip" ^
 ".\ThemeParkResourceViewer\bin\Release\ThemeParkResourceViewer.exe" ^
 ".\ThemeParkResourceViewer\bin\Release\ThemeParkResourceViewer.exe.config" ^
 ".\ThemeParkResourceViewer\bin\Release\*.dll" ^
 "-mx=9"
if %ERRORLEVEL% NEQ 0 pause 