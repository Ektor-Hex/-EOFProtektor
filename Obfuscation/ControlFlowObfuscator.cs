using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using EOFProtektor.Core;
using EOFProtektor.Utils;

namespace EOFProtektor.Obfuscation
{
    public static class ControlFlowObfuscator
    {
        public static void ApplyAdvancedControlFlowObfuscation(ModuleDefMD mod, ProtectionData data)
        {
            Console.WriteLine("=== INICIANDO CONTROL FLOW OBFUSCATION EXTREMO AGRESIVO ===");
            Console.WriteLine($"[TÉCNICO] Módulo objetivo: {mod.Name}");
            Console.WriteLine($"[TÉCNICO] Seed de obfuscación: {data.Seed}");
            Console.WriteLine($"[TÉCNICO] Total de tipos en módulo: {mod.Types.Count}");
            Console.WriteLine("[AGRESIVO] Modo ultra-agresivo activado - Máxima ofuscación");
            
            var rng = new Random(data.Seed);
            int methodsObfuscated = 0;
            
            // Crear MUCHOS más dispatchers para confundir masivamente
            Console.WriteLine("[FASE 1] Creando dispatchers de confusión MASIVOS...");
            var dispatchers = CreateMultipleDispatchers(mod, data, 50); // Aumentado de 15 a 50
            Console.WriteLine($"[TÉCNICO] Generados {dispatchers.Count} dispatchers caóticos");
            foreach (var dispatcher in dispatchers)
            {
                mod.GlobalType.Methods.Add(dispatcher);
                Console.WriteLine($"[TÉCNICO] Dispatcher agregado: {dispatcher.Name} (Instrucciones: {dispatcher.Body.Instructions.Count})");
            }
            
            // Crear métodos de estado falsos MASIVOS
            Console.WriteLine("[FASE 2] Creando métodos de estado falsos MASIVOS...");
            var fakeStateMethods = CreateMassiveFakeStateMethods(mod, data, 150); // Aumentado de 50 a 150
            Console.WriteLine($"[TÉCNICO] Generados {fakeStateMethods.Count} métodos de estado falsos");
            foreach (var fakeMethod in fakeStateMethods)
            {
                mod.GlobalType.Methods.Add(fakeMethod);
                Console.WriteLine($"[TÉCNICO] Método falso agregado: {fakeMethod.Name} (Variables locales: {fakeMethod.Body.Variables.Count})");
            }
            
            // Crear métodos de confusión adicionales MASIVOS
            Console.WriteLine("[FASE 3] Creando métodos de confusión adicionales MASIVOS...");
            var confusionMethods = CreateConfusionMethods(mod, data, 100); // Aumentado de 25 a 100
            Console.WriteLine($"[TÉCNICO] Generados {confusionMethods.Count} métodos de confusión");
            foreach (var confusionMethod in confusionMethods)
            {
                mod.GlobalType.Methods.Add(confusionMethod);
                Console.WriteLine($"[TÉCNICO] Método confusión agregado: {confusionMethod.Name} (Complejidad: {confusionMethod.Body.Instructions.Count} instrucciones)");
            }
            
            // Crear métodos de ruido adicionales para máxima confusión
            Console.WriteLine("[FASE 4] Creando métodos de RUIDO EXTREMO...");
            var noiseMethods = CreateExtremeNoiseMethods(mod, data, 200); // Nuevo: 200 métodos de ruido
            Console.WriteLine($"[TÉCNICO] Generados {noiseMethods.Count} métodos de ruido extremo");
            foreach (var noiseMethod in noiseMethods)
            {
                mod.GlobalType.Methods.Add(noiseMethod);
                Console.WriteLine($"[TÉCNICO] Método ruido agregado: {noiseMethod.Name} (Complejidad: {noiseMethod.Body.Instructions.Count} instrucciones)");
            }
            
            // Aplicar a TODAS las clases y métodos posibles con máxima agresividad
            Console.WriteLine("[FASE 5] Aplicando obfuscación ULTRA-EXTREMA a métodos del módulo...");
            int totalMethods = 0;
            int skippedMethods = 0;
            
            foreach (var type in mod.Types.ToArray())
            {
                if (type.IsGlobalModuleType) continue;
                
                Console.WriteLine($"[TÉCNICO] Procesando tipo: {type.FullName} ({type.Methods.Count} métodos)");
                
                foreach (var method in type.Methods.ToArray())
                {
                    totalMethods++;
                    if (!method.HasBody || method.Body.Instructions.Count < 2) // Reducido de 3 a 2 para ser más agresivo
                    {
                        skippedMethods++;
                        Console.WriteLine($"[TÉCNICO] Método omitido: {method.Name} (Sin cuerpo o muy pequeño)");
                        continue;
                    }
                    
                    try
                    {
                        Console.WriteLine($"[TÉCNICO] Obfuscando método: {method.FullName}");
                        Console.WriteLine($"[TÉCNICO] - Instrucciones originales: {method.Body.Instructions.Count}");
                        Console.WriteLine($"[TÉCNICO] - Variables locales originales: {method.Body.Variables.Count}");
                        
                        // Aplicar múltiples capas de obfuscación ULTRA-extrema
                        ApplyUltraExtremeChaosObfuscation(method, mod, data, dispatchers, fakeStateMethods, confusionMethods, noiseMethods, rng);
                        
                        Console.WriteLine($"[TÉCNICO] - Instrucciones después: {method.Body.Instructions.Count}");
                        Console.WriteLine($"[TÉCNICO] - Variables locales después: {method.Body.Variables.Count}");
                        Console.WriteLine($"[TÉCNICO] ✓ Método obfuscado exitosamente");
                        
                        methodsObfuscated++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Fallo en método {method.Name}: {ex.Message}");
                        skippedMethods++;
                    }
                }
            }
            
            Console.WriteLine("=== RESUMEN DE CONTROL FLOW OBFUSCATION ULTRA-AGRESIVO ===");
            Console.WriteLine($"[TÉCNICO] Total métodos procesados: {totalMethods}");
            Console.WriteLine($"[TÉCNICO] Métodos obfuscados exitosamente: {methodsObfuscated}");
            Console.WriteLine($"[TÉCNICO] Métodos omitidos: {skippedMethods}");
            Console.WriteLine($"[TÉCNICO] Dispatchers creados: {dispatchers.Count}");
            Console.WriteLine($"[TÉCNICO] Métodos falsos creados: {fakeStateMethods.Count}");
            Console.WriteLine($"[TÉCNICO] Métodos de confusión creados: {confusionMethods.Count}");
            Console.WriteLine($"[TÉCNICO] Métodos de ruido creados: {noiseMethods.Count}");
            Console.WriteLine($"[AGRESIVO] Total métodos sintéticos: {dispatchers.Count + fakeStateMethods.Count + confusionMethods.Count + noiseMethods.Count}");
            Console.WriteLine($"✓ Control Flow Obfuscation ULTRA-AGRESIVO aplicado a {methodsObfuscated} métodos");
        }
        
        private static List<MethodDefUser> CreateMultipleDispatchers(ModuleDefMD mod, ProtectionData data, int count)
        {
            var dispatchers = new List<MethodDefUser>();
            var rng = new Random(data.Seed);
            
            for (int i = 0; i < count; i++)
            {
                var method = new MethodDefUser(NameObfuscator.GenerateObfuscatedName($"ChaosDispatcher_{i}", data.Seed + i + 1000),
                    MethodSig.CreateStatic(mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32),
                    MethodAttributes.Static | MethodAttributes.Private);

                var body = new CilBody();
                method.Body = body;
                body.KeepOldMaxStack = true;
                
                // Múltiples variables locales para confundir
                for (int v = 0; v < 10; v++)
                {
                    body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                }
                
                var instructions = body.Instructions;
                
                // Crear un laberinto de cálculos aparatosos
                CreateMazeOfCalculations(instructions, rng, 20 + i * 5);
                
                dispatchers.Add(method);
            }
            
            return dispatchers;
        }
        
        private static List<MethodDefUser> CreateMassiveFakeStateMethods(ModuleDefMD mod, ProtectionData data, int count)
        {
            var methods = new List<MethodDefUser>();
            var rng = new Random(data.Seed + 500);
            
            for (int i = 0; i < count; i++)
            {
                var method = new MethodDefUser(NameObfuscator.GenerateObfuscatedName($"FakeState_{i}", data.Seed + i + 2000),
                    MethodSig.CreateStatic(mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32),
                    MethodAttributes.Static | MethodAttributes.Private);

                var body = new CilBody();
                method.Body = body;
                body.KeepOldMaxStack = true;
                
                // Múltiples variables para crear confusión
                for (int v = 0; v < 8; v++)
                {
                    body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                }
                
                var instructions = body.Instructions;
                
                // Crear múltiples loops anidados aparatosos
                CreateNestedFakeLoops(instructions, rng, 15 + i * 3);
                
                methods.Add(method);
            }
            
            return methods;
        }
        
        private static List<MethodDefUser> CreateConfusionMethods(ModuleDefMD mod, ProtectionData data, int count)
        {
            var methods = new List<MethodDefUser>();
            var rng = new Random(data.Seed + 1500);
            
            for (int i = 0; i < count; i++)
            {
                var method = new MethodDefUser(NameObfuscator.GenerateObfuscatedName($"ConfusionMatrix_{i}", data.Seed + i + 3000),
                    MethodSig.CreateStatic(mod.CorLibTypes.Void, mod.CorLibTypes.Int32),
                    MethodAttributes.Static | MethodAttributes.Private);

                var body = new CilBody();
                method.Body = body;
                body.KeepOldMaxStack = true;
                
                // Variables para operaciones aparatosas
                for (int v = 0; v < 6; v++)
                {
                    body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                }
                
                var instructions = body.Instructions;
                
                // Crear operaciones aparatosas que no hacen nada útil
                CreateUselessOperations(instructions, rng, 25 + i * 4);
                
                methods.Add(method);
            }
            
            return methods;
        }
        
        private static void CreateMazeOfCalculations(IList<Instruction> instructions, Random rng, int complexity)
        {
            // Crear operaciones simples y seguras
            for (int i = 0; i < Math.Min(complexity, 20); i++)
            {
                // Operaciones matemáticas básicas
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Mul));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
            }
            
            // Retorno simple
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        
        private static void CreateNestedFakeLoops(IList<Instruction> instructions, Random rng, int complexity)
        {
            // Crear operaciones simples que simulan loops
            for (int i = 0; i < Math.Min(complexity, 15); i++)
            {
                // Operaciones matemáticas básicas
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Mul));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
            }
            
            // Retorno simple
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        
        private static void CreateUselessOperations(IList<Instruction> instructions, Random rng, int complexity)
        {
            // Operaciones simples que no hacen nada útil pero confunden
            for (int i = 0; i < Math.Min(complexity, 25); i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                instructions.Add(Instruction.Create(OpCodes.Mul));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                instructions.Add(Instruction.Create(OpCodes.Rem));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
                
                // Operaciones XOR simples
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(100, 9999)));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
            }
            
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        
        private static void ApplyExtremeChaosObfuscation(MethodDef method, ModuleDefMD mod, ProtectionData data, 
            List<MethodDefUser> dispatchers, List<MethodDefUser> fakeStateMethods, List<MethodDefUser> confusionMethods, Random rng)
        {
            if (!method.HasBody || method.Body.Instructions.Count < 3) return;
            
            Console.WriteLine($"    [CAOS] Iniciando obfuscación caótica en: {method.Name}");
            method.Body.KeepOldMaxStack = true;
            var instructions = method.Body.Instructions;
            var originalCount = instructions.Count;
            
            // Insertar llamadas a métodos aparatosos en múltiples puntos
            var insertionPoints = new List<int>();
            for (int i = 1; i < originalCount - 1; i += rng.Next(2, 6))
            {
                insertionPoints.Add(i);
            }
            
            Console.WriteLine($"    [CAOS] Puntos de inserción identificados: {insertionPoints.Count}");
            
            int insertionOffset = 0;
            int dispatcherCalls = 0;
            int fakeStateCalls = 0;
            int confusionCalls = 0;
            
            foreach (var point in insertionPoints)
            {
                var adjustedPoint = point + insertionOffset;
                if (adjustedPoint >= instructions.Count) break;
                
                // Insertar llamadas a dispatchers aparatosos
                if (dispatchers.Count > 0)
                {
                    var dispatcher = dispatchers[rng.Next(dispatchers.Count)];
                    Console.WriteLine($"    [CAOS] Insertando dispatcher: {dispatcher.Name} en posición {adjustedPoint}");
                    instructions.Insert(adjustedPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                    instructions.Insert(adjustedPoint + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                    instructions.Insert(adjustedPoint + 2, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                    instructions.Insert(adjustedPoint + 3, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                    instructions.Insert(adjustedPoint + 4, Instruction.Create(OpCodes.Call, dispatcher));
                    instructions.Insert(adjustedPoint + 5, Instruction.Create(OpCodes.Pop)); // Descartar resultado
                    insertionOffset += 6;
                    dispatcherCalls++;
                }
                
                // Insertar llamadas a métodos de estado falsos
                if (fakeStateMethods.Count > 0 && rng.Next(100) < 70)
                {
                    var fakeMethod = fakeStateMethods[rng.Next(fakeStateMethods.Count)];
                    var currentPoint = adjustedPoint + insertionOffset;
                    if (currentPoint < instructions.Count)
                    {
                        Console.WriteLine($"    [CAOS] Insertando método falso: {fakeMethod.Name} en posición {currentPoint}");
                        instructions.Insert(currentPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Insert(currentPoint + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Insert(currentPoint + 2, Instruction.Create(OpCodes.Call, fakeMethod));
                        instructions.Insert(currentPoint + 3, Instruction.Create(OpCodes.Pop)); // Descartar resultado
                        insertionOffset += 4;
                        fakeStateCalls++;
                    }
                }
                
                // Insertar llamadas a métodos de confusión
                if (confusionMethods.Count > 0 && rng.Next(100) < 50)
                {
                    var confusionMethod = confusionMethods[rng.Next(confusionMethods.Count)];
                    var currentPoint = adjustedPoint + insertionOffset;
                    if (currentPoint < instructions.Count)
                    {
                        Console.WriteLine($"    [CAOS] Insertando método confusión: {confusionMethod.Name} en posición {currentPoint}");
                        instructions.Insert(currentPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 1000)));
                        instructions.Insert(currentPoint + 1, Instruction.Create(OpCodes.Call, confusionMethod));
                        insertionOffset += 2;
                        confusionCalls++;
                    }
                }
            }
            
            Console.WriteLine($"    [CAOS] Llamadas insertadas - Dispatchers: {dispatcherCalls}, Estados falsos: {fakeStateCalls}, Confusión: {confusionCalls}");
            
            // Insertar operaciones aparatosas adicionales
            var chaosOpsCount = Math.Min(20, originalCount / 2);
            Console.WriteLine($"    [CAOS] Insertando {chaosOpsCount} operaciones caóticas adicionales");
            InsertChaosOperations(method, rng, chaosOpsCount);
            
            try
            {
                Console.WriteLine($"    [CAOS] Optimizando branches y actualizando offsets...");
                // Optimizar branches para evitar errores de distancia
                method.Body.SimplifyBranches();
                method.Body.OptimizeBranches();
                method.Body.UpdateInstructionOffsets();
                Console.WriteLine($"    [CAOS] ✓ Optimización completada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    [CAOS] ⚠ Error en optimización: {ex.Message}");
                method.Body.KeepOldMaxStack = true;
            }
        }
        
        private static void InsertChaosOperations(MethodDef method, Random rng, int count)
        {
            var instructions = method.Body.Instructions;
            Console.WriteLine($"      [OPERACIONES] Insertando {count} operaciones caóticas en {method.Name}");
            
            int operationsInserted = 0;
            for (int i = 0; i < Math.Min(count, 10); i++)
            {
                // Encontrar punto de inserción seguro
                int insertPoint = rng.Next(1, Math.Max(2, instructions.Count - 1));
                
                // Insertar operaciones simples stack-neutral
                try
                {
                    var val1 = rng.Next(1, 10000);
                    var val2 = rng.Next(1, 1000);
                    Console.WriteLine($"      [OPERACIONES] Insertando XOR({val1}, {val2}) en posición {insertPoint}");
                    
                    instructions.Insert(insertPoint, Instruction.Create(OpCodes.Ldc_I4, val1));
                    instructions.Insert(insertPoint + 1, Instruction.Create(OpCodes.Ldc_I4, val2));
                    instructions.Insert(insertPoint + 2, Instruction.Create(OpCodes.Xor));
                    instructions.Insert(insertPoint + 3, Instruction.Create(OpCodes.Pop)); // Descartar resultado
                    operationsInserted++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"      [OPERACIONES] ⚠ Error insertando operación {i}: {ex.Message}");
                    break;
                }
            }
            
            Console.WriteLine($"      [OPERACIONES] ✓ {operationsInserted} operaciones caóticas insertadas exitosamente");
        }
        
        // NUEVO: Crear métodos de ruido extremo para máxima confusión
        private static List<MethodDefUser> CreateExtremeNoiseMethods(ModuleDefMD mod, ProtectionData data, int count)
        {
            var methods = new List<MethodDefUser>();
            var rng = new Random(data.Seed + 9999);
            
            for (int i = 0; i < count; i++)
            {
                // Crear signature primero
                var methodSig = MethodSig.CreateStatic(mod.CorLibTypes.Int32, 
                    mod.CorLibTypes.Int32, mod.CorLibTypes.Int32, mod.CorLibTypes.Int32);
                
                var method = new MethodDefUser($"NoiseMethod_{rng.Next(10000, 99999)}_{i}",
                    methodSig,
                    MethodAttributes.Public | MethodAttributes.Static);
                
                method.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                
                // Crear parámetros correctamente
                var param1 = new ParamDefUser("x", 1);
                var param2 = new ParamDefUser("y", 2);
                var param3 = new ParamDefUser("z", 3);
                
                method.ParamDefs.Add(param1);
                method.ParamDefs.Add(param2);
                method.ParamDefs.Add(param3);
                
                method.Body = new CilBody();
                method.Body.Instructions.Clear();
                
                // Crear ruido extremo con múltiples operaciones complejas
                CreateExtremeNoiseOperations(method.Body.Instructions, rng, rng.Next(50, 200));
                
                // Agregar variables locales para más confusión
                for (int v = 0; v < rng.Next(10, 30); v++)
                {
                    method.Body.Variables.Add(new Local(mod.CorLibTypes.Int32));
                }
                
                methods.Add(method);
            }
            
            return methods;
        }
        
        private static void CreateExtremeNoiseOperations(IList<Instruction> instructions, Random rng, int complexity)
        {
            // Crear operaciones extremadamente complejas y confusas
            for (int i = 0; i < Math.Min(complexity, 150); i++)
            {
                // Operaciones matemáticas complejas
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                instructions.Add(Instruction.Create(OpCodes.Mul));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 10000)));
                instructions.Add(Instruction.Create(OpCodes.Add));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 5000)));
                instructions.Add(Instruction.Create(OpCodes.Rem));
                instructions.Add(Instruction.Create(OpCodes.Or));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
                
                // Operaciones de bit shifting
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 31)));
                instructions.Add(Instruction.Create(OpCodes.Shl));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 31)));
                instructions.Add(Instruction.Create(OpCodes.Shr));
                instructions.Add(Instruction.Create(OpCodes.And));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
                
                // Operaciones de negación y complemento
                instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                instructions.Add(Instruction.Create(OpCodes.Not));
                instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                instructions.Add(Instruction.Create(OpCodes.Neg));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Pop)); // Descartar resultado
            }
            
            // Retorno con operación compleja
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            instructions.Add(Instruction.Create(OpCodes.Mul));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 99999)));
            instructions.Add(Instruction.Create(OpCodes.Rem));
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }
        
        // NUEVO: Aplicar obfuscación ULTRA-extrema con máxima agresividad
        private static void ApplyUltraExtremeChaosObfuscation(MethodDef method, ModuleDefMD mod, ProtectionData data, 
            List<MethodDefUser> dispatchers, List<MethodDefUser> fakeStateMethods, List<MethodDefUser> confusionMethods, 
            List<MethodDefUser> noiseMethods, Random rng)
        {
            if (!method.HasBody || method.Body.Instructions.Count < 2) return;
            
            Console.WriteLine($"    [ULTRA-CAOS] Iniciando obfuscación ULTRA-extrema en: {method.Name}");
            method.Body.KeepOldMaxStack = true;
            var instructions = method.Body.Instructions;
            var originalCount = instructions.Count;
            
            // Insertar llamadas masivas en MUCHOS más puntos
            var insertionPoints = new List<int>();
            for (int i = 1; i < originalCount - 1; i += rng.Next(1, 3)) // Más frecuente
            {
                insertionPoints.Add(i);
            }
            
            Console.WriteLine($"    [ULTRA-CAOS] Puntos de inserción MASIVOS identificados: {insertionPoints.Count}");
            
            int insertionOffset = 0;
            int dispatcherCalls = 0;
            int fakeStateCalls = 0;
            int confusionCalls = 0;
            int noiseCalls = 0;
            
            foreach (var point in insertionPoints)
            {
                var adjustedPoint = point + insertionOffset;
                if (adjustedPoint >= instructions.Count) break;
                
                // Insertar llamadas a dispatchers (probabilidad aumentada)
                if (dispatchers.Count > 0 && rng.Next(100) < 90) // 90% probabilidad
                {
                    var dispatcher = dispatchers[rng.Next(dispatchers.Count)];
                    Console.WriteLine($"    [ULTRA-CAOS] Insertando dispatcher MASIVO: {dispatcher.Name} en posición {adjustedPoint}");
                    instructions.Insert(adjustedPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                    instructions.Insert(adjustedPoint + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                    instructions.Insert(adjustedPoint + 2, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                    instructions.Insert(adjustedPoint + 3, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                    instructions.Insert(adjustedPoint + 4, Instruction.Create(OpCodes.Call, dispatcher));
                    instructions.Insert(adjustedPoint + 5, Instruction.Create(OpCodes.Pop));
                    insertionOffset += 6;
                    dispatcherCalls++;
                }
                
                // Insertar llamadas a métodos de estado falsos (probabilidad aumentada)
                if (fakeStateMethods.Count > 0 && rng.Next(100) < 85) // 85% probabilidad
                {
                    var fakeMethod = fakeStateMethods[rng.Next(fakeStateMethods.Count)];
                    var currentPoint = adjustedPoint + insertionOffset;
                    if (currentPoint < instructions.Count)
                    {
                        Console.WriteLine($"    [ULTRA-CAOS] Insertando método falso MASIVO: {fakeMethod.Name} en posición {currentPoint}");
                        instructions.Insert(currentPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                        instructions.Insert(currentPoint + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                        instructions.Insert(currentPoint + 2, Instruction.Create(OpCodes.Call, fakeMethod));
                        instructions.Insert(currentPoint + 3, Instruction.Create(OpCodes.Pop));
                        insertionOffset += 4;
                        fakeStateCalls++;
                    }
                }
                
                // Insertar llamadas a métodos de confusión (probabilidad aumentada)
                if (confusionMethods.Count > 0 && rng.Next(100) < 75) // 75% probabilidad
                {
                    var confusionMethod = confusionMethods[rng.Next(confusionMethods.Count)];
                    var currentPoint = adjustedPoint + insertionOffset;
                    if (currentPoint < instructions.Count)
                    {
                        Console.WriteLine($"    [ULTRA-CAOS] Insertando método confusión MASIVO: {confusionMethod.Name} en posición {currentPoint}");
                        instructions.Insert(currentPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 50000)));
                        instructions.Insert(currentPoint + 1, Instruction.Create(OpCodes.Call, confusionMethod));
                        insertionOffset += 2;
                        confusionCalls++;
                    }
                }
                
                // NUEVO: Insertar llamadas a métodos de ruido extremo
                if (noiseMethods.Count > 0 && rng.Next(100) < 80) // 80% probabilidad
                {
                    var noiseMethod = noiseMethods[rng.Next(noiseMethods.Count)];
                    var currentPoint = adjustedPoint + insertionOffset;
                    if (currentPoint < instructions.Count)
                    {
                        Console.WriteLine($"    [ULTRA-CAOS] Insertando método RUIDO EXTREMO: {noiseMethod.Name} en posición {currentPoint}");
                        instructions.Insert(currentPoint, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 99999)));
                        instructions.Insert(currentPoint + 1, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 99999)));
                        instructions.Insert(currentPoint + 2, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 99999)));
                        instructions.Insert(currentPoint + 3, Instruction.Create(OpCodes.Call, noiseMethod));
                        instructions.Insert(currentPoint + 4, Instruction.Create(OpCodes.Pop));
                        insertionOffset += 5;
                        noiseCalls++;
                    }
                }
            }
            
            Console.WriteLine($"    [ULTRA-CAOS] Llamadas MASIVAS insertadas - Dispatchers: {dispatcherCalls}, Estados falsos: {fakeStateCalls}, Confusión: {confusionCalls}, Ruido: {noiseCalls}");
            
            // Insertar MUCHAS más operaciones caóticas
            var chaosOpsCount = Math.Min(50, originalCount); // Aumentado significativamente
            Console.WriteLine($"    [ULTRA-CAOS] Insertando {chaosOpsCount} operaciones caóticas ULTRA-extremas");
            InsertUltraExtremeOperations(method, rng, chaosOpsCount);
            
            try
            {
                Console.WriteLine($"    [ULTRA-CAOS] Optimizando branches y actualizando offsets...");
                method.Body.SimplifyBranches();
                method.Body.OptimizeBranches();
                method.Body.UpdateInstructionOffsets();
                Console.WriteLine($"    [ULTRA-CAOS] ✓ Optimización ULTRA-extrema completada");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    [ULTRA-CAOS] ⚠ Error en optimización: {ex.Message}");
                method.Body.KeepOldMaxStack = true;
            }
        }
        
        private static void InsertUltraExtremeOperations(MethodDef method, Random rng, int count)
        {
            var instructions = method.Body.Instructions;
            Console.WriteLine($"      [ULTRA-OPERACIONES] Insertando {count} operaciones ULTRA-extremas en {method.Name}");
            
            int operationsInserted = 0;
            for (int i = 0; i < Math.Min(count, 30); i++) // Aumentado de 10 a 30
            {
                int insertPoint = rng.Next(1, Math.Max(2, instructions.Count - 1));
                
                try
                {
                    var val1 = rng.Next(1, 99999);
                    var val2 = rng.Next(1, 50000);
                    var val3 = rng.Next(1, 25000);
                    Console.WriteLine($"      [ULTRA-OPERACIONES] Insertando operación compleja ({val1}, {val2}, {val3}) en posición {insertPoint}");
                    
                    // Operaciones más complejas y confusas
                    instructions.Insert(insertPoint, Instruction.Create(OpCodes.Ldc_I4, val1));
                    instructions.Insert(insertPoint + 1, Instruction.Create(OpCodes.Ldc_I4, val2));
                    instructions.Insert(insertPoint + 2, Instruction.Create(OpCodes.Mul));
                    instructions.Insert(insertPoint + 3, Instruction.Create(OpCodes.Ldc_I4, val3));
                    instructions.Insert(insertPoint + 4, Instruction.Create(OpCodes.Xor));
                    instructions.Insert(insertPoint + 5, Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 31)));
                    instructions.Insert(insertPoint + 6, Instruction.Create(OpCodes.Shl));
                    instructions.Insert(insertPoint + 7, Instruction.Create(OpCodes.Pop)); // Descartar resultado
                    operationsInserted++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"      [ULTRA-OPERACIONES] ⚠ Error insertando operación {i}: {ex.Message}");
                    break;
                }
            }
            
            Console.WriteLine($"      [ULTRA-OPERACIONES] ✓ {operationsInserted} operaciones ULTRA-extremas insertadas exitosamente");
        }
    }
}