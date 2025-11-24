# Ejemplos de Uso - EOF Protektor

## Ejemplo 1: Uso Básico Interactivo

```bash
# Ejecutar el programa sin parámetros
EOFProtektor.exe

# El programa te pedirá:
# 1. Ruta del archivo a proteger
# 2. Nivel de protección (1-3)
# 3. Si deseas agregar un patch personalizado
```

## Ejemplo 2: Uso con Parámetros

```bash
# Proteger un archivo específico
EOFProtektor.exe "C:\MiApp\MiPrograma.exe"
```

## Ejemplo 3: Patches Personalizados

### Patch en Hexadecimal
```
Bytes hex: 48 8B C4 48 89 58 08 48 89 70 10 48 89 78 18 4C 89 60 20
```

### Patch desde Archivo
```
Ruta del archivo: C:\patches\mi_codigo.bin
```

## Ejemplo 4: Niveles de Protección

### Nivel 1 - Básico
- Rápido procesamiento
- Protección básica de integridad
- Ideal para desarrollo y pruebas

### Nivel 2 - Intermedio (Recomendado)
- Balance entre protección y rendimiento
- Anti-debug y anti-dump
- Ideal para distribución general

### Nivel 3 - Avanzado
- Máxima protección
- Checkpoints distribuidos
- Ideal para software crítico

## Ejemplo 5: Flujo Completo

```
=== EOF Protektor - Anti-Tamper Protection ===

No se especificó un archivo como parámetro.

Ingresa la ruta del archivo .exe a proteger: C:\MiApp\MiPrograma.exe
Archivo seleccionado: MiPrograma.exe
Ruta completa: C:\MiApp\MiPrograma.exe

Selecciona el nivel de protección:
1. Básico (Rápido)
2. Intermedio (Recomendado)
3. Avanzado (Máxima protección)

Ingresa tu opción (1-3) [2]: 2

¿Deseas agregar un patch personalizado?
Esto permite inyectar código personalizado en el archivo protegido.

¿Agregar patch personalizado? (s/N): s

Opciones para el patch personalizado:
1. Ingresar bytes en hexadecimal
2. Cargar desde archivo

Selecciona una opción (1-2): 1

Ingresa los bytes del patch en formato hexadecimal.
Ejemplo: 48 8B C4 48 89 58 08 48 89 70 10
(Puedes usar espacios, guiones o sin separadores)

Bytes hex: 90 90 90 90
✓ Patch cargado: 4 bytes

Aplicando protección...

Cargando módulo: MiPrograma.exe
Generando datos de protección...
Aplicando patch personalizado (4 bytes)...
✓ Patch personalizado aplicado exitosamente
Aplicando protección multicapa...
Inyectando validaciones anti-tamper...
✓ Protección nivel 2 aplicada
Guardando archivo protegido...
✓ Protección aplicada exitosamente.

El archivo ha sido protegido con técnicas anti-tamper avanzadas.
Presiona cualquier tecla para salir...
```

## Notas Importantes

1. **Backup**: Siempre haz una copia de seguridad antes de proteger
2. **Pruebas**: Verifica que el archivo protegido funcione correctamente
3. **Antivirus**: Algunos antivirus pueden generar falsos positivos
4. **Rendimiento**: La protección avanzada puede afectar el rendimiento

## Solución de Problemas Comunes

### "El archivo no existe"
- Verifica la ruta del archivo
- Usa comillas si la ruta tiene espacios
- Asegúrate de tener permisos de lectura

### "No se pudo cargar el módulo"
- Verifica que sea un ejecutable .NET válido
- El archivo no debe estar corrupto
- Debe ser un archivo PE válido

### Falsos positivos de antivirus
- Es normal con protectores
- Agrega el archivo a las excepciones
- Usa certificados de código si es posible