# EOF Protektor v2.0 - Arquitectura Modular

## Descripción
EOF Protektor v2.0 es una versión completamente refactorizada del protector de ejecutables .NET con una arquitectura modular limpia y separación de responsabilidades.

## Estructura del Proyecto

```
EOFProtektor/
├── Core/
│   └── ProtectionData.cs          # Datos de configuración de protección
├── Utils/
│   └── NameObfuscator.cs          # Utilidades para ofuscación de nombres
├── Protection/
│   ├── AntiDebugProtection.cs     # Protecciones anti-debug
│   └── IntegrityProtection.cs     # Protecciones de integridad
├── Obfuscation/
│   └── ControlFlowObfuscator.cs   # Control Flow Obfuscation extremo
├── Program_Protected.cs           # Punto de entrada principal modular
├── program.cs                     # Archivo original (excluido del build)
└── EOFProtektor.csproj           # Configuración del proyecto
```

## Características

### Arquitectura Modular
- **Separación de responsabilidades**: Cada módulo tiene una función específica
- **Mantenibilidad**: Código organizado y fácil de mantener
- **Extensibilidad**: Fácil agregar nuevas funcionalidades
- **Reutilización**: Módulos independientes y reutilizables

### Módulos Principales

#### Core/ProtectionData.cs
- Gestión de datos de protección
- Generación de marcadores únicos
- Claves de validación
- Configuración de semillas aleatorias

#### Utils/NameObfuscator.cs
- Ofuscación de nombres de métodos y clases
- Generación de nombres aleatorios
- Utilidades de string manipulation

#### Protection/AntiDebugProtection.cs
- Métodos anti-debugging avanzados
- Detección de hooks
- Verificación de integridad del CLR
- Destrucción de debuggers

#### Protection/IntegrityProtection.cs
- Protección multicapa de archivos
- Aplicación de patches personalizados
- Cálculo de checksums SHA256
- Ofuscación de claves de validación

#### Obfuscation/ControlFlowObfuscator.cs
- Control Flow Obfuscation extremadamente complejo
- Dispatcher caótico de estados
- Métodos de estado falsos
- Laberintos de saltos aparatosos
- División de código en bloques aleatorios

## Niveles de Protección

### Nivel 1 - Básico
- Validación de integridad básica
- Checksums de archivo
- Protección EOF básica

### Nivel 2 - Intermedio
- Todo lo del Nivel 1
- Protección anti-debugging
- Validación anti-dump
- Detección de hooks

### Nivel 3 - Avanzado
- Todo lo del Nivel 2
- Control Flow Obfuscation extremo
- Checkpoints distribuidos
- Trampas anti-bypass
- Protección del constructor del módulo

## Uso

### Compilación
```bash
dotnet build --configuration Release
```

### Ejecución
```bash
# Modo interactivo
./EOFProtektor.exe

# Con argumentos
./EOFProtektor.exe "ruta/al/archivo.exe"
```

### Patches Personalizados
El protector permite agregar patches personalizados de dos formas:
1. **Hexadecimal**: Ingreso directo de bytes en formato hex
2. **Archivo**: Carga de un archivo binario como patch

## Ventajas de la Arquitectura Modular

1. **Mantenibilidad**: Cada módulo es independiente y fácil de mantener
2. **Testabilidad**: Módulos pueden ser probados por separado
3. **Escalabilidad**: Fácil agregar nuevos tipos de protección
4. **Legibilidad**: Código más limpio y organizado
5. **Reutilización**: Módulos pueden ser reutilizados en otros proyectos

## Archivos Generados

El protector genera archivos con el sufijo `_protected`:
- `archivo.exe` → `archivo_protected.exe`

## Tecnologías Utilizadas

- **.NET 6.0**: Framework base
- **dnlib 4.5.0**: Manipulación de assemblies .NET
- **SHA256**: Algoritmos de hash para integridad
- **CIL**: Inyección de código IL personalizado

## Notas de Desarrollo

- El archivo `program.cs` original se mantiene para referencia pero está excluido del build
- La nueva arquitectura permite fácil extensión con nuevos módulos
- Cada módulo tiene su propio namespace para evitar conflictos
- Se utiliza inyección de dependencias implícita a través de parámetros

## Futuras Mejoras

- Sistema de plugins dinámicos
- Configuración externa via JSON/XML
- Interfaz gráfica de usuario
- Soporte para más formatos de archivo
- Integración con sistemas de CI/CD