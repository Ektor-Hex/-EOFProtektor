# EOF Protektor v2.0 - Advanced .NET Protector

Un protector avanzado de archivos ejecutables .NET que implementa técnicas anti-tamper, anti-debug y ofuscación sofisticadas.

## 🎯 Características Principales

- **Protección Anti-Tamper**: Verificación de integridad del archivo con múltiples capas
- **Anti-Debug**: Detección y prevención de debuggers
- **Anti-Dump**: Protección contra volcado de memoria
- **Control Flow Obfuscation**: Ofuscación extrema del flujo de control
- **Class Virtualization**: Virtualización dinámica de clases completas
- **Hide Main**: Ocultación del punto de entrada real
- **Patches Personalizados**: Inyección de código personalizado en EOF
- **Múltiples Niveles**: Básico, Intermedio y Avanzado
- **Interfaz Gráfica y CLI**: Fácil de usar con GUI o línea de comandos

## 📋 Requisitos

- **.NET 6.0** o superior
- **Windows** (recomendado para GUI)
- **dnlib 4.5.0** (incluido automáticamente vía NuGet)
- Archivos ejecutables .NET válidos

## 🏗️ Arquitectura Modular

```
EOFProtektor/
├── Core/
│   └── ProtectionData.cs          # Configuración y datos de protección
├── Utils/
│   └── NameObfuscator.cs          # Ofuscación de nombres
├── Protection/
│   ├── AntiDebugProtection.cs     # Protecciones anti-debug
│   └── IntegrityProtection.cs     # Protecciones de integridad
├── Obfuscation/
│   ├── ControlFlowObfuscator.cs   # Control Flow Obfuscation extremo
│   ├── HideMainObfuscator.cs      # Ocultación del punto de entrada
│   └── ClassVirtualizationObfuscator.cs  # Virtualización de clases
├── Program_Protected.cs           # Punto de entrada modular
├── program.cs                     # Versión monolítica (legacy)
├── ProtectionConfigForm.cs        # Interfaz gráfica
└── EOFProtektor.csproj           # Configuración del proyecto
```

## 🚀 Compilación

```bash
# Restaurar dependencias
dotnet restore

# Compilar en modo Release
dotnet build -c Release

# Publicar ejecutable independiente
dotnet publish -c Release -r win-x64 --self-contained
```

## 💻 Uso

### Modo GUI (Recomendado)

```bash
# Ejecutar sin parámetros para abrir interfaz gráfica
EOFProtektor.exe
```

### Modo Línea de Comandos

```bash
# Proteger un archivo específico (modo interactivo)
EOFProtektor.exe "ruta\al\archivo.exe"

# Con opciones avanzadas
EOFProtektor.exe --protection-level 3 --virtualize-all archivo.exe

# Sin Control Flow Obfuscation
EOFProtektor.exe --no-controlflow archivo.exe

# Ayuda
EOFProtektor.exe --help
```

## 🔐 Niveles de Protección

### Nivel 1 - Básico
- ✅ Validación básica de integridad
- ✅ Verificación de marcadores principales
- ✅ Tiempo de procesamiento mínimo
- 📊 Ideal para: Desarrollo y pruebas

### Nivel 2 - Intermedio (Recomendado)
- ✅ Incluye protección básica
- ✅ Anti-debug avanzado
- ✅ Detección de herramientas de análisis
- ✅ Anti-dump con verificación de memoria
- 📊 Ideal para: Distribución general

### Nivel 3 - Avanzado
- ✅ Incluye protección intermedia
- ✅ **Control Flow Obfuscation EXTREMO**
  - 50 dispatchers caóticos
  - 150 métodos de estado falsos
  - 100 métodos de confusión
  - 200 métodos de ruido extremo
- ✅ **Hide Main Obfuscation**
  - 300 métodos Main falsos
  - 50 dispatchers de entrada
  - 100 clases falsas con Main
- ✅ Checkpoints distribuidos en el código
- ✅ Trampas para herramientas de bypass
- ✅ Protección del constructor del módulo
- 📊 Ideal para: Software crítico y alta seguridad

## 🎨 Opciones Avanzadas

### Control Flow Obfuscation
Ofusca el flujo de control de los métodos insertando:
- Dispatchers caóticos con cálculos complejos
- Métodos de estado falsos
- Laberintos de saltos aparatosos
- Operaciones de ruido extremo

### Virtualización de Clases
Convierte métodos en llamadas indirectas a través de un dispatcher virtual:
- Tabla virtual de métodos
- Anti-análisis en cada llamada
- Redirección completa de flujo

### Ocultación de Main
Oculta el verdadero punto de entrada:
- Crea cientos de métodos Main falsos
- Red compleja de redirecciones
- Clases falsas con puntos de entrada sintéticos

## 📦 Patches Personalizados

### Opción 1: Bytes Hexadecimales
```
Ejemplo: 48 8B C4 48 89 58 08 48 89 70 10
```

### Opción 2: Archivo de Patch
```
Ejemplo: C:\patches\mi_patch.bin
```

Los patches se inyectan al final del archivo (EOF) con:
- Marcadores de inicio y fin únicos
- Checksum SHA256 del patch
- Clave de validación ofuscada

## 🛡️ Técnicas Implementadas

1. **EOF Data Injection** - Datos en End-of-File
2. **Control Flow Flattening** - Aplanamiento de flujo
3. **Opaque Predicates** - Predicados opacos
4. **Junk Code Injection** - Código basura masivo
5. **Method Virtualization** - Virtualización de métodos
6. **Class Virtualization** - Virtualización de clases
7. **Entry Point Obfuscation** - Ocultación de entrada
8. **String Encryption** - Ofuscación de strings
9. **Anti-Debug Checks** - Verificaciones anti-debugging
10. **Anti-Dump Protection** - Protección anti-volcado

## ⚙️ Dependencias

- **dnlib 4.5.0**: Manipulación de ensamblados .NET
- **.NET 6.0 SDK**: Compilación y ejecución
- **Windows Forms**: Interfaz gráfica

## 📄 Licencia

Este proyecto es de código abierto. Úsalo bajo tu propia responsabilidad.

## ⚠️ Advertencias

- Este software está diseñado para proteger **tu propio código**
- No usar para propósitos maliciosos
- La ofuscación extrema puede impactar el rendimiento
- Siempre mantén copias de seguridad de tus archivos originales
- Algunos antivirus pueden reportar falsos positivos debido a las técnicas de ofuscación

## 🔧 Solución de Problemas

### Error: "No se puede cargar el módulo"
- Verifica que el archivo sea un ejecutable .NET válido
- Asegúrate de tener permisos de lectura/escritura

### Error: "Acceso denegado al guardar"
- Ejecuta como administrador
- Verifica que el archivo no esté en uso

### El archivo protegido no se ejecuta
- Reduce el nivel de protección
- Desactiva Control Flow Obfuscation
- Verifica compatibilidad con el framework .NET target

## 📊 Estadísticas de Protección

En modo **Nivel 3 - Avanzado**:
- **500+** métodos sintéticos generados
- **Hasta 10x** aumento en tamaño de código
- **Extremadamente** difícil de analizar estáticamente
- **Resistente** a herramientas automatizadas de deobfuscación

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:
1. Fork el proyecto
2. Crea una rama para tu feature
3. Commit tus cambios
4. Push a la rama
5. Abre un Pull Request

## 📧 Contacto

Para reportar bugs o sugerencias, abre un issue en el repositorio de GitHub.

---

**EOF Protektor v2.0** - Protección avanzada para ejecutables .NET 🛡️

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