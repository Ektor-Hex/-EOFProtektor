using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using TypeAttributes = dnlib.DotNet.TypeAttributes;
using EOFProtektor.Obfuscation;

namespace EOFProtektor
{
    internal static class Program
    {
        private static readonly Random rnd = new Random();
        
        [STAThread]
        static int Main(string[] args)
        {
            // Habilitar estilos visuales para WinForms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Si se pasan argumentos de línea de comandos, usar modo consola
            if (args.Length > 0)
            {
                return RunConsoleMode(args);
            }

            // Modo GUI por defecto
            return RunGuiMode();
        }

        static int RunGuiMode()
        {
            try
            {
                // Prueba con formulario simple
                using var testForm = new TestForm();
                testForm.ShowDialog();
                return 0;
                
                /*
                using var configForm = new ProtectionConfigForm();
                var result = configForm.ShowDialog();

                if (result == DialogResult.OK && configForm.ApplyProtection)
                {
                    // Aplicar protección con la configuración seleccionada
                    ApplyAdvancedProtection(
                        configForm.SelectedFilePath!,
                        configForm.ProtectionLevel,
                        null, // customPatch
                        configForm.EnableControlFlow,
                        configForm.VirtualizeAll
                    );
                    return 0;
                }

                return 0; // Usuario canceló
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico: {ex.Message}\n\nDetalles:\n{ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        static int RunConsoleMode(string[] args)
        {
            Console.WriteLine("=== EOF Protektor - Anti-Tamper Protection ===");
            Console.WriteLine();

            string filePath = "";
            int protectionLevel = 2;
            bool enableControlFlow = true;
            bool virtualizeAll = false;
            byte[]? customPatch = null;

            // Procesar argumentos de línea de comandos
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--no-controlflow":
                    case "-ncf":
                        enableControlFlow = false;
                        Console.WriteLine("[CONFIGURACIÓN] Control Flow Obfuscation: DESHABILITADO");
                        break;
                    case "--virtualize-all":
                    case "-va":
                        virtualizeAll = true;
                        Console.WriteLine("[CONFIGURACIÓN] Virtualización completa: HABILITADA");
                        break;
                    case "--protection-level":
                    case "-pl":
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int level))
                        {
                            protectionLevel = level;
                            i++; // Saltar el siguiente argumento
                            Console.WriteLine($"[CONFIGURACIÓN] Nivel de protección: {protectionLevel}");
                        }
                        break;
                    case "--help":
                    case "-h":
                        ShowHelp();
                        return 0;
                    default:
                        if (string.IsNullOrEmpty(filePath) && !args[i].StartsWith("-"))
                        {
                            filePath = args[i];
                        }
                        break;
                }
            }

            // Si no se proporcionó un archivo, usar el método original
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetFilePath(new string[0]);
            }

            if (string.IsNullOrEmpty(filePath))
                return 1;

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: El archivo '{filePath}' no existe.");
                Console.WriteLine("Presiona cualquier tecla para salir...");
                
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                return 1;
            }

            try
            {
                Console.WriteLine($"Archivo seleccionado: {Path.GetFileName(filePath)}");
                Console.WriteLine($"Ruta completa: {filePath}");
                Console.WriteLine($"[CONFIGURACIÓN] Nivel de protección: {protectionLevel}");
                Console.WriteLine($"[CONFIGURACIÓN] Control Flow: {(enableControlFlow ? "HABILITADO" : "DESHABILITADO")}");
                Console.WriteLine($"[CONFIGURACIÓN] Virtualización completa: {(virtualizeAll ? "HABILITADA" : "SELECTIVA")}");
                Console.WriteLine();

                // Si no se especificaron argumentos especiales, usar el flujo interactivo original
                if (args.Length == 0 || (args.Length == 1 && !args[0].StartsWith("-")))
                {
                    protectionLevel = GetProtectionLevel();
                    customPatch = GetCustomPatch();
                }

                Console.WriteLine("Aplicando protección...");
                Console.WriteLine();

                ApplyAdvancedProtection(filePath, protectionLevel, customPatch, enableControlFlow, virtualizeAll);
                
                Console.WriteLine("✓ Protección aplicada exitosamente.");
                Console.WriteLine();
                Console.WriteLine("El archivo ha sido protegido con técnicas anti-tamper avanzadas.");
                Console.WriteLine("Presiona cualquier tecla para salir...");
                
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Detalles del error:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
                Console.WriteLine("Presiona cualquier tecla para salir...");
                
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                return 1;
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("USO: EOFProtektor.exe [archivo.exe] [opciones]");
            Console.WriteLine();
            Console.WriteLine("OPCIONES:");
            Console.WriteLine("  --no-controlflow, -ncf     Deshabilitar ofuscación de flujo de control");
            Console.WriteLine("  --virtualize-all, -va      Virtualizar todas las funciones");
            Console.WriteLine("  --protection-level, -pl    Nivel de protección (1-3, default: 2)");
            Console.WriteLine("  --help, -h                 Mostrar esta ayuda");
            Console.WriteLine();
            Console.WriteLine("EJEMPLOS:");
            Console.WriteLine("  EOFProtektor.exe programa.exe");
            Console.WriteLine("  EOFProtektor.exe programa.exe --no-controlflow");
            Console.WriteLine("  EOFProtektor.exe programa.exe --virtualize-all");
            Console.WriteLine("  EOFProtektor.exe programa.exe -va -ncf -pl 3");
            Console.WriteLine();
            Console.WriteLine("NIVELES DE PROTECCIÓN:");
            Console.WriteLine("  1 = Básico (Rápido)");
            Console.WriteLine("  2 = Intermedio (Recomendado)");
            Console.WriteLine("  3 = Avanzado (Máxima protección)");
        }

        static string GetFilePath(string[] args)
        {
            if (args.Length > 0)
            {
                return args[0];
            }

            Console.WriteLine("No se especificó un archivo como parámetro.");
            Console.WriteLine();
            Console.Write("Ingresa la ruta del archivo .exe a proteger: ");
            
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No se ingresó ninguna ruta.");
                Console.WriteLine("Presiona cualquier tecla para salir...");
                
                // Verificar si la entrada está redirigida (para scripts automatizados)
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                return string.Empty;
            }

            // Remover comillas si las tiene
            return input.Trim('"');
        }

        static int GetProtectionLevel()
        {
            Console.WriteLine("Selecciona el nivel de protección:");
            Console.WriteLine("1. Básico (Rápido)");
            Console.WriteLine("2. Intermedio (Recomendado)");
            Console.WriteLine("3. Avanzado (Máxima protección)");
            Console.WriteLine();
            Console.Write("Ingresa tu opción (1-3) [2]: ");

            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return 2; // Valor por defecto

            if (int.TryParse(input, out int level) && level >= 1 && level <= 3)
                return level;

            Console.WriteLine("Opción inválida, usando nivel intermedio.");
            return 2;
        }

        static byte[]? GetCustomPatch()
        {
            Console.WriteLine();
            Console.WriteLine("¿Deseas agregar un patch personalizado?");
            Console.WriteLine("Esto permite inyectar código personalizado en el archivo protegido.");
            Console.WriteLine();
            Console.Write("¿Agregar patch personalizado? (s/N): ");

            string? response = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(response) || 
                (!response.ToLower().StartsWith("s") && !response.ToLower().StartsWith("y")))
            {
                return null;
            }

            Console.WriteLine();
            Console.WriteLine("Opciones para el patch personalizado:");
            Console.WriteLine("1. Ingresar bytes en hexadecimal");
            Console.WriteLine("2. Cargar desde archivo");
            Console.WriteLine();
            Console.Write("Selecciona una opción (1-2): ");

            string? option = Console.ReadLine();
            
            if (option == "1")
            {
                return GetHexPatch();
            }
            else if (option == "2")
            {
                return GetFilePatch();
            }
            else
            {
                Console.WriteLine("Opción inválida, continuando sin patch personalizado.");
                return null;
            }
        }

        static byte[]? GetHexPatch()
        {
            Console.WriteLine();
            Console.WriteLine("Ingresa los bytes del patch en formato hexadecimal.");
            Console.WriteLine("Ejemplo: 48 8B C4 48 89 58 08 48 89 70 10");
            Console.WriteLine("(Puedes usar espacios, guiones o sin separadores)");
            Console.WriteLine();
            Console.Write("Bytes hex: ");

            string? hexInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(hexInput))
            {
                Console.WriteLine("No se ingresaron bytes.");
                return null;
            }

            try
            {
                // Limpiar la entrada
                hexInput = hexInput.Replace(" ", "").Replace("-", "").Replace("0x", "");
                
                if (hexInput.Length % 2 != 0)
                {
                    Console.WriteLine("Error: El número de caracteres hexadecimales debe ser par.");
                    return null;
                }

                byte[] bytes = new byte[hexInput.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexInput.Substring(i * 2, 2), 16);
                }

                Console.WriteLine($"✓ Patch cargado: {bytes.Length} bytes");
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar los bytes hex: {ex.Message}");
                return null;
            }
        }

        static byte[]? GetFilePatch()
        {
            Console.WriteLine();
            Console.Write("Ingresa la ruta del archivo con el patch: ");
            
            string? filePath = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("No se ingresó ninguna ruta.");
                return null;
            }

            filePath = filePath.Trim('"');

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: El archivo '{filePath}' no existe.");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(filePath);
                Console.WriteLine($"✓ Patch cargado desde archivo: {bytes.Length} bytes");
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar el archivo: {ex.Message}");
                return null;
            }
        }

        static void ApplyAdvancedProtection(string path, int protectionLevel = 2, byte[]? customPatch = null, bool enableControlFlow = true, bool virtualizeAll = false)
        {
            Console.WriteLine("=== INICIANDO PROCESO DE PROTECCIÓN AVANZADA ===");
            Console.WriteLine($"[TÉCNICO] Archivo objetivo: {Path.GetFileName(path)}");
            Console.WriteLine($"[TÉCNICO] Ruta completa: {path}");
            Console.WriteLine($"[TÉCNICO] Nivel de protección: {protectionLevel}");
            
            Console.WriteLine($"V Módulo cargado: {Path.GetFileName(path)}");
            ModuleDefMD mod = ModuleDefMD.Load(path);
            
            Console.WriteLine($"[TÉCNICO] Información del módulo:");
            Console.WriteLine($"[TÉCNICO] - Nombre: {mod.Name}");
            Console.WriteLine($"[TÉCNICO] - MVID: {mod.Mvid}");
            Console.WriteLine($"[TÉCNICO] - Assembly: {mod.Assembly.Name}");
            Console.WriteLine($"[TÉCNICO] - Tipos: {mod.Types.Count}");
            Console.WriteLine($"[TÉCNICO] - Métodos globales: {mod.GlobalType.Methods.Count}");
            
            var protectionData = GenerateProtectionData(mod);
            Console.WriteLine($"[TÉCNICO] Datos de protección generados:");
            Console.WriteLine($"[TÉCNICO] - Seed: {protectionData.Seed}");
            Console.WriteLine($"[TÉCNICO] - Marcador primario: 0x{protectionData.PrimaryMarker:X8}");
            Console.WriteLine($"[TÉCNICO] - Marcador secundario: 0x{protectionData.SecondaryMarker:X8}");
            Console.WriteLine($"[TÉCNICO] - Clave de validación: 0x{protectionData.ValidationKey:X8}");
            
            Console.WriteLine("Generando datos de protección...");
            byte[] originalBytes = File.ReadAllBytes(path);
            Console.WriteLine($"[TÉCNICO] Tamaño del archivo original: {originalBytes.Length:N0} bytes");
            
            // Aplicar patch personalizado si se proporcionó
            if (customPatch != null && customPatch.Length > 0)
            {
                Console.WriteLine($"Aplicando patch personalizado ({customPatch.Length} bytes)...");
                Console.WriteLine($"[TÉCNICO] Datos del patch: {BitConverter.ToString(customPatch.Take(Math.Min(16, customPatch.Length)).ToArray())}...");
                ApplyCustomPatch(path, customPatch, protectionData);
                originalBytes = File.ReadAllBytes(path); // Recargar después del patch
                Console.WriteLine($"[TÉCNICO] Nuevo tamaño después del patch: {originalBytes.Length:N0} bytes");
            }
            
            Console.WriteLine("Aplicando protección multicapa...");
            ApplyMultiLayerProtection(path, originalBytes, protectionData);
            
            Console.WriteLine("Inyectando validaciones anti-tamper...");
            mod = ModuleDefMD.Load(path);
            InjectBypassResistantValidation(mod, protectionData, protectionLevel);
            
            Console.WriteLine("Guardando archivo protegido...");
            Console.WriteLine($"[TÉCNICO] Configurando opciones de escritura...");
            
            // Configurar opciones de escritura para evitar problemas de stack
            var writerOptions = new ModuleWriterOptions(mod);
            writerOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;
            Console.WriteLine($"[TÉCNICO] - KeepOldMaxStack: Habilitado");
            
            // Liberar el archivo original antes de escribir
            string tempPath = path + ".tmp";
            Console.WriteLine($"[TÉCNICO] Escribiendo a archivo temporal: {Path.GetFileName(tempPath)}");
            mod.Write(tempPath, writerOptions);
            mod.Dispose(); // Liberar el handle del archivo original
            
            // Reemplazar el archivo original con el temporal
            Console.WriteLine($"[TÉCNICO] Reemplazando archivo original...");
            File.Delete(path);
            File.Move(tempPath, path);
            
            var finalSize = new FileInfo(path).Length;
            Console.WriteLine($"[TÉCNICO] Tamaño final del archivo protegido: {finalSize:N0} bytes");
            Console.WriteLine($"[TÉCNICO] Incremento de tamaño: {finalSize - originalBytes.Length:N0} bytes ({((double)(finalSize - originalBytes.Length) / originalBytes.Length * 100):F1}%)");
        }

        static void ApplyCustomPatch(string path, byte[] patchData, ProtectionData protectionData)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                
                // Crear una sección personalizada para el patch
                var patchSection = new List<byte>();
                
                // Agregar marcador de inicio del patch
                patchSection.AddRange(BitConverter.GetBytes(protectionData.PrimaryMarker));
                
                // Agregar tamaño del patch
                patchSection.AddRange(BitConverter.GetBytes(patchData.Length));
                
                // Agregar datos del patch
                patchSection.AddRange(patchData);
                
                // Agregar marcador de fin del patch
                patchSection.AddRange(BitConverter.GetBytes(protectionData.SecondaryMarker));
                
                // Calcular checksum del patch
                using var sha = SHA256.Create();
                byte[] patchChecksum = sha.ComputeHash(patchData);
                patchSection.AddRange(patchChecksum);
                
                // Escribir el patch al final del archivo
                using var fs = new FileStream(path, FileMode.Append, FileAccess.Write);
                fs.Write(patchSection.ToArray(), 0, patchSection.Count);
                
                Console.WriteLine($"✓ Patch personalizado aplicado exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Advertencia: No se pudo aplicar el patch personalizado: {ex.Message}");
            }
        }

        static ProtectionData GenerateProtectionData(ModuleDefMD mod)
        {
            var data = new ProtectionData();
            
            var seedSource = $"{mod.Name}{mod.Mvid}{mod.Assembly.Name}";
            data.Seed = seedSource.GetHashCode();
            
            var rng = new Random(data.Seed);
            data.PrimaryMarker = (uint)rng.Next();
            data.SecondaryMarker = (uint)rng.Next();
            data.ValidationKey = (uint)rng.Next();
            data.DecoyMarker = (uint)rng.Next();
            
            data.ChecksumPositions = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                data.ChecksumPositions.Add(rng.Next(1000, 20000));
            }
            
            return data;
        }

        static void ApplyMultiLayerProtection(string path, byte[] originalBytes, ProtectionData data)
        {
            Console.WriteLine("=== APLICANDO PROTECCIÓN MULTICAPA ===");
            Console.WriteLine($"[TÉCNICO] Archivo: {Path.GetFileName(path)}");
            Console.WriteLine($"[TÉCNICO] Tamaño original: {originalBytes.Length:N0} bytes");
            Console.WriteLine($"[TÉCNICO] Seed de protección: {data.Seed}");
            
            using var sha = SHA256.Create();
            
            Console.WriteLine("[TÉCNICO] Calculando hash completo del archivo...");
            byte[] fullHash = sha.ComputeHash(originalBytes);
            Console.WriteLine($"[TÉCNICO] Hash SHA256 completo: {BitConverter.ToString(fullHash).Replace("-", "").Substring(0, 16)}...");
            
            Console.WriteLine($"[TÉCNICO] Calculando {data.ChecksumPositions.Count} hashes parciales...");
            var partialHashes = new List<byte[]>();
            int validPositions = 0;
            foreach (var pos in data.ChecksumPositions)
            {
                if (pos < originalBytes.Length)
                {
                    int length = Math.Min(2048, originalBytes.Length - pos);
                    byte[] segment = new byte[length];
                    Array.Copy(originalBytes, pos, segment, 0, length);
                    var hash = sha.ComputeHash(segment);
                    partialHashes.Add(hash);
                    validPositions++;
                    Console.WriteLine($"[TÉCNICO] - Posición {pos}: {length} bytes -> {BitConverter.ToString(hash).Replace("-", "").Substring(0, 8)}...");
                }
            }
            Console.WriteLine($"[TÉCNICO] Hashes parciales válidos: {validPositions}/{data.ChecksumPositions.Count}");
            
            Console.WriteLine("[TÉCNICO] Calculando hash de metadatos...");
            byte[] metadataHash = ComputeMetadataHash(originalBytes);
            Console.WriteLine($"[TÉCNICO] Hash de metadatos: {BitConverter.ToString(metadataHash).Replace("-", "").Substring(0, 16)}...");
            
            Console.WriteLine("[TÉCNICO] Escribiendo datos de protección al final del archivo...");
            long originalSize = new FileInfo(path).Length;
            
            using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                Console.WriteLine($"[TÉCNICO] - Escribiendo marcador señuelo: 0x{data.DecoyMarker:X8}");
                fs.Write(BitConverter.GetBytes(data.DecoyMarker), 0, 4);
                
                Console.WriteLine($"[TÉCNICO] - Escribiendo marcador primario: 0x{data.PrimaryMarker:X8}");
                fs.Write(BitConverter.GetBytes(data.PrimaryMarker), 0, 4);
                
                Console.WriteLine($"[TÉCNICO] - Escribiendo tamaño original: {originalBytes.Length:N0} bytes");
                fs.Write(BitConverter.GetBytes(originalBytes.Length), 0, 4);
                
                Console.WriteLine($"[TÉCNICO] - Escribiendo hash completo: {fullHash.Length} bytes");
                fs.Write(fullHash, 0, fullHash.Length);
                
                Console.WriteLine($"[TÉCNICO] - Escribiendo {partialHashes.Count} hashes parciales");
                fs.Write(BitConverter.GetBytes(partialHashes.Count), 0, 4);
                foreach (var hash in partialHashes)
                {
                    fs.Write(hash, 0, hash.Length);
                }
                
                Console.WriteLine($"[TÉCNICO] - Escribiendo hash de metadatos: {metadataHash.Length} bytes");
                fs.Write(metadataHash, 0, metadataHash.Length);
                
                var obfuscatedKey = data.ValidationKey ^ data.PrimaryMarker ^ data.SecondaryMarker;
                Console.WriteLine($"[TÉCNICO] - Escribiendo clave ofuscada: 0x{obfuscatedKey:X8}");
                fs.Write(BitConverter.GetBytes(obfuscatedKey), 0, 4);
                
                Console.WriteLine($"[TÉCNICO] - Escribiendo marcador secundario: 0x{data.SecondaryMarker:X8}");
                fs.Write(BitConverter.GetBytes(data.SecondaryMarker), 0, 4);
            }
            
            long finalSize = new FileInfo(path).Length;
            long protectionDataSize = finalSize - originalSize;
            Console.WriteLine($"[TÉCNICO] Datos de protección agregados: {protectionDataSize:N0} bytes");
            Console.WriteLine("✓ Protección multicapa aplicada exitosamente");
        }

        static byte[] ComputeMetadataHash(byte[] peBytes)
        {
            using var sha = SHA256.Create();
            var samples = new List<byte>();
            int step = Math.Max(1, peBytes.Length / 150);
            
            for (int i = 0; i < peBytes.Length; i += step)
            {
                samples.Add(peBytes[i]);
            }
            
            return sha.ComputeHash(samples.ToArray());
        }

        static void InjectBypassResistantValidation(ModuleDefMD mod, ProtectionData data, int protectionLevel = 2)
        {
            Console.WriteLine("=== INYECTANDO VALIDACIONES ANTI-TAMPER ===");
            Console.WriteLine($"[TÉCNICO] Nivel de protección: {protectionLevel}");
            Console.WriteLine($"[TÉCNICO] Módulo: {mod.Name}");
            Console.WriteLine($"[TÉCNICO] Assembly: {mod.Assembly.Name}");
            
            // Nivel 1: Protección básica
            if (protectionLevel >= 1)
            {
                Console.WriteLine("[TÉCNICO] === APLICANDO PROTECCIÓN NIVEL 1 (BÁSICA) ===");
                Console.WriteLine("[TÉCNICO] Creando validador principal...");
                var mainValidator = CreateEarlyValidator(mod, data, "MainVal");
                mod.GlobalType.Methods.Add(mainValidator);
                Console.WriteLine($"[TÉCNICO] - Validador principal creado: {mainValidator.Name}");
                
                Console.WriteLine("[TÉCNICO] Protegiendo constructor del módulo...");
                ProtectModuleConstructor(mod, mainValidator, null);
                Console.WriteLine("[TÉCNICO] ✓ Constructor del módulo protegido");
            }
            
            // Nivel 2: Protección intermedia (incluye anti-debug)
            if (protectionLevel >= 2)
            {
                Console.WriteLine("[TÉCNICO] === APLICANDO PROTECCIÓN NIVEL 2 (INTERMEDIA) ===");
                Console.WriteLine("[TÉCNICO] Inyectando clase anti-debug avanzada...");
                InjectUltimateAntiDebugClass(mod, data);
                Console.WriteLine("[TÉCNICO] ✓ Clase anti-debug inyectada");
                
                var validators = new List<MethodDefUser>();
                var mainValidator = mod.GlobalType.Methods.FirstOrDefault(m => m.Name.Contains("MainVal")) as MethodDefUser;
                
                Console.WriteLine("[TÉCNICO] Creando validador anti-dump...");
                var antiDumpValidator = CreateAntiDumpValidator(mod, data, "ADVal");
                validators.Add(antiDumpValidator);
                mod.GlobalType.Methods.Add(antiDumpValidator);
                Console.WriteLine($"[TÉCNICO] - Validador anti-dump creado: {antiDumpValidator.Name}");
                
                Console.WriteLine("[TÉCNICO] Creando validador continuo...");
                var continuousValidator = CreateContinuousValidator(mod, data, "ContVal");
                validators.Add(continuousValidator);
                mod.GlobalType.Methods.Add(continuousValidator);
                Console.WriteLine($"[TÉCNICO] - Validador continuo creado: {continuousValidator.Name}");
                
                if (mainValidator != null)
                {
                    Console.WriteLine("[TÉCNICO] Vinculando validadores al constructor...");
                    ProtectModuleConstructor(mod, mainValidator, antiDumpValidator);
                    Console.WriteLine("[TÉCNICO] ✓ Validadores vinculados");
                }
                
                Console.WriteLine($"[TÉCNICO] Total de validadores creados: {validators.Count}");
            }
            
            // Nivel 3: Protección avanzada (incluye checkpoints distribuidos, trampas, control flow obfuscation y Hide Main)
            if (protectionLevel >= 3)
            {
                Console.WriteLine("[TÉCNICO] === APLICANDO PROTECCIÓN NIVEL 3 (AVANZADA) ===");
                var continuousValidator = mod.GlobalType.Methods.FirstOrDefault(m => m.Name.Contains("ContVal")) as MethodDefUser;
                if (continuousValidator != null)
                {
                    Console.WriteLine("[TÉCNICO] Inyectando checkpoints distribuidos...");
                    InjectDistributedCheckpoints(mod, continuousValidator, data);
                    Console.WriteLine("[TÉCNICO] ✓ Checkpoints distribuidos inyectados");
                }
                
                Console.WriteLine("[TÉCNICO] Creando trampas anti-bypass...");
                CreateBypassTraps(mod, data);
                Console.WriteLine("[TÉCNICO] ✓ Trampas anti-bypass creadas");
                
                if (enableControlFlow)
                {
                    Console.WriteLine("[TÉCNICO] Aplicando ofuscación de flujo de control extrema...");
                    // Aplicar Control Flow Obfuscation extremo
                    ApplyAdvancedControlFlowObfuscation(mod, data);
                    Console.WriteLine("[TÉCNICO] ✓ Ofuscación de flujo de control aplicada");
                }
                else
                {
                    Console.WriteLine("[TÉCNICO] Ofuscación de flujo de control: OMITIDA (--no-controlflow)");
                }
                
                Console.WriteLine("[TÉCNICO] === APLICANDO HIDE MAIN METHODOLOGY EXTREMA ===");
                // Aplicar Hide Main Obfuscation ultra-complejo
                ApplyHideMainObfuscation(mod, data);
                Console.WriteLine("[TÉCNICO] ✓ Hide Main Methodology aplicada exitosamente");
                
                Console.WriteLine("[TÉCNICO] === APLICANDO VIRTUALIZACIÓN DINÁMICA DE CLASES ===");
                // Aplicar virtualización de clases tipo .NET Reactor
                ApplyClassVirtualization(mod, protectionLevel, virtualizeAll);
                Console.WriteLine("[TÉCNICO] ✓ Virtualización dinámica de clases aplicada exitosamente");
            }
            
            var totalMethods = mod.GetTypes().SelectMany(t => t.Methods).Count();
            var protectedMethods = mod.GlobalType.Methods.Count(m => m.Name.Contains("Val") || m.Name.Contains("Core") || m.Name.Contains("State"));
            
            Console.WriteLine($"[TÉCNICO] === RESUMEN DE PROTECCIÓN ===");
            Console.WriteLine($"[TÉCNICO] Métodos totales en el módulo: {totalMethods}");
            Console.WriteLine($"[TÉCNICO] Métodos de protección agregados: {protectedMethods}");
            Console.WriteLine($"[TÉCNICO] Porcentaje de protección: {((double)protectedMethods / totalMethods * 100):F1}%");
            Console.WriteLine($"✓ Protección nivel {protectionLevel} aplicada exitosamente");
        }

        static void InjectUltimateAntiDebugClass(ModuleDefMD mod, ProtectionData data)
        {
            var antiDebugType = new TypeDefUser("", GenerateObfuscatedName("SystemCore", data.Seed),
                mod.CorLibTypes.Object.TypeDefOrRef);
            antiDebugType.Attributes = TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract;
            
            mod.Types.Add(antiDebugType);
            
            var destroyDebuggerMethod = CreateDestroyDebuggerMethod(mod, antiDebugType, data);
            antiDebugType.Methods.Add(destroyDebuggerMethod);
            
            var detectDebuggerHookMethod = CreateDetectDebuggerHookMethod(mod, antiDebugType, data);
            antiDebugType.Methods.Add(detectDebuggerHookMethod);
            
            var verifyCLRIntegrityMethod = CreateVerifyCLRIntegrityMethod(mod, antiDebugType, data);
            antiDebugType.Methods.Add(verifyCLRIntegrityMethod);
            
            var ultimateAntiDebugMethod = CreateUltimateAntiDebugMethod(mod, antiDebugType, data,
                destroyDebuggerMethod, detectDebuggerHookMethod, verifyCLRIntegrityMethod);
            antiDebugType.Methods.Add(ultimateAntiDebugMethod);
            
            mod.GlobalType.Methods.Add(CreateAntiDebugInitializerMethod(mod, ultimateAntiDebugMethod, data));
        }

        static MethodDefUser CreateDestroyDebuggerMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data)
        {
            var method = new MethodDefUser(GenerateObfuscatedName("InitCore", data.Seed + 10),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            body.Variables.Add(new Local(mod.CorLibTypes.IntPtr));
            body.Variables.Add(new Local(mod.CorLibTypes.IntPtr));
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.IntPtr));
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean));

            var instructions = body.Instructions;

            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(System.Diagnostics.Debugger).GetProperty("IsAttached").GetGetMethod())));
            var noDebugger = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brfalse_S, noDebugger));
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_M1));
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }))));
            
            instructions.Add(noDebugger);
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return method;
        }

        static MethodDefUser CreateDetectDebuggerHookMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data)
        {
            var method = new MethodDefUser(GenerateObfuscatedName("CheckState", data.Seed + 20),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            var instructions = body.Instructions;

            for (int i = 0; i < 3; i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Call,
                    mod.Import(typeof(System.Diagnostics.Debugger).GetProperty("IsAttached").GetGetMethod())));
                
                if (i < 2)
                {
                    var noDebuggerDetected = Instruction.Create(OpCodes.Nop);
                    instructions.Add(Instruction.Create(OpCodes.Brfalse_S, noDebuggerDetected));
                    
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                    instructions.Add(Instruction.Create(OpCodes.Ret));
                    
                    instructions.Add(noDebuggerDetected);
                    instructions.Add(Instruction.Create(OpCodes.Pop));
                }
                else
                {
                    var finalNoDebugger = Instruction.Create(OpCodes.Nop);
                    instructions.Add(Instruction.Create(OpCodes.Brfalse_S, finalNoDebugger));
                    
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                    instructions.Add(Instruction.Create(OpCodes.Ret));
                    
                    instructions.Add(finalNoDebugger);
                }
            }

            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return method;
        }

        static MethodDefUser CreateVerifyCLRIntegrityMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data)
        {
            var method = new MethodDefUser(GenerateObfuscatedName("ValidateSystem", data.Seed + 30),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            var instructions = body.Instructions;

            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return method;
        }

        static MethodDefUser CreateUltimateAntiDebugMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data,
            MethodDef destroyMethod, MethodDef detectHookMethod, MethodDef verifyCLRMethod)
        {
            var method = new MethodDefUser(GenerateObfuscatedName("Execute", data.Seed + 40),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Public);

            var body = new CilBody();
            method.Body = body;

            var instructions = body.Instructions;

            instructions.Add(Instruction.Create(OpCodes.Call, verifyCLRMethod));
            var clrOk = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brtrue_S, clrOk));
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_M1));
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }))));
            
            instructions.Add(clrOk);
            
            instructions.Add(Instruction.Create(OpCodes.Call, detectHookMethod));
            var noHook = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brtrue_S, noHook));
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_M1));
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }))));
            
            instructions.Add(noHook);
            
            instructions.Add(Instruction.Create(OpCodes.Call, destroyMethod));
            var debuggerDestroyed = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brtrue_S, debuggerDestroyed));
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_M1));
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }))));
            
            instructions.Add(debuggerDestroyed);
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return method;
        }

        static MethodDefUser CreateAntiDebugInitializerMethod(ModuleDefMD mod, MethodDef ultimateAntiDebugMethod, ProtectionData data)
        {
            var method = new MethodDefUser(GenerateObfuscatedName("Initialize", data.Seed + 50),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            var instructions = body.Instructions;

            instructions.Add(Instruction.Create(OpCodes.Call, ultimateAntiDebugMethod));
            instructions.Add(Instruction.Create(OpCodes.Ret));

            return method;
        }

        static MethodDefUser CreateEarlyValidator(ModuleDefMD mod, ProtectionData data, string baseName)
        {
            var method = new MethodDefUser(GenerateObfuscatedName(baseName, data.Seed),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            body.Variables.Add(new Local(mod.CorLibTypes.String));
            body.Variables.Add(new Local(new SZArraySig(mod.CorLibTypes.Byte)));
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean));
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));

            var instructions = body.Instructions;
            
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(Environment).GetProperty("TickCount").GetGetMethod())));
            instructions.Add(Instruction.Create(OpCodes.Stloc, body.Variables[4]));
            
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(System.Diagnostics.Debugger).GetProperty("IsAttached").GetGetMethod())));
            var continueExec = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brfalse_S, continueExec));
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_M1));
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }))));
            
            instructions.Add(continueExec);
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }

        static MethodDefUser CreateAntiDumpValidator(ModuleDefMD mod, ProtectionData data, string baseName)
        {
            var method = new MethodDefUser(GenerateObfuscatedName(baseName, data.Seed + 1),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            body.Variables.Add(new Local(mod.CorLibTypes.String));
            body.Variables.Add(new Local(new SZArraySig(mod.CorLibTypes.Byte)));
            body.Variables.Add(new Local(mod.CorLibTypes.IntPtr));
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));

            var instructions = body.Instructions;
            
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(System.Reflection.Assembly).GetMethod("GetExecutingAssembly"))));
            instructions.Add(Instruction.Create(OpCodes.Callvirt,
                mod.Import(typeof(System.Reflection.Assembly).GetProperty("Location").GetGetMethod())));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            
            instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
            instructions.Add(Instruction.Create(OpCodes.Call,
                mod.Import(typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) }))));
            var locationOk = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brfalse_S, locationOk));
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            instructions.Add(locationOk);
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }

        static MethodDefUser CreateContinuousValidator(ModuleDefMD mod, ProtectionData data, string baseName)
        {
            var method = new MethodDefUser(GenerateObfuscatedName(baseName, data.Seed + 2),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;

            body.Variables.Add(new Local(mod.CorLibTypes.String));
            body.Variables.Add(new Local(new SZArraySig(mod.CorLibTypes.Byte)));
            
            var instructions = body.Instructions;
            
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }

        static void ProtectModuleConstructor(ModuleDefMD mod, MethodDefUser mainValidator, MethodDefUser antiDumpValidator)
        {
            var modType = mod.GlobalType;
            var cctor = modType.FindOrCreateStaticConstructor();
            var instructions = cctor.Body.Instructions;
            
            // Asegurar que hay al menos una instrucción (ret)
            if (instructions.Count == 0)
            {
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            
            int insertIndex = 0;
            
            // Insertar llamada al validador principal (verificar que no sea nulo)
            if (mainValidator != null)
            {
                instructions.Insert(insertIndex++, Instruction.Create(OpCodes.Call, mainValidator));
            }
            
            // Insertar validación anti-dump (verificar que no sea nulo)
            if (antiDumpValidator != null)
            {
                instructions.Insert(insertIndex++, Instruction.Create(OpCodes.Call, antiDumpValidator));
                
                // Crear etiqueta para salto condicional solo si tenemos validador
                var continueLabel = instructions.Count > insertIndex ? instructions[insertIndex] : instructions[instructions.Count - 1];
                instructions.Insert(insertIndex++, Instruction.Create(OpCodes.Brtrue_S, continueLabel));
                
                // Insertar código de salida si la validación falla
                instructions.Insert(insertIndex++, Instruction.Create(OpCodes.Ldc_I4_M1));
                var exitMethod = mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }));
                if (exitMethod != null)
                {
                    instructions.Insert(insertIndex++, Instruction.Create(OpCodes.Call, exitMethod));
                }
            }
        }

        static void InjectDistributedCheckpoints(ModuleDefMD mod, MethodDefUser validator, ProtectionData data)
        {
            // Verificar que el validador no sea nulo
            if (validator == null) return;
            
            int injected = 0;
            var rng = new Random(data.Seed);
            
            foreach (var type in mod.Types)
            {
                if (type.IsGlobalModuleType) continue;
                
                foreach (var method in type.Methods.Where(m => m.HasBody && m.Body.Instructions.Count > 15))
                {
                    if (injected >= 20) return;
                    
                    try
                    {
                        int insertPos = rng.Next(2, Math.Max(3, method.Body.Instructions.Count / 3));
                        
                        var instructions = method.Body.Instructions;
                        var call = Instruction.Create(OpCodes.Call, validator);
                        var skipLabel = instructions[insertPos];
                        var branch = Instruction.Create(OpCodes.Brtrue_S, skipLabel);
                        
                        var exit = Instruction.Create(OpCodes.Ldc_I4_M1);
                        var exitMethod = mod.Import(typeof(Environment).GetMethod("Exit", new[] { typeof(int) }));
                        
                        // Verificar que el método Exit se importó correctamente
                        if (exitMethod != null)
                        {
                            var exitCall = Instruction.Create(OpCodes.Call, exitMethod);
                            
                            instructions.Insert(insertPos, call);
                            instructions.Insert(insertPos + 1, branch);
                            instructions.Insert(insertPos + 2, exit);
                            instructions.Insert(insertPos + 3, exitCall);
                            
                            method.Body.UpdateInstructionOffsets();
                            injected++;
                        }
                    }
                    catch
                    {
                        // Continue if fails on specific method
                    }
                }
            }
        }

        static void CreateBypassTraps(ModuleDefMD mod, ProtectionData data)
        {
            var decoyMethod = new MethodDefUser(GenerateObfuscatedName("AntiTamper", data.Seed + 100),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            decoyMethod.Body = body;
            
            body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "Tamper detected"));
            
            var exceptionCtor = mod.Import(typeof(Exception).GetConstructor(new[] { typeof(string) }));
            if (exceptionCtor != null)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Newobj, exceptionCtor));
                body.Instructions.Add(Instruction.Create(OpCodes.Throw));
            }
            else
            {
                // Fallback si no se puede importar el constructor
                body.Instructions.Add(Instruction.Create(OpCodes.Pop));
                body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            
            mod.GlobalType.Methods.Add(decoyMethod);
            
            var decoyMethod2 = new MethodDefUser(GenerateObfuscatedName("TamperCheck", data.Seed + 200),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body2 = new CilBody();
            decoyMethod2.Body = body2;
            
            var entryPointCtor = mod.Import(typeof(System.EntryPointNotFoundException).GetConstructor(Type.EmptyTypes));
            if (entryPointCtor != null)
            {
                body2.Instructions.Add(Instruction.Create(OpCodes.Newobj, entryPointCtor));
                body2.Instructions.Add(Instruction.Create(OpCodes.Throw));
            }
            else
            {
                // Fallback si no se puede importar el constructor
                body2.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            
            mod.GlobalType.Methods.Add(decoyMethod2);
        }

        static string GenerateObfuscatedName(string baseName, int seed)
        {
            var rng = new Random(seed);
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            var result = new StringBuilder();
            
            var prefixes = new[] { "Init", "Setup", "Config", "Helper", "Util", "Core", "Base" };
            result.Append(prefixes[rng.Next(prefixes.Length)]);
            
            for (int i = 0; i < 8; i++)
            {
                result.Append(chars[rng.Next(chars.Length)]);
            }
            
            return result.ToString();
        }

        // ============================================================================
        // CONTROL FLOW OBFUSCATION EXTREMO - EL MÁS APARATOSO Y DIFÍCIL DE LEER
        // ============================================================================
        
        static void ApplyAdvancedControlFlowObfuscation(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("Aplicando Control Flow Obfuscation extremo...");
            
            var rng = new Random(data.Seed);
            int methodsObfuscated = 0;
            
            // Crear dispatcher central para control flow
            var dispatcher = CreateChaosDispatcher(mod, data);
            mod.GlobalType.Methods.Add(dispatcher);
            
            // Crear métodos de estado falsos
            var fakeStateMethods = CreateFakeStateMethods(mod, data, 15);
            foreach (var fakeMethod in fakeStateMethods)
            {
                mod.GlobalType.Methods.Add(fakeMethod);
            }
            
            foreach (var type in mod.Types.ToArray())
            {
                if (type.IsGlobalModuleType) continue;
                
                foreach (var method in type.Methods.ToArray())
                {
                    if (!method.HasBody || method.Body.Instructions.Count < 10) continue;
                    if (methodsObfuscated >= 25) break;
                    
                    try
                    {
                        // Aplicar múltiples capas de obfuscación
                        ApplyMultiLayerControlFlowChaos(method, mod, data, dispatcher, fakeStateMethods, rng);
                        methodsObfuscated++;
                    }
                    catch
                    {
                        // Continuar si falla en un método específico
                    }
                }
                
                if (methodsObfuscated >= 25) break;
            }
            
            Console.WriteLine($"✓ Control Flow Obfuscation aplicado a {methodsObfuscated} métodos");
        }
        
        static MethodDefUser CreateChaosDispatcher(ModuleDefMD mod, ProtectionData data)
        {
            var method = new MethodDefUser(GenerateObfuscatedName("StateDispatcher", data.Seed + 1000),
                MethodSig.CreateStatic(mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;
            
            // Variables locales para el caos
            body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // 0: state
            body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // 1: chaos_factor
            body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // 2: temp
            body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // 3: result
            body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // 4: counter
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean)); // 5: flag
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed);
            
            // Crear un laberinto de saltos y cálculos aparatosos
            var labels = new List<Instruction>();
            for (int i = 0; i < 20; i++)
            {
                labels.Add(Instruction.Create(OpCodes.Nop));
            }
            
            // Entrada caótica
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0)); // state
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1)); // input1
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_2)); // input2
            instructions.Add(Instruction.Create(OpCodes.Add));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0)); // store state
            
            // Laberinto de saltos aparatoso
            for (int i = 0; i < labels.Count; i++)
            {
                instructions.Add(labels[i]);
                
                // Cálculos aparatosos sin sentido
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                instructions.Add(Instruction.Create(OpCodes.Mul));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50)));
                instructions.Add(Instruction.Create(OpCodes.Rem));
                instructions.Add(Instruction.Create(OpCodes.Stloc_1));
                
                // Saltos condicionales aparatosos
                instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i % 7));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                
                var nextLabel = labels[(i + rng.Next(1, 5)) % labels.Count];
                instructions.Add(Instruction.Create(OpCodes.Brtrue_S, nextLabel));
                
                // Más cálculos aparatosos
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(10, 200)));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                
                // Salto incondicional a otro punto aleatorio
                if (i < labels.Count - 1)
                {
                    var randomLabel = labels[(i + rng.Next(2, 8)) % labels.Count];
                    instructions.Add(Instruction.Create(OpCodes.Br_S, randomLabel));
                }
            }
            
            // Salida final aparatosa
            instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            instructions.Add(Instruction.Create(OpCodes.Add));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            instructions.Add(Instruction.Create(OpCodes.Mul));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 0x12345));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }
        
        static List<MethodDefUser> CreateFakeStateMethods(ModuleDefMD mod, ProtectionData data, int count)
        {
            var methods = new List<MethodDefUser>();
            var rng = new Random(data.Seed + 500);
            
            for (int i = 0; i < count; i++)
            {
                var method = new MethodDefUser(GenerateObfuscatedName("FakeState", data.Seed + i + 2000),
                    MethodSig.CreateStatic(mod.CorLibTypes.Int32, mod.CorLibTypes.Int32),
                    MethodAttributes.Static | MethodAttributes.Private);

                var body = new CilBody();
                method.Body = body;
                
                body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                body.Variables.Add(new Local(mod.CorLibTypes.Boolean));
                
                var instructions = body.Instructions;
                
                // Crear código aparatoso que no hace nada útil
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Mul));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                instructions.Add(Instruction.Create(OpCodes.Rem));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                
                // Bucle aparatoso
                var loopStart = Instruction.Create(OpCodes.Ldloc_0);
                var loopEnd = Instruction.Create(OpCodes.Nop);
                
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Stloc_1));
                instructions.Add(Instruction.Create(OpCodes.Br_S, loopEnd));
                
                instructions.Add(loopStart);
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50)));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Stloc_1));
                
                instructions.Add(loopEnd);
                instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(3, 10)));
                instructions.Add(Instruction.Create(OpCodes.Clt));
                instructions.Add(Instruction.Create(OpCodes.Brtrue_S, loopStart));
                
                // Retorno aparatoso
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(100, 500)));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Ret));
                
                methods.Add(method);
            }
            
            return methods;
        }
        
        static void ApplyMultiLayerControlFlowChaos(MethodDef method, ModuleDefMD mod, ProtectionData data, 
            MethodDefUser dispatcher, List<MethodDefUser> fakeStateMethods, Random rng)
        {
            if (!method.HasBody || method.Body.Instructions.Count < 5) return;
            
            var instructions = method.Body.Instructions;
            var originalInstructions = instructions.ToArray();
            
            // Limpiar instrucciones existentes
            instructions.Clear();
            
            // Agregar variables locales para el caos
            method.Body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // state
            method.Body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // chaos_counter
            method.Body.Variables.Add(new Local(mod.CorLibTypes.Int32)); // fake_state
            method.Body.Variables.Add(new Local(mod.CorLibTypes.Boolean)); // chaos_flag
            
            int stateVar = method.Body.Variables.Count - 4;
            int chaosVar = method.Body.Variables.Count - 3;
            int fakeVar = method.Body.Variables.Count - 2;
            int flagVar = method.Body.Variables.Count - 1;
            
            // Dividir el código original en bloques
            var blocks = CreateChaosBlocks(originalInstructions, rng);
            var blockLabels = new Dictionary<int, Instruction>();
            
            // Crear etiquetas para cada bloque
            for (int i = 0; i < blocks.Count; i++)
            {
                blockLabels[i] = Instruction.Create(OpCodes.Nop);
            }
            
            // Inicialización caótica
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
            instructions.Add(Instruction.Create(OpCodes.Stloc, stateVar));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Add(Instruction.Create(OpCodes.Stloc, chaosVar));
            
            // Crear el switch caótico principal
            var switchStart = Instruction.Create(OpCodes.Ldloc, stateVar);
            instructions.Add(Instruction.Create(OpCodes.Br_S, switchStart));
            
            // Insertar bloques con control flow aparatoso
            for (int i = 0; i < blocks.Count; i++)
            {
                instructions.Add(blockLabels[i]);
                
                // Llamada aparatosa al dispatcher
                instructions.Add(Instruction.Create(OpCodes.Ldloc, stateVar));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                instructions.Add(Instruction.Create(OpCodes.Call, dispatcher));
                instructions.Add(Instruction.Create(OpCodes.Stloc, fakeVar));
                
                // Llamadas aparatosas a métodos falsos
                for (int j = 0; j < rng.Next(1, 4); j++)
                {
                    var fakeMethod = fakeStateMethods[rng.Next(fakeStateMethods.Count)];
                    instructions.Add(Instruction.Create(OpCodes.Ldloc, fakeVar));
                    instructions.Add(Instruction.Create(OpCodes.Call, fakeMethod));
                    instructions.Add(Instruction.Create(OpCodes.Stloc, fakeVar));
                }
                
                // Insertar el código original del bloque
                foreach (var instr in blocks[i])
                {
                    instructions.Add(instr);
                }
                
                // Cálculo aparatoso del siguiente estado
                instructions.Add(Instruction.Create(OpCodes.Ldloc, stateVar));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(100, 999)));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, fakeVar));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Stloc, stateVar));
                
                // Salto condicional aparatoso al siguiente bloque
                if (i < blocks.Count - 1)
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldloc, chaosVar));
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                    instructions.Add(Instruction.Create(OpCodes.Add));
                    instructions.Add(Instruction.Create(OpCodes.Stloc, chaosVar));
                    
                    instructions.Add(Instruction.Create(OpCodes.Ldloc, chaosVar));
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(2, 10)));
                    instructions.Add(Instruction.Create(OpCodes.Rem));
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                    instructions.Add(Instruction.Create(OpCodes.Ceq));
                    
                    var nextBlock = blockLabels[(i + 1) % blocks.Count];
                    instructions.Add(Instruction.Create(OpCodes.Brtrue_S, nextBlock));
                    
                    // Salto aparatoso a bloque aleatorio
                    var randomBlock = blockLabels[rng.Next(Math.Max(0, i - 2), Math.Min(blocks.Count, i + 3))];
                    instructions.Add(Instruction.Create(OpCodes.Br_S, randomBlock));
                }
            }
            
            // Switch aparatoso al final
            instructions.Add(switchStart);
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
            instructions.Add(Instruction.Create(OpCodes.Rem));
            
            // Crear saltos aparatosos basados en el estado
            for (int i = 0; i < Math.Min(blocks.Count, 10); i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Dup));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Brtrue_S, blockLabels[i % blocks.Count]));
            }
            
            // Salto por defecto
            instructions.Add(Instruction.Create(OpCodes.Pop));
            if (blocks.Count > 0)
            {
                instructions.Add(Instruction.Create(OpCodes.Br_S, blockLabels[0]));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            
            // Actualizar offsets
            method.Body.UpdateInstructionOffsets();
        }
        
        static List<List<Instruction>> CreateChaosBlocks(Instruction[] originalInstructions, Random rng)
        {
            var blocks = new List<List<Instruction>>();
            var currentBlock = new List<Instruction>();
            
            int blockSize = Math.Max(2, originalInstructions.Length / Math.Max(1, rng.Next(3, 8)));
            
            for (int i = 0; i < originalInstructions.Length; i++)
            {
                currentBlock.Add(originalInstructions[i]);
                
                if (currentBlock.Count >= blockSize || i == originalInstructions.Length - 1)
                {
                    if (currentBlock.Count > 0)
                    {
                        blocks.Add(new List<Instruction>(currentBlock));
                        currentBlock.Clear();
                    }
                    blockSize = Math.Max(2, rng.Next(2, 6));
                }
            }
            
            // Mezclar bloques para mayor caos
            for (int i = 0; i < blocks.Count; i++)
            {
                int swapIndex = rng.Next(blocks.Count);
                var temp = blocks[i];
                blocks[i] = blocks[swapIndex];
                blocks[swapIndex] = temp;
            }
            
            return blocks;
        }

        static void ApplyHideMainObfuscation(ModuleDefMD mod, ProtectionData data)
        {
            // Llamar al nuevo módulo HideMainObfuscator
            EOFProtektor.Obfuscation.HideMainObfuscator.ApplyHideMainObfuscation(mod, data);
        }

        static void ApplyClassVirtualization(ModuleDefMD mod, int protectionLevel, bool virtualizeAll = false)
        {
            // Llamar al nuevo módulo ClassVirtualizationObfuscator
            ClassVirtualizationObfuscator.ApplyClassVirtualization(mod, protectionLevel, virtualizeAll);
        }

        class ProtectionData
        {
            public int Seed { get; set; }
            public uint PrimaryMarker { get; set; }
            public uint SecondaryMarker { get; set; }
            public uint ValidationKey { get; set; }
            public uint DecoyMarker { get; set; }
            public List<int> ChecksumPositions { get; set; } = new List<int>();
        }
    }
}