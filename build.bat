@echo off
echo ===============================================
echo    EOF Protektor - Script de Compilacion
echo ===============================================
echo.

echo Restaurando dependencias...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Error al restaurar dependencias
    pause
    exit /b 1
)

echo.
echo Compilando en modo Release...
dotnet build -c Release
if %ERRORLEVEL% neq 0 (
    echo Error en la compilacion
    pause
    exit /b 1
)

echo.
echo Publicando ejecutable independiente...
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
if %ERRORLEVEL% neq 0 (
    echo Error al publicar
    pause
    exit /b 1
)

echo.
echo ===============================================
echo    Compilacion completada exitosamente!
echo ===============================================
echo.
echo Archivos generados:
echo - bin\Release\net6.0\EOFProtektor.dll
echo - bin\Release\net6.0\win-x64\publish\EOFProtektor.exe
echo.
echo El ejecutable independiente esta en:
echo bin\Release\net6.0\win-x64\publish\EOFProtektor.exe
echo.
pause