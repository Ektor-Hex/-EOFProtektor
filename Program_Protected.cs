using System;
using System.IO;
using System.Linq;
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
        static void Main(string[] args)
        {
            Console.WriteLine("=== EOF PROTEKTOR v2.0 - ARQUITECTURA MODULAR ===");
            Console.WriteLine("Protector de ejecutables .NET con ofuscación avanzada");
            Console.WriteLine();

            try
            {
                string filePath = GetFilePath(args);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Presiona cualquier tecla para salir...");
                Console.ReadKey();
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
            Console.WriteLine("Iniciando proceso de protección...");

            // Crear datos de protección
            var data = new ProtectionData();
            
            // Cargar el módulo
            var mod = ModuleDefMD.Load(filePath);
            Console.WriteLine($"✓ Módulo cargado: {mod.Name}");

            // Aplicar protecciones según el nivel
            ApplyProtectionByLevel(mod, data, protectionLevel);

            // Aplicar patch personalizado si se solicita
            if (useCustomPatch)
            {
                ApplyCustomPatchLogic(filePath, data);
            }

            // Aplicar protección multicapa
            IntegrityProtection.ApplyMultiLayerProtection(filePath, data);

            // Guardar el módulo protegido
            string outputPath = GetOutputPath(filePath);
            mod.Write(outputPath);
            Console.WriteLine($"✓ Archivo protegido guardado: {outputPath}");
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
}