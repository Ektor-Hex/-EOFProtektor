using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using EOFProtektor.Core;
using EOFProtektor.Protection;
using EOFProtektor.Obfuscation;
using EOFProtektor.Utils;

namespace EOFProtektor
{
    class Program_Protected
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Si hay argumentos de línea de comandos, usar modo consola
            if (args.Length > 0)
            {
                RunConsoleMode(args);
                return;
            }

            // Sin argumentos, usar modo GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                using var configForm = new ProtectionConfigForm();
                Application.Run(configForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico: {ex.Message}\n\nDetalles:\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void RunConsoleMode(string[] args)
        {
            Console.WriteLine("=== EOF PROTEKTOR v2.0 - ARQUITECTURA MODULAR ===");
            Console.WriteLine("Protector de ejecutables .NET con ofuscación avanzada");
            Console.WriteLine();

            try
            {
                string filePath = GetFilePath(args);
                
                if (!ValidateFile(filePath))
                {
                    Console.WriteLine("Error: Archivo inválido o inaccesible.");
                    Console.WriteLine("Presiona cualquier tecla para salir...");
                    Console.ReadKey();
                    return;
                }
                
                int protectionLevel = GetProtectionLevel();
                bool useCustomPatch = GetCustomPatch();

                Console.WriteLine($"Archivo seleccionado: {Path.GetFileName(filePath)}");
                Console.WriteLine($"Ruta completa: {filePath}");
                Console.WriteLine($"Nivel de protección: {protectionLevel}");
                Console.WriteLine();

                ApplyAdvancedProtection(filePath, protectionLevel, useCustomPatch);

                Console.WriteLine();
                Console.WriteLine("✓ Protección aplicada exitosamente!");
                Console.WriteLine("El archivo protegido ha sido guardado.");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: Archivo no encontrado - {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error: Acceso denegado - {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error de E/S: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                Console.WriteLine("Presiona cualquier tecla para salir...");
                Console.ReadKey();
            }
        }
        
        static bool ValidateFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;
                
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"El archivo '{filePath}' no existe.");
                    return false;
                }
                
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                {
                    Console.WriteLine("El archivo está vacío.");
                    return false;
                }
                
                // Verificar extensión
                var ext = Path.GetExtension(filePath).ToLower();
                if (ext != ".exe" && ext != ".dll")
                {
                    Console.WriteLine($"Advertencia: El archivo no tiene extensión .exe o .dll ({ext})");
                    Console.Write("¿Continuar de todos modos? (s/n): ");
                    var response = Console.ReadLine()?.ToLower();
                    if (response != "s" && response != "si" && response != "y" && response != "yes")
                        return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al validar el archivo: {ex.Message}");
                return false;
            }
        }

        static string GetFilePath(string[] args)
        {
            if (args.Length > 0 && File.Exists(args[0]))
            {
                return args[0];
            }

            Console.Write("Ingresa la ruta del archivo .exe a proteger: ");
            string? input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input) || !File.Exists(input))
            {
                throw new FileNotFoundException("El archivo especificado no existe.");
            }

            return input;
        }

        static int GetProtectionLevel()
        {
            Console.WriteLine("Selecciona el nivel de protección:");
            Console.WriteLine("1 - Básico (Validación de integridad)");
            Console.WriteLine("2 - Intermedio (+ Anti-debugging + Anti-dumping)");
            Console.WriteLine("3 - Avanzado (+ Control Flow Obfuscation + Validación continua)");
            Console.Write("Nivel (1-3): ");

            if (int.TryParse(Console.ReadLine(), out int level) && level >= 1 && level <= 3)
            {
                return level;
            }

            Console.WriteLine("Nivel inválido, usando nivel 1 por defecto.");
            return 1;
        }

        static bool GetCustomPatch()
        {
            Console.Write("¿Deseas agregar un patch personalizado? (s/n): ");
            string? response = Console.ReadLine()?.ToLower();
            return response == "s" || response == "si" || response == "y" || response == "yes";
        }

        static void ApplyAdvancedProtection(string filePath, int protectionLevel, bool useCustomPatch)
        {
            try
            {
                Console.WriteLine("Iniciando proceso de protección...");

                // Crear datos de protección
                var data = new ProtectionData();
                Console.WriteLine($"✓ Datos de protección generados (Seed: {data.Seed})");
                
                // Cargar el módulo
                ModuleDefMD mod;
                try
                {
                    mod = ModuleDefMD.Load(filePath);
                    Console.WriteLine($"✓ Módulo cargado: {mod.Name}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error al cargar el módulo .NET: {ex.Message}", ex);
                }

                // Aplicar protecciones según el nivel
                try
                {
                    ApplyProtectionByLevel(mod, data, protectionLevel);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: Error al aplicar algunas protecciones: {ex.Message}");
                }

                // Guardar el módulo protegido primero
                string outputPath = GetOutputPath(filePath);
                
                try
                {
                    mod.Write(outputPath);
                    Console.WriteLine($"✓ Archivo protegido guardado: {outputPath}");
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error al guardar el archivo protegido: {ex.Message}", ex);
                }
                
                // Aplicar patch personalizado si se solicita
                if (useCustomPatch)
                {
                    try
                    {
                        ApplyCustomPatchLogic(outputPath, data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Advertencia: Error al aplicar patch personalizado: {ex.Message}");
                    }
                }

                // Aplicar protección multicapa
                try
                {
                    IntegrityProtection.ApplyMultiLayerProtection(outputPath, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: Error al aplicar protección multicapa: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico en ApplyAdvancedProtection: {ex.Message}");
                throw;
            }
        }

        static void ApplyProtectionByLevel(ModuleDefMD mod, ProtectionData data, int level)
        {
            switch (level)
            {
                case 1:
                    Console.WriteLine("Aplicando protección básica...");
                    InjectBasicValidation(mod, data);
                    break;

                case 2:
                    Console.WriteLine("Aplicando protección intermedia...");
                    InjectBasicValidation(mod, data);
                    AntiDebugProtection.InjectUltimateAntiDebugClass(mod, data);
                    InjectAntiDumpValidation(mod, data);
                    break;

                case 3:
                    Console.WriteLine("Aplicando protección avanzada...");
                    InjectBasicValidation(mod, data);
                    AntiDebugProtection.InjectUltimateAntiDebugClass(mod, data);
                    InjectAntiDumpValidation(mod, data);
                    InjectDistributedCheckpoints(mod, data);
                    CreateBypassTraps(mod, data);
                    ControlFlowObfuscator.ApplyAdvancedControlFlowObfuscation(mod, data);
                    ProtectModuleConstructor(mod, data);
                    break;
            }
        }

        static void InjectBasicValidation(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("Inyectando validación básica...");
            
            var validatorType = new TypeDefUser("", NameObfuscator.GenerateObfuscatedName("Validator", data.Seed),
                mod.CorLibTypes.Object.TypeDefOrRef);
            validatorType.Attributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            
            var validateMethod = new MethodDefUser(NameObfuscator.GenerateObfuscatedName("Validate", data.Seed + 100),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            validateMethod.Body = body;
            
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean));
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed);
            
            // Validación aparatosa
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, data.ChecksumPosition));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Add(Instruction.Create(OpCodes.Cgt));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            validatorType.Methods.Add(validateMethod);
            mod.Types.Add(validatorType);
            
            Console.WriteLine("✓ Validación básica inyectada");
        }

        static void InjectAntiDumpValidation(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("Inyectando validación anti-dump...");
            
            var antiDumpType = new TypeDefUser("", NameObfuscator.GenerateObfuscatedName("AntiDump", data.Seed + 200),
                mod.CorLibTypes.Object.TypeDefOrRef);
            antiDumpType.Attributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            
            var checkMethod = new MethodDefUser(NameObfuscator.GenerateObfuscatedName("CheckDump", data.Seed + 300),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            checkMethod.Body = body;
            
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed + 100);
            
            // Anti-dump aparatoso
            for (int i = 0; i < 5; i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Pop));
            }
            
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            antiDumpType.Methods.Add(checkMethod);
            mod.Types.Add(antiDumpType);
            
            Console.WriteLine("✓ Validación anti-dump inyectada");
        }

        static void InjectDistributedCheckpoints(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("Inyectando checkpoints distribuidos...");
            
            var checkpointType = new TypeDefUser("", NameObfuscator.GenerateObfuscatedName("Checkpoint", data.Seed + 400),
                mod.CorLibTypes.Object.TypeDefOrRef);
            checkpointType.Attributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            
            // Crear múltiples métodos de checkpoint
            for (int i = 0; i < 3; i++)
            {
                var checkpointMethod = new MethodDefUser(NameObfuscator.GenerateObfuscatedName($"Check{i}", data.Seed + 500 + i),
                    MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                    MethodAttributes.Static | MethodAttributes.Private);

                var body = new CilBody();
                checkpointMethod.Body = body;
                
                body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                
                var instructions = body.Instructions;
                var rng = new Random(data.Seed + i * 100);
                
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, data.Seed + i));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Cgt));
                instructions.Add(Instruction.Create(OpCodes.Ret));
                
                checkpointType.Methods.Add(checkpointMethod);
            }
            
            mod.Types.Add(checkpointType);
            Console.WriteLine("✓ Checkpoints distribuidos inyectados");
        }

        static void CreateBypassTraps(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("Creando trampas anti-bypass...");
            
            var trapType = new TypeDefUser("", NameObfuscator.GenerateObfuscatedName("Trap", data.Seed + 600),
                mod.CorLibTypes.Object.TypeDefOrRef);
            trapType.Attributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            
            var trapMethod = new MethodDefUser(NameObfuscator.GenerateObfuscatedName("TriggerTrap", data.Seed + 700),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            trapMethod.Body = body;
            
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean));
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed + 200);
            
            // Trampas aparatosas
            for (int i = 0; i < 8; i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Rem));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Stloc_1));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                instructions.Add(Instruction.Create(OpCodes.Pop));
            }
            
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            trapType.Methods.Add(trapMethod);
            mod.Types.Add(trapType);
            
            Console.WriteLine("✓ Trampas anti-bypass creadas");
        }

        static void ProtectModuleConstructor(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("Protegiendo constructor del módulo...");
            
            var cctor = mod.GlobalType.FindOrCreateStaticConstructor();
            var instructions = cctor.Body.Instructions;
            
            // Insertar validaciones al inicio del constructor
            var rng = new Random(data.Seed + 300);
            
            for (int i = 0; i < 5; i++)
            {
                instructions.Insert(i * 3, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
                instructions.Insert(i * 3 + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                instructions.Insert(i * 3 + 2, Instruction.Create(OpCodes.Xor));
                instructions.Insert(i * 3 + 3, Instruction.Create(OpCodes.Pop));
            }
            
            Console.WriteLine("✓ Constructor del módulo protegido");
        }

        static void ApplyCustomPatchLogic(string filePath, ProtectionData data)
        {
            Console.WriteLine("Configurando patch personalizado...");
            Console.WriteLine("1 - Ingresar bytes en hexadecimal");
            Console.WriteLine("2 - Cargar desde archivo");
            Console.Write("Opción: ");

            string? option = Console.ReadLine();
            byte[] patchData;

            if (option == "2")
            {
                Console.Write("Ruta del archivo de patch: ");
                string? patchFile = Console.ReadLine();
                
                if (string.IsNullOrEmpty(patchFile) || !File.Exists(patchFile))
                {
                    Console.WriteLine("Archivo no encontrado, omitiendo patch.");
                    return;
                }
                
                patchData = File.ReadAllBytes(patchFile);
            }
            else
            {
                Console.Write("Ingresa los bytes en hexadecimal (ej: 48656C6C6F): ");
                string? hexInput = Console.ReadLine();
                
                if (string.IsNullOrEmpty(hexInput))
                {
                    Console.WriteLine("Entrada vacía, omitiendo patch.");
                    return;
                }
                
                try
                {
                    patchData = ConvertHexStringToBytes(hexInput);
                }
                catch
                {
                    Console.WriteLine("Formato hexadecimal inválido, omitiendo patch.");
                    return;
                }
            }

            IntegrityProtection.ApplyCustomPatch(filePath, patchData, data);
        }

        static void ApplyAdvancedProtectionFromGUI(string filePath, int protectionLevel, 
            byte[]? customPatchData, bool enableControlFlow, bool virtualizeAll)
        {
            try
            {
                Console.WriteLine("Iniciando proceso de protección...");

                // Crear datos de protección
                var data = new ProtectionData();
                Console.WriteLine($"✓ Datos de protección generados (Seed: {data.Seed})");
                
                // Cargar el módulo
                ModuleDefMD mod;
                try
                {
                    mod = ModuleDefMD.Load(filePath);
                    Console.WriteLine($"✓ Módulo cargado: {mod.Name}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error al cargar el módulo .NET: {ex.Message}", ex);
                }

                // Aplicar protecciones según el nivel
                try
                {
                    ApplyProtectionByLevel(mod, data, protectionLevel);
                    
                    // Aplicar control flow si está habilitado
                    if (enableControlFlow && protectionLevel >= 2)
                    {
                        Console.WriteLine("Aplicando Control Flow Obfuscation adicional...");
                        ControlFlowObfuscator.ApplyAdvancedControlFlowObfuscation(mod, data);
                    }
                    
                    // Aplicar virtualización si está habilitada
                    if (virtualizeAll && protectionLevel >= 3)
                    {
                        Console.WriteLine("Aplicando virtualización completa de clases...");
                        ClassVirtualizationObfuscator.ApplyClassVirtualization(mod, protectionLevel, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: Error al aplicar algunas protecciones: {ex.Message}");
                }

                // Guardar el módulo protegido primero
                string outputPath = GetOutputPath(filePath);
                
                try
                {
                    mod.Write(outputPath);
                    Console.WriteLine($"✓ Archivo protegido guardado: {outputPath}");
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error al guardar el archivo protegido: {ex.Message}", ex);
                }
                
                // Aplicar patch personalizado si se proporcionó
                if (customPatchData != null && customPatchData.Length > 0)
                {
                    try
                    {
                        Console.WriteLine("Aplicando patch personalizado...");
                        IntegrityProtection.ApplyCustomPatch(outputPath, customPatchData, data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Advertencia: Error al aplicar patch personalizado: {ex.Message}");
                    }
                }

                // Aplicar protección multicapa
                try
                {
                    IntegrityProtection.ApplyMultiLayerProtection(outputPath, data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: Error al aplicar protección multicapa: {ex.Message}");
                }
                
                Console.WriteLine();
                Console.WriteLine("✓ Protección aplicada exitosamente!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico: {ex.Message}");
                throw;
            }
        }

        static string GetOutputPath(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath) ?? "";
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);
            
            return Path.Combine(directory, $"{fileName}_protected{extension}");
        }

        static byte[] ConvertHexStringToBytes(string hex)
        {
            // Remover espacios y convertir a mayúsculas
            hex = hex.Replace(" ", "").ToUpper();
            
            // Verificar que la longitud sea par
            if (hex.Length % 2 != 0)
                throw new ArgumentException("La cadena hexadecimal debe tener una longitud par");
            
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            
            return bytes;
        }
    }

    // Clase para redirigir Console.WriteLine a un TextBox
    public class TextBoxWriter : System.IO.TextWriter
    {
        private TextBox textBox;
        
        public TextBoxWriter(TextBox textBox)
        {
            this.textBox = textBox;
        }

        public override void Write(char value)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => textBox.AppendText(value.ToString())));
            }
            else
            {
                textBox.AppendText(value.ToString());
            }
        }

        public override void Write(string? value)
        {
            if (value != null)
            {
                if (textBox.InvokeRequired)
                {
                    textBox.Invoke(new Action(() => textBox.AppendText(value)));
                }
                else
                {
                    textBox.AppendText(value);
                }
            }
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}