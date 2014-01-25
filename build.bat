@echo off

rd /s/q ".\ThemeParkResourceViewer\bin\Release\net9.0-windows\"

dotnet build -c Release ".\ThemeParkResourceViewer.sln" 
if %ERRORLEVEL% NEQ 0 pause

"%PROGRAMFILES%\7-Zip\7z" a -tzip "ThemeParkResourceViewer.zip" ^
 ".\ThemeParkResourceViewer\bin\Release\net9.0-windows\*" ^
 "-mx=9"
if %ERRORLEVEL% NEQ 0 pause 