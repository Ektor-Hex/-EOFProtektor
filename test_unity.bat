@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo === Probando EOFProtektor con UnityTestDoll.exe ===
echo.

echo Ejecutando protector con nivel avanzado (3) sin patch personalizado...
echo.

(
echo 3
echo N
) | bin\Debug\net6.0\EOFProtektor.exe "C:\Users\Ektor\Desktop\UnityTestDoll.exe"

echo.
echo === Prueba completada ===
pause