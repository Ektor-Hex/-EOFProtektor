using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using EOFProtektor.Core;
using EOFProtektor.Utils;

namespace EOFProtektor.Protection
{
    public static class AntiDebugProtection
    {
        public static void InjectUltimateAntiDebugClass(ModuleDefMD mod, ProtectionData data)
        {
            var antiDebugType = new TypeDefUser("", NameObfuscator.GenerateObfuscatedName("AntiDebug", data.Seed + 3000), 
                mod.CorLibTypes.Object.TypeDefOrRef);
            antiDebugType.Attributes = TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed;
            
            mod.Types.Add(antiDebugType);
            
            // Crear métodos anti-debug
            var destroyMethod = CreateDestroyDebuggerMethod(mod, antiDebugType, data);
            var detectMethod = CreateDetectDebuggerHookMethod(mod, antiDebugType, data);
            var verifyMethod = CreateVerifyCLRIntegrityMethod(mod, antiDebugType, data);
            
            antiDebugType.Methods.Add(destroyMethod);
            antiDebugType.Methods.Add(detectMethod);
            antiDebugType.Methods.Add(verifyMethod);
        }
        
        private static MethodDefUser CreateDestroyDebuggerMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data)
        {
            var method = new MethodDefUser(NameObfuscator.GenerateObfuscatedName("DestroyDebugger", data.Seed + 4000),
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;
            
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean));
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed + 1000);
            
            // Código aparatoso anti-debug
            for (int i = 0; i < 10; i++)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
                instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(100, 999)));
                instructions.Add(Instruction.Create(OpCodes.Xor));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 0x12345));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Stloc_1));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                instructions.Add(Instruction.Create(OpCodes.Pop));
            }
            
            instructions.Add(Instruction.Create(OpCodes.Ret));
            return method;
        }
        
        private static MethodDefUser CreateDetectDebuggerHookMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data)
        {
            var method = new MethodDefUser(NameObfuscator.GenerateObfuscatedName("DetectHook", data.Seed + 5000),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;
            
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            body.Variables.Add(new Local(mod.CorLibTypes.Boolean));
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed + 2000);
            
            // Simulación de detección de hooks
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1, 100)));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 50));
            instructions.Add(Instruction.Create(OpCodes.Cgt));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }
        
        private static MethodDefUser CreateVerifyCLRIntegrityMethod(ModuleDefMD mod, TypeDef parentType, ProtectionData data)
        {
            var method = new MethodDefUser(NameObfuscator.GenerateObfuscatedName("VerifyIntegrity", data.Seed + 6000),
                MethodSig.CreateStatic(mod.CorLibTypes.Boolean),
                MethodAttributes.Static | MethodAttributes.Private);

            var body = new CilBody();
            method.Body = body;
            
            body.Variables.Add(new Local(mod.CorLibTypes.Int32));
            
            var instructions = body.Instructions;
            var rng = new Random(data.Seed + 3000);
            
            // Verificación aparatosa de integridad
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, rng.Next(1000, 9999)));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 0x1337));
            instructions.Add(Instruction.Create(OpCodes.Xor));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            instructions.Add(Instruction.Create(OpCodes.Cgt));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            
            return method;
        }
    }
}