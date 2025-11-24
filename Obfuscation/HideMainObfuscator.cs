using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using EOFProtektor.Core;

namespace EOFProtektor.Obfuscation
{
    public static class HideMainObfuscator
    {
        public static void ApplyHideMainObfuscation(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("=== INICIANDO HIDE MAIN OBFUSCATION EXTREMO ===");
            Console.WriteLine("[HIDE-MAIN] Metodología ultra-compleja activada");
            Console.WriteLine($"[TÉCNICO] Módulo objetivo: {mod.Name}");
            Console.WriteLine($"[TÉCNICO] Seed de obfuscación: {data.Seed}");
            
            var rng = new Random(data.Seed + 12345);
            
            // Encontrar el método Main original
            var originalMain = FindOriginalMainMethod(mod);
            if (originalMain == null)
            {
                Console.WriteLine("[HIDE-MAIN] ⚠ No se encontró método Main, creando punto de entrada sintético");
                originalMain = CreateSyntheticEntryPoint(mod, data, rng);
            }
            
            Console.WriteLine($"[HIDE-MAIN] Método Main original encontrado: {originalMain.FullName}");
            
            // FASE 1: Crear cientos de métodos Main falsos
            Console.WriteLine("[FASE 1] Creando CIENTOS de métodos Main FALSOS...");
            var fakeMains = CreateMassiveFakeMainMethods(mod, data, rng, 300); // 300 métodos Main falsos
            Console.WriteLine($"[TÉCNICO] Generados {fakeMains.Count} métodos Main falsos");
            
            // FASE 2: Crear dispatchers de entrada múltiples
            Console.WriteLine("[FASE 2] Creando dispatchers de entrada MÚLTIPLES...");
            var entryDispatchers = CreateEntryPointDispatchers(mod, data, rng, 50); // 50 dispatchers
            Console.WriteLine($"[TÉCNICO] Generados {entryDispatchers.Count} dispatchers de entrada");
            
            // FASE 3: Crear clases falsas con métodos Main
            Console.WriteLine("[FASE 3] Creando CLASES FALSAS con métodos Main...");
            var fakeClasses = CreateFakeClassesWithMain(mod, data, rng, 100); // 100 clases falsas
            Console.WriteLine($"[TÉCNICO] Generadas {fakeClasses.Count} clases falsas con Main");
            
            // FASE 4: Ofuscar el Main original con múltiples capas
            Console.WriteLine("[FASE 4] Ofuscando Main ORIGINAL con capas EXTREMAS...");
            ObfuscateOriginalMain(originalMain, mod, data, rng, entryDispatchers);
            
            // FASE 5: Crear red de redirecciones complejas
            Console.WriteLine("[FASE 5] Creando RED de redirecciones COMPLEJAS...");
            CreateComplexRedirectionNetwork(mod, originalMain, fakeMains, entryDispatchers, rng);
            
            // FASE 6: Aplicar anti-análisis al punto de entrada
            Console.WriteLine("[FASE 6] Aplicando ANTI-ANÁLISIS al punto de entrada...");
            ApplyAntiAnalysisToEntryPoint(mod, originalMain, rng);
            
            // FASE 7: Crear métodos de validación de entrada
            Console.WriteLine("[FASE 7] Creando métodos de VALIDACIÓN de entrada...");
            var validationMethods = CreateEntryValidationMethods(mod, data, rng, 75);
            Console.WriteLine($"[TÉCNICO] Generados {validationMethods.Count} métodos de validación");
            
            Console.WriteLine("=== RESUMEN DE HIDE MAIN OBFUSCATION EXTREMO ===");
            Console.WriteLine($"[TÉCNICO] Métodos Main falsos creados: {fakeMains.Count}");
            Console.WriteLine($"[TÉCNICO] Dispatchers de entrada creados: {entryDispatchers.Count}");
            Console.WriteLine($"[TÉCNICO] Clases falsas creadas: {fakeClasses.Count}");
            Console.WriteLine($"[TÉCNICO] Métodos de validación creados: {validationMethods.Count}");
            Console.WriteLine($"[EXTREMO] Total elementos de confusión: {fakeMains.Count + entryDispatchers.Count + fakeClasses.Count + validationMethods.Count}");
            Console.WriteLine("✓ Hide Main Obfuscation EXTREMO aplicado exitosamente");
        }
        
        private static MethodDef FindOriginalMainMethod(ModuleDefMD mod)
        {
            // Buscar el método Main en todas las clases
            foreach (var type in mod.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == "Main" && method.IsStatic)
                    {
                        return method;
                    }
                }
            }
            
            // Buscar por punto de entrada del módulo
            if (mod.EntryPoint != null)
            {
                return mod.EntryPoint;
            }
            
            return null;
        }
        
        private static MethodDef CreateSyntheticEntryPoint(ModuleDefMD mod, ProtectionData data, Random rng)
        {
            var methodSig = MethodSig.CreateStatic(mod.CorLibTypes.Void, mod.CorLibTypes.String.ToSZArraySig());
            var method = new MethodDefUser("Main", methodSig, MethodAttributes.Public | MethodAttributes.Static);
            method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            
            var param = new ParamDefUser("args", 1);
            method.ParamDefs.Add(param);
            
            method.Body = new CilBody();
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            
            // Agregar a una clase existente o crear una nueva
            var targetType = mod.Types.FirstOrDefault(t => !t.IsGlobalModuleType) ?? mod.GlobalType;
            targetType.Methods.Add(method);
            
            // Establecer como punto de entrada
            mod.EntryPoint = method;
            
            return method;
        }
        
        private static List<MethodDefUser> CreateMassiveFakeMainMethods(ModuleDefMD mod, ProtectionData data, Random rng, int count)
        {
            var methods = new List<MethodDefUser>();
            
            for (int i = 0; i < count; i++)
            {
                // Crear diferentes variaciones de Main
                var methodName = GenerateFakeMainName(rng, i);
                var methodSig = GenerateFakeMainSignature(mod, rng);
                
                var method = new MethodDefUser(methodName, methodSig, 
                    MethodAttributes.Public | MethodAttributes.Static);
                method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                
                // Agregar parámetros si es necesario
                if (methodSig.Params.Count > 0)
                {
                    for (int p = 0; p < methodSig.Params.Count; p++)
                    {
                        method.ParamDefs.Add(new ParamDefUser($"param{p}", (ushort)(p + 1)));
                    }
                }
                
                method.Body = new CilBody();
                
                // Crear cuerpo falso complejo
                CreateComplexFakeMainBody(method.Body, mod, rng, i, method);
                
                // Agregar a una clase aleatoria o crear nueva
                var targetType = GetOrCreateRandomType(mod, rng, i);
                targetType.Methods.Add(method);
                
                methods.Add(method);
                
                Console.WriteLine($"[TÉCNICO] Main falso creado: {targetType.Name}.{methodName} (Complejidad: {method.Body.Instructions.Count} instrucciones)");
            }
            
            return methods;
        }
        
        private static string GenerateFakeMainName(Random rng, int index)
        {
            var mainVariations = new[]
            {
                "Main", "main", "MAIN", "Main_", "_Main", "MainEntry", "EntryPoint", 
                "Start", "Begin", "Execute", "Run", "Launch", "Init", "Initialize",
                "MainMethod", "PrimaryEntry", "ApplicationMain", "ProgramMain",
                "CoreMain", "SystemMain", "ProcessMain", "ThreadMain"
            };
            
            var baseName = mainVariations[rng.Next(mainVariations.Length)];
            
            // Agregar sufijos aleatorios
            var suffixes = new[] { "", "_", "__", "Ex", "Impl", "Core", "Sys", "App", "Proc" };
            var suffix = suffixes[rng.Next(suffixes.Length)];
            
            // Agregar números aleatorios ocasionalmente
            if (rng.Next(100) < 30)
            {
                suffix += rng.Next(1, 999).ToString();
            }
            
            return baseName + suffix + "_" + index;
        }
        
        private static MethodSig GenerateFakeMainSignature(ModuleDefMD mod, Random rng)
        {
            var returnTypes = new[]
            {
                mod.CorLibTypes.Void,
                mod.CorLibTypes.Int32,
                mod.CorLibTypes.String,
                mod.CorLibTypes.Boolean
            };
            
            var returnType = returnTypes[rng.Next(returnTypes.Length)];
            
            // Generar parámetros aleatorios
            var paramTypes = new List<TypeSig>();
            var paramCount = rng.Next(0, 4); // 0-3 parámetros
            
            for (int i = 0; i < paramCount; i++)
            {
                var paramTypeOptions = new TypeSig[]
                {
                    mod.CorLibTypes.String.ToSZArraySig(), // string[]
                    mod.CorLibTypes.String,
                    mod.CorLibTypes.Int32,
                    mod.CorLibTypes.Boolean,
                    mod.CorLibTypes.Object
                };
                
                paramTypes.Add(paramTypeOptions[rng.Next(paramTypeOptions.Length)]);
            }
            
            return MethodSig.CreateStatic(returnType, paramTypes.ToArray());
        }
        
        private static void CreateComplexFakeMainBody(CilBody body, ModuleDefMD mod, Random rng, int methodIndex, MethodDefUser method)
        {
            var instructions = body.Instructions;
            var complexity = rng.Next(50, 300); // 50-300 instrucciones
            
            // Agregar variables locales
            for (int i = 0; i < rng.Next(5, 20); i++)
            {
                body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            }
            
            // Crear operaciones complejas que simulan un Main real
            for (int i = 0; i < complexity; i++)
            {
                switch (rng.Next(10))
                {
                    case 0: // Operaciones matemáticas
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Add(Instruction.Create(OpCodes.Add));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 1: // Operaciones con variables locales
                        if (body.Variables.Count > 0)
                        {
                            var localIndex = rng.Next(body.Variables.Count);
                            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                            instructions.Add(Instruction.Create(OpCodes.Stloc_S, body.Variables[localIndex]));
                            instructions.Add(Instruction.Create(OpCodes.Ldloc_S, body.Variables[localIndex]));
                            instructions.Add(Instruction.Create(OpCodes.Pop));
                        }
                        break;
                        
                    case 2: // Operaciones de comparación
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Ceq));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 3: // Operaciones XOR
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 25000)));
                        instructions.Add(Instruction.Create(OpCodes.Xor));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 4: // Operaciones de bit shifting
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 31)));
                        instructions.Add(Instruction.Create(OpCodes.Shl));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 5: // Operaciones de multiplicación
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Mul));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 6: // Operaciones de división
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(100, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(2, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Div));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 7: // Operaciones de módulo
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(100, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(2, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Rem));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 8: // Operaciones AND/OR
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        if (rng.Next(2) == 0)
                            instructions.Add(Instruction.Create(OpCodes.And));
                        else
                            instructions.Add(Instruction.Create(OpCodes.Or));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 9: // Operaciones de negación
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Neg));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                }
            }
            
            // Agregar retorno apropiado según el tipo
            if (method.ReturnType == mod.CorLibTypes.Void)
            {
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            else if (method.ReturnType == mod.CorLibTypes.Int32)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(0, 2))); // Simular exit code
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            else if (method.ReturnType == mod.CorLibTypes.Boolean)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(0, 2)));
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            else
            {
                instructions.Add(Instruction.Create(OpCodes.Ldnull));
                instructions.Add(Instruction.Create(OpCodes.Ret));
            }
        }
        
        private static TypeDef GetOrCreateRandomType(ModuleDefMD mod, Random rng, int index)
        {
            // 50% probabilidad de usar tipo existente, 50% crear nuevo
            if (rng.Next(100) < 50 && mod.Types.Count > 1)
            {
                var existingTypes = mod.Types.Where(t => !t.IsGlobalModuleType).ToList();
                if (existingTypes.Count > 0)
                {
                    return existingTypes[rng.Next(existingTypes.Count)];
                }
            }
            
            // Crear nuevo tipo
            var className = GenerateFakeClassName(rng, index);
            var newType = new TypeDefUser("", className, mod.CorLibTypes.Object.TypeDefOrRef);
            newType.Attributes = TypeAttributes.Public | TypeAttributes.Class;
            
            mod.Types.Add(newType);
            return newType;
        }
        
        private static string GenerateFakeClassName(Random rng, int index)
        {
            var classNames = new[]
            {
                "Program", "Application", "Main", "Entry", "Startup", "Bootstrap", "Launcher",
                "Core", "System", "Process", "Thread", "Service", "Manager", "Controller",
                "Handler", "Processor", "Engine", "Runtime", "Framework", "Platform",
                "AppMain", "ProgramCore", "SystemEntry", "MainApp", "CoreApp", "BaseApp"
            };
            
            var baseName = classNames[rng.Next(classNames.Length)];
            var suffix = rng.Next(100) < 30 ? rng.Next(1, 999).ToString() : "";
            
            return baseName + suffix + "_" + index;
        }
        
        private static List<MethodDefUser> CreateEntryPointDispatchers(ModuleDefMD mod, ProtectionData data, Random rng, int count)
        {
            var dispatchers = new List<MethodDefUser>();
            
            for (int i = 0; i < count; i++)
            {
                var methodSig = MethodSig.CreateStatic(mod.CorLibTypes.Int32,
                    mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32);
                
                var method = new MethodDefUser($"EntryDispatcher_{rng.Next(10000, 99999)}_{i}",
                    methodSig, MethodAttributes.Public | MethodAttributes.Static);
                method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                
                // Agregar parámetros
                for (int p = 1; p <= 4; p++)
                {
                    method.ParamDefs.Add(new ParamDefUser($"param{p}", (ushort)p));
                }
                
                method.Body = new CilBody();
                CreateComplexDispatcherBody(method.Body, mod, rng);
                
                mod.GlobalType.Methods.Add(method);
                dispatchers.Add(method);
                
                Console.WriteLine($"[TÉCNICO] Dispatcher de entrada creado: {method.Name} (Instrucciones: {method.Body.Instructions.Count})");
            }
            
            return dispatchers;
        }
        
        private static void CreateComplexDispatcherBody(CilBody body, ModuleDefMD mod, Random rng)
        {
            var instructions = body.Instructions;
            
            // Agregar variables locales
            for (int i = 0; i < rng.Next(10, 25); i++)
            {
                body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            }
            
            // Crear lógica compleja de dispatching
            for (int i = 0; i < rng.Next(100, 250); i++)
            {
                switch (rng.Next(8))
                {
                    case 0: // Operaciones con argumentos
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                        instructions.Add(Instruction.Create(OpCodes.Add));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                        instructions.Add(Instruction.Create(OpCodes.Mul));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 1: // Operaciones complejas
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
                        instructions.Add(Instruction.Create(OpCodes.Xor));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Add(Instruction.Create(OpCodes.Rem));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 2: // Operaciones con variables locales
                        if (body.Variables.Count > 0)
                        {
                            var localIndex = rng.Next(body.Variables.Count);
                            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                            instructions.Add(Instruction.Create(OpCodes.Xor));
                            instructions.Add(Instruction.Create(OpCodes.Stloc_S, body.Variables[localIndex]));
                        }
                        break;
                        
                    case 3: // Bit operations
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 31)));
                        instructions.Add(Instruction.Create(OpCodes.Shl));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
                        instructions.Add(Instruction.Create(OpCodes.Or));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 4: // Comparaciones
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                        instructions.Add(Instruction.Create(OpCodes.Cgt));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 5: // Operaciones de negación
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                        instructions.Add(Instruction.Create(OpCodes.Not));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
                        instructions.Add(Instruction.Create(OpCodes.And));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 6: // División y módulo
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(2, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Div));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                        instructions.Add(Instruction.Create(OpCodes.Add));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 7: // Operaciones complejas combinadas
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                        instructions.Add(Instruction.Create(OpCodes.Mul));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
                        instructions.Add(Instruction.Create(OpCodes.Xor));
                        instructions.Add(Instruction.Create(OpCodes.Add));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Rem));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                }
            }
            
            // Retorno complejo
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            instructions.Add(Instruction.Create(OpCodes.Mul));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
            instructions.Add(Instruction.Create(OpCodes.Add));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 99999)));
            instructions.Add(Instruction.Create(OpCodes.Rem));
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        
        private static List<TypeDef> CreateFakeClassesWithMain(ModuleDefMD mod, ProtectionData data, Random rng, int count)
        {
            var fakeClasses = new List<TypeDef>();
            
            for (int i = 0; i < count; i++)
            {
                var className = $"FakeMainClass_{rng.Next(10000, 99999)}_{i}";
                var newType = new TypeDefUser("", className, mod.CorLibTypes.Object.TypeDefOrRef);
                newType.Attributes = TypeAttributes.Public | TypeAttributes.Class;
                
                // Crear múltiples métodos Main en cada clase
                var mainCount = rng.Next(2, 6); // 2-5 métodos Main por clase
                for (int m = 0; m < mainCount; m++)
                {
                    var mainMethod = CreateFakeMainInClass(mod, rng, m);
                    newType.Methods.Add(mainMethod);
                }
                
                // Agregar métodos adicionales para confundir
                var additionalMethods = rng.Next(5, 15);
                for (int a = 0; a < additionalMethods; a++)
                {
                    var additionalMethod = CreateAdditionalFakeMethod(mod, rng, a);
                    newType.Methods.Add(additionalMethod);
                }
                
                mod.Types.Add(newType);
                fakeClasses.Add(newType);
                
                Console.WriteLine($"[TÉCNICO] Clase falsa creada: {className} ({mainCount} métodos Main, {additionalMethods} métodos adicionales)");
            }
            
            return fakeClasses;
        }
        
        private static MethodDefUser CreateFakeMainInClass(ModuleDefMD mod, Random rng, int index)
        {
            var mainName = GenerateFakeMainName(rng, index);
            var methodSig = GenerateFakeMainSignature(mod, rng);
            
            var method = new MethodDefUser(mainName, methodSig, 
                MethodAttributes.Public | MethodAttributes.Static);
            method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            
            // Agregar parámetros
            for (int p = 0; p < methodSig.Params.Count; p++)
            {
                method.ParamDefs.Add(new ParamDefUser($"param{p}", (ushort)(p + 1)));
            }
            
            method.Body = new CilBody();
            CreateComplexFakeMainBody(method.Body, mod, rng, index, method);
            
            return method;
        }
        
        private static MethodDefUser CreateAdditionalFakeMethod(ModuleDefMD mod, Random rng, int index)
        {
            var methodNames = new[]
            {
                "Initialize", "Setup", "Configure", "Prepare", "Execute", "Process",
                "Handle", "Manage", "Control", "Validate", "Check", "Verify",
                "Start", "Begin", "Launch", "Run", "Invoke", "Call"
            };
            
            var methodName = methodNames[rng.Next(methodNames.Length)] + "_" + index;
            var methodSig = MethodSig.CreateStatic(mod.CorLibTypes.Void, mod.CorLibTypes.Int32);
            
            var method = new MethodDefUser(methodName, methodSig, 
                MethodAttributes.Public | MethodAttributes.Static);
            method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            
            method.ParamDefs.Add(new ParamDefUser("value", 1));
            
            method.Body = new CilBody();
            var instructions = method.Body.Instructions;
            
            // Crear cuerpo simple pero confuso
            for (int i = 0; i < rng.Next(20, 80); i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Pop));
            }
            
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }
        
        private static void ObfuscateOriginalMain(MethodDef originalMain, ModuleDefMD mod, ProtectionData data, Random rng, List<MethodDefUser> dispatchers)
        {
            if (!originalMain.HasBody) return;
            
            Console.WriteLine($"[HIDE-MAIN] Ofuscando Main original: {originalMain.FullName}");
            
            // Renombrar el método Main original
            var newName = $"RealEntry_{rng.Next(10000, 99999)}_{data.Seed % 1000}";
            Console.WriteLine($"[HIDE-MAIN] Renombrando Main original a: {newName}");
            originalMain.Name = newName;
            
            // Aplicar control flow obfuscation extremo al Main original
            ControlFlowObfuscator.ApplyAdvancedControlFlowObfuscation(mod, data);
            
            // Insertar llamadas a dispatchers en el Main original
            var instructions = originalMain.Body.Instructions;
            var insertionPoints = new List<int>();
            
            for (int i = 1; i < instructions.Count - 1; i += rng.Next(3, 8))
            {
                insertionPoints.Add(i);
            }
            
            int offset = 0;
            foreach (var point in insertionPoints)
            {
                if (dispatchers.Count > 0)
                {
                    var dispatcher = dispatchers[rng.Next(dispatchers.Count)];
                    var adjustedPoint = point + offset;
                    
                    if (adjustedPoint < instructions.Count)
                    {
                        instructions.Insert(adjustedPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Insert(adjustedPoint + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Insert(adjustedPoint + 2, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Insert(adjustedPoint + 3, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Insert(adjustedPoint + 4, Instruction.Create(OpCodes.Call, dispatcher));
                        instructions.Insert(adjustedPoint + 5, Instruction.Create(OpCodes.Pop));
                        offset += 6;
                    }
                }
            }
            
            Console.WriteLine($"[HIDE-MAIN] Main original ofuscado con {insertionPoints.Count} llamadas a dispatchers");
        }
        
        private static void CreateComplexRedirectionNetwork(ModuleDefMD mod, MethodDef originalMain, 
            List<MethodDefUser> fakeMains, List<MethodDefUser> dispatchers, Random rng)
        {
            Console.WriteLine("[HIDE-MAIN] Creando red de redirecciones complejas...");
            
            // Crear métodos de redirección que apuntan a otros métodos falsos
            var redirectionCount = rng.Next(50, 100);
            
            for (int i = 0; i < redirectionCount; i++)
            {
                var methodSig = MethodSig.CreateStatic(mod.CorLibTypes.Void);
                var redirector = new MethodDefUser($"Redirect_{rng.Next(10000, 99999)}_{i}",
                    methodSig, MethodAttributes.Public | MethodAttributes.Static);
                redirector.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                
                redirector.Body = new CilBody();
                var instructions = redirector.Body.Instructions;
                
                // Crear redirecciones complejas
                for (int r = 0; r < rng.Next(10, 30); r++)
                {
                    if (fakeMains.Count > 0 && rng.Next(100) < 30)
                    {
                        var targetFake = fakeMains[rng.Next(fakeMains.Count)];
                        
                        // Agregar parámetros necesarios para la llamada
                        for (int p = 0; p < targetFake.MethodSig.Params.Count; p++)
                        {
                            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        }
                        
                        instructions.Add(Instruction.Create(OpCodes.Call, targetFake));
                        
                        // Manejar valor de retorno si existe
                        if (targetFake.ReturnType != mod.CorLibTypes.Void)
                        {
                            instructions.Add(Instruction.Create(OpCodes.Pop));
                        }
                    }
                    else
                    {
                        // Operaciones de confusión
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Add(Instruction.Create(OpCodes.Xor));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                    }
                }
                
                instructions.Add(Instruction.Create(OpCodes.Ret));
                
                mod.GlobalType.Methods.Add(redirector);
                Console.WriteLine($"[TÉCNICO] Redirector creado: {redirector.Name} (Complejidad: {instructions.Count} instrucciones)");
            }
        }
        
        private static void ApplyAntiAnalysisToEntryPoint(ModuleDefMD mod, MethodDef originalMain, Random rng)
        {
            Console.WriteLine("[HIDE-MAIN] Aplicando anti-análisis al punto de entrada...");
            
            if (!originalMain.HasBody) return;
            
            var instructions = originalMain.Body.Instructions;
            
            // Insertar verificaciones anti-debugging al inicio
            var antiDebugChecks = new List<Instruction>
            {
                // Verificación de debugger
                Instruction.Create(OpCodes.Call, mod.Import(typeof(System.Diagnostics.Debugger).GetMethod("get_IsAttached"))),
                Instruction.Create(OpCodes.Brtrue, instructions[0]), // Si hay debugger, saltar al código original
                
                // Verificaciones de tiempo
                Instruction.Create(OpCodes.Call, mod.Import(typeof(Environment).GetMethod("get_TickCount"))),
                Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 5000)),
                Instruction.Create(OpCodes.Add),
                Instruction.Create(OpCodes.Pop),
                
                // Verificaciones de memoria
                Instruction.Create(OpCodes.Call, mod.Import(typeof(GC).GetMethod("GetTotalMemory", new[] { typeof(bool) }))),
                Instruction.Create(OpCodes.Ldc_I4_1),
                Instruction.Create(OpCodes.Pop),
            };
            
            // Insertar al inicio del método
            for (int i = antiDebugChecks.Count - 1; i >= 0; i--)
            {
                instructions.Insert(0, antiDebugChecks[i]);
            }
            
            Console.WriteLine($"[HIDE-MAIN] Anti-análisis aplicado con {antiDebugChecks.Count} verificaciones");
        }
        
        private static List<MethodDefUser> CreateEntryValidationMethods(ModuleDefMD mod, ProtectionData data, Random rng, int count)
        {
            var validationMethods = new List<MethodDefUser>();
            
            for (int i = 0; i < count; i++)
            {
                var methodSig = MethodSig.CreateStatic(mod.CorLibTypes.Boolean, mod.CorLibTypes.Int32, mod.CorLibTypes.String);
                var method = new MethodDefUser($"ValidateEntry_{rng.Next(10000, 99999)}_{i}",
                    methodSig, MethodAttributes.Public | MethodAttributes.Static);
                method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                
                method.ParamDefs.Add(new ParamDefUser("code", 1));
                method.ParamDefs.Add(new ParamDefUser("key", 2));
                
                method.Body = new CilBody();
                CreateValidationMethodBody(method.Body, mod, rng, data.Seed);
                
                mod.GlobalType.Methods.Add(method);
                validationMethods.Add(method);
                
                Console.WriteLine($"[TÉCNICO] Método de validación creado: {method.Name} (Instrucciones: {method.Body.Instructions.Count})");
            }
            
            return validationMethods;
        }
        
        private static void CreateValidationMethodBody(CilBody body, ModuleDefMD mod, Random rng, int seed)
        {
            var instructions = body.Instructions;
            
            // Agregar variables locales
            for (int i = 0; i < rng.Next(8, 15); i++)
            {
                body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            }
            
            // Crear lógica de validación compleja
            for (int i = 0; i < rng.Next(80, 150); i++)
            {
                switch (rng.Next(6))
                {
                    case 0: // Validación con argumentos
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, seed));
                        instructions.Add(Instruction.Create(OpCodes.Xor));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Add(Instruction.Create(OpCodes.Rem));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 1: // Operaciones con string
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                        instructions.Add(Instruction.Create(OpCodes.Callvirt, mod.Import(typeof(string).GetMethod("get_Length"))));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Mul));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 2: // Operaciones matemáticas complejas
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                        instructions.Add(Instruction.Create(OpCodes.Mul));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(2, 100)));
                        instructions.Add(Instruction.Create(OpCodes.Div));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 3: // Operaciones de hash simuladas
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 0x5A827999)); // Constante de hash
                        instructions.Add(Instruction.Create(OpCodes.Xor));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 31));
                        instructions.Add(Instruction.Create(OpCodes.Shl));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 4: // Verificaciones de rango
                        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
                        instructions.Add(Instruction.Create(OpCodes.Cgt));
                        instructions.Add(Instruction.Create(OpCodes.Pop));
                        break;
                        
                    case 5: // Operaciones con variables locales
                        if (body.Variables.Count > 0)
                        {
                            var localIndex = rng.Next(body.Variables.Count);
                            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                            instructions.Add(Instruction.Create(OpCodes.Add));
                            instructions.Add(Instruction.Create(OpCodes.Stloc_S, body.Variables[localIndex]));
                        }
                        break;
                }
            }
            
            // Retorno aleatorio (siempre true para no romper la ejecución)
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}