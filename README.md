# EOF Protektor - Anti-Tamper Protection

Un protector avanzado de archivos ejecutables .NET que implementa técnicas anti-tamper y anti-debug sofisticadas.

## Características

- **Protección Anti-Tamper**: Verificación de integridad del archivo
- **Anti-Debug**: Detección y prevención de debuggers
- **Anti-Dump**: Protección contra volcado de memoria
- **Patches Personalizados**: Inyección de código personalizado
- **Múltiples Niveles**: Básico, Intermedio y Avanzado
- **Interfaz Interactiva**: Fácil de usar con menús guiados

## Requisitos

- .NET 6.0 o superior
- Windows (recomendado)
- Archivos ejecutables .NET válidos

## Compilación

```bash
# Clonar o descargar el proyecto
cd EOFProtektor

# Restaurar dependencias
dotnet restore

# Compilar el proyecto
dotnet build -c Release

# O compilar directamente
dotnet publish -c Release -r win-x64 --self-contained
```

## Uso

### Modo Interactivo (Recomendado)

```bash
# Ejecutar sin parámetros para modo interactivo
EOFProtektor.exe
```

El programa te guiará a través de:
1. Selección del archivo a proteger
2. Nivel de protección deseado
3. Opción de agregar patches personalizados

### Modo Línea de Comandos

```bash
# Proteger un archivo específico
EOFProtektor.exe "ruta\al\archivo.exe"
```

## Niveles de Protección

### 1. Básico (Rápido)
- Validación básica de integridad
- Verificación de marcadores principales
- Tiempo de procesamiento mínimo

### 2. Intermedio (Recomendado)
- Incluye protección básica
- Anti-debug avanzado
- Detección de herramientas de análisis
- Anti-dump con verificación de memoria

### 3. Avanzado (Máxima Protección)
- Incluye protección intermedia
- Checkpoints distribuidos en el código
- Trampas para herramientas de bypass
- Máxima resistencia a análisis

## Patches Personalizados

### Opción 1: Bytes Hexadecimales
Ingresa bytes directamente en formato hex:
```
Ejemplo: 48 8B C4 48 89 58 08 48 89 70 10
```

### Opción 2: Archivo de Patch
Carga bytes desde un archivo:
```
Ejemplo: C:\patches\mi_patch.bin
```

## Estructura del Proyecto

```
EOFProtektor/
├── EOFProtektor.csproj    # Archivo de proyecto
├── program.cs             # Código principal
└── README.md             # Este archivo
```

## Dependencias

- **dnlib**: Biblioteca para manipulación de archivos .NET
- **System.Security.Cryptography**: Para funciones de hash y criptografía

## Advertencias

⚠️ **Importante**: 
- Siempre haz una copia de seguridad de tus archivos antes de protegerlos
- Algunos antivirus pueden detectar falsamente el archivo protegido
- La protección puede aumentar el tamaño del archivo
- Prueba el archivo protegido antes de distribuirlo

## Limitaciones

- Solo funciona con archivos ejecutables .NET
- Requiere que el archivo original sea válido
- La protección avanzada puede afectar el rendimiento

## Solución de Problemas

### Error: "El archivo no existe"
- Verifica que la ruta del archivo sea correcta
- Asegúrate de que el archivo tenga permisos de lectura

### Error: "No se pudo cargar el módulo"
- El archivo puede estar corrupto
- Verifica que sea un ejecutable .NET válido
- Intenta con un archivo diferente

### Error de compilación
- Verifica que tengas .NET 6.0 instalado
- Ejecuta `dotnet restore` para restaurar dependencias
- Revisa que todas las referencias estén disponibles

## Licencia

Este proyecto es de código abierto. Úsalo bajo tu propia responsabilidad.

## Contribuciones

Las contribuciones son bienvenidas. Por favor:
1. Haz un fork del proyecto
2. Crea una rama para tu característica
3. Envía un pull request

---

**Nota**: Este software está diseñado para propósitos educativos y de protección legítima. No lo uses para actividades maliciosas.