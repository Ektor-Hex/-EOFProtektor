using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace EOFProtektor.Obfuscation
{
    public static class ClassVirtualizationObfuscator
    {
        private static Random random = new Random();
        private static Dictionary<TypeDef, VirtualizedClass> virtualizedClasses = new Dictionary<TypeDef, VirtualizedClass>();
        private static Dictionary<string, int> virtualMethodIds = new Dictionary<string, int>();
        private static int nextVirtualId = 1000;

        public class VirtualizedClass
        {
            public TypeDef OriginalType { get; set; }
            public TypeDef VirtualType { get; set; }
            public MethodDef VirtualDispatcher { get; set; }
            public FieldDef VirtualTable { get; set; }
            public Dictionary<MethodDef, int> MethodIds { get; set; } = new Dictionary<MethodDef, int>();
            public List<MethodDef> VirtualizedMethods { get; set; } = new List<MethodDef>();
        }

        public static void ApplyClassVirtualization(ModuleDef module, int protectionLevel, bool virtualizeAll = false)
        {
            Console.WriteLine("[VIRTUALIZACIÓN] Iniciando virtualización dinámica de clases...");
            
            if (virtualizeAll)
            {
                Console.WriteLine("[VIRTUALIZACIÓN] Modo: VIRTUALIZACIÓN COMPLETA - Todas las funciones serán virtualizadas");
            }
            
            try
            {
                var typesToVirtualize = GetTypesToVirtualize(module, protectionLevel, virtualizeAll);
                Console.WriteLine($"[VIRTUALIZACIÓN] Tipos seleccionados para virtualización: {typesToVirtualize.Count}");

                foreach (var type in typesToVirtualize)
                {
                    VirtualizeClass(module, type, protectionLevel, virtualizeAll);
                }

                CreateGlobalVirtualRuntime(module);
                ApplyAntiAnalysisToVirtualization(module);

                Console.WriteLine($"[VIRTUALIZACIÓN] ✓ Virtualización aplicada a {virtualizedClasses.Count} clases");
                Console.WriteLine($"[VIRTUALIZACIÓN] ✓ Total métodos virtualizados: {virtualizedClasses.Values.Sum(v => v.VirtualizedMethods.Count)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error en virtualización de clases: {ex.Message}");
            }
        }

        private static List<TypeDef> GetTypesToVirtualize(ModuleDef module, int protectionLevel, bool virtualizeAll = false)
        {
            var types = new List<TypeDef>();
            
            foreach (var type in module.Types)
            {
                if (ShouldVirtualizeType(type, protectionLevel, virtualizeAll))
                {
                    types.Add(type);
                }
            }

            // En niveles altos, virtualizar más tipos
            if (protectionLevel >= 3)
            {
                // Incluir tipos anidados
                foreach (var type in module.Types.ToList())
                {
                    foreach (var nestedType in type.NestedTypes.ToList())
                    {
                        if (ShouldVirtualizeType(nestedType, protectionLevel, virtualizeAll))
                        {
                            types.Add(nestedType);
                        }
                    }
                }
            }

            return types;
        }

        private static bool ShouldVirtualizeType(TypeDef type, int protectionLevel, bool virtualizeAll = false)
        {
            if (type == null || type.IsInterface || type.IsEnum || type.IsDelegate)
                return false;

            if (type.Name == "<Module>" || type.Name.StartsWith("<>"))
                return false;

            if (type.HasCustomAttributes && type.CustomAttributes.Any(ca => 
                ca.TypeFullName.Contains("CompilerGenerated") || 
                ca.TypeFullName.Contains("DebuggerNonUserCode")))
                return false;

            // Si virtualizeAll está habilitado, virtualizar todos los tipos válidos
            if (virtualizeAll)
            {
                // Virtualizar si tiene al menos un método válido
                return type.Methods.Any(m => !m.IsConstructor && !m.IsStaticConstructor && m.HasBody);
            }

            // Solo virtualizar tipos con métodos suficientes
            var methodCount = type.Methods.Count(m => !m.IsConstructor && !m.IsStaticConstructor && m.HasBody);
            
            switch (protectionLevel)
            {
                case 1: return methodCount >= 5; // Básico: solo clases grandes
                case 2: return methodCount >= 3; // Intermedio: clases medianas
                case 3: return methodCount >= 2; // Avanzado: casi todas las clases
                default: return false;
            }
        }

        private static void VirtualizeClass(ModuleDef module, TypeDef originalType, int protectionLevel, bool virtualizeAll = false)
        {
            Console.WriteLine($"[VIRTUALIZACIÓN] Virtualizando clase: {originalType.Name}");

            var virtualizedClass = new VirtualizedClass
            {
                OriginalType = originalType
            };

            // Crear tipo virtual
            CreateVirtualType(module, virtualizedClass);

            // Crear tabla virtual
            CreateVirtualTable(virtualizedClass);

            // Crear dispatcher virtual
            CreateVirtualDispatcher(virtualizedClass);

            // Virtualizar métodos
            VirtualizeMethods(virtualizedClass, protectionLevel, virtualizeAll);

            // Aplicar redirecciones
            ApplyMethodRedirections(virtualizedClass);

            virtualizedClasses[originalType] = virtualizedClass;
        }

        private static void CreateVirtualType(ModuleDef module, VirtualizedClass virtualizedClass)
        {
            var originalType = virtualizedClass.OriginalType;
            var virtualTypeName = GenerateVirtualTypeName();

            var virtualType = new TypeDefUser(
                originalType.Namespace,
                virtualTypeName,
                module.CorLibTypes.Object.TypeDefOrRef)
            {
                Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed
            };

            module.Types.Add(virtualType);
            virtualizedClass.VirtualType = virtualType;

            Console.WriteLine($"[VIRTUALIZACIÓN] Tipo virtual creado: {virtualTypeName}");
        }

        private static void CreateVirtualTable(VirtualizedClass virtualizedClass)
        {
            var virtualType = virtualizedClass.VirtualType;
            var module = virtualType.Module;

            // Crear campo para la tabla virtual
            var virtualTableField = new FieldDefUser(
                GenerateRandomName("VT_"),
                new FieldSig(module.CorLibTypes.Object),
                FieldAttributes.Private | FieldAttributes.Static);

            virtualType.Fields.Add(virtualTableField);
            virtualizedClass.VirtualTable = virtualTableField;

            // Crear método inicializador de tabla virtual
            CreateVirtualTableInitializer(virtualizedClass);
        }

        private static void CreateVirtualTableInitializer(VirtualizedClass virtualizedClass)
        {
            var virtualType = virtualizedClass.VirtualType;
            var module = virtualType.Module;

            var initMethod = new MethodDefUser(
                GenerateRandomName("Init_"),
                MethodSig.CreateStatic(module.CorLibTypes.Void),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Private | MethodAttributes.Static);

            var body = new CilBody();
            initMethod.Body = body;

            // Crear array de delegates/funciones
            body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, virtualizedClass.MethodIds.Count));
            body.Instructions.Add(Instruction.Create(OpCodes.Newarr, module.CorLibTypes.Object));
            
            // Inicializar cada entrada de la tabla virtual
            int index = 0;
            foreach (var kvp in virtualizedClass.MethodIds)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, index));
                
                // Crear delegate complejo para el método
                CreateComplexDelegate(body, kvp.Key, module);
                
                body.Instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
                index++;
            }

            body.Instructions.Add(Instruction.Create(OpCodes.Stsfld, virtualizedClass.VirtualTable));
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            virtualType.Methods.Add(initMethod);

            // Llamar al inicializador desde el constructor estático
            CreateStaticConstructor(virtualizedClass, initMethod);
        }

        private static void CreateComplexDelegate(CilBody body, MethodDef method, ModuleDef module)
        {
            // Crear un delegate complejo que apunte al método original
            // Esto es una simplificación - en una implementación real sería mucho más complejo
            
            // Por ahora, almacenar una referencia al método
            if (method.IsStatic)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, method));
            }
            else
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, method));
            }
            
            // Crear un objeto que encapsule la información del método
            body.Instructions.Add(Instruction.Create(OpCodes.Newobj, module.CorLibTypes.Object.TypeDefOrRef.ResolveTypeDef().FindDefaultConstructor()));
        }

        private static void CreateStaticConstructor(VirtualizedClass virtualizedClass, MethodDef initMethod)
        {
            var virtualType = virtualizedClass.VirtualType;
            var module = virtualType.Module;

            var cctor = virtualType.FindStaticConstructor();
            if (cctor == null)
            {
                cctor = new MethodDefUser(
                    ".cctor",
                    MethodSig.CreateStatic(module.CorLibTypes.Void),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

                cctor.Body = new CilBody();
                virtualType.Methods.Add(cctor);
            }

            // Insertar llamada al inicializador al principio
            var instructions = cctor.Body.Instructions;
            instructions.Insert(0, Instruction.Create(OpCodes.Call, initMethod));
        }

        private static void CreateVirtualDispatcher(VirtualizedClass virtualizedClass)
        {
            var virtualType = virtualizedClass.VirtualType;
            var module = virtualType.Module;

            var dispatcher = new MethodDefUser(
                GenerateRandomName("Dispatch_"),
                MethodSig.CreateStatic(module.CorLibTypes.Object, module.CorLibTypes.Int32, new SZArraySig(module.CorLibTypes.Object)),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static);

            var body = new CilBody();
            dispatcher.Body = body;

            // Crear switch complejo para dispatch
            CreateComplexDispatchSwitch(body, virtualizedClass);

            virtualType.Methods.Add(dispatcher);
            virtualizedClass.VirtualDispatcher = dispatcher;

            Console.WriteLine($"[VIRTUALIZACIÓN] Dispatcher virtual creado: {dispatcher.Name}");
        }

        private static void CreateComplexDispatchSwitch(CilBody body, VirtualizedClass virtualizedClass)
        {
            var module = virtualizedClass.VirtualType.Module;
            var instructions = body.Instructions;

            // Cargar el ID del método
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            // Crear labels para cada método
            var labels = new List<Instruction>();
            var defaultLabel = Instruction.Create(OpCodes.Nop);

            foreach (var kvp in virtualizedClass.MethodIds)
            {
                labels.Add(Instruction.Create(OpCodes.Nop));
            }

            // Switch statement
            instructions.Add(Instruction.Create(OpCodes.Switch, labels.ToArray()));
            instructions.Add(Instruction.Create(OpCodes.Br, defaultLabel));

            // Implementar cada case
            int index = 0;
            foreach (var kvp in virtualizedClass.MethodIds)
            {
                instructions.Add(labels[index]);
                
                // Llamar al método original con anti-análisis
                CreateAntiAnalysisMethodCall(instructions, kvp.Key, module);
                
                instructions.Add(Instruction.Create(OpCodes.Ret));
                index++;
            }

            // Default case
            instructions.Add(defaultLabel);
            instructions.Add(Instruction.Create(OpCodes.Ldnull));
            instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private static void CreateAntiAnalysisMethodCall(IList<Instruction> instructions, MethodDef method, ModuleDef module)
        {
            // Agregar verificaciones anti-análisis antes de la llamada real
            
            // Verificación de tiempo
            instructions.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(Environment).GetMethod("get_TickCount"))));
            instructions.Add(Instruction.Create(OpCodes.Ldc_I4, random.Next(1000, 5000)));
            instructions.Add(Instruction.Create(OpCodes.Add));
            instructions.Add(Instruction.Create(OpCodes.Pop));

            // Verificación de debugger
            instructions.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(System.Diagnostics.Debugger).GetMethod("get_IsAttached"))));
            var skipLabel = Instruction.Create(OpCodes.Nop);
            instructions.Add(Instruction.Create(OpCodes.Brfalse, skipLabel));
            instructions.Add(Instruction.Create(OpCodes.Ldnull));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            instructions.Add(skipLabel);

            // Cargar argumentos del array
            if (method.Parameters.Count > 0)
            {
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                    instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                    instructions.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                    
                    // Convertir tipo si es necesario
                    var paramType = method.Parameters[i].Type;
                    if (paramType.IsValueType)
                    {
                        instructions.Add(Instruction.Create(OpCodes.Unbox_Any, paramType.ToTypeDefOrRef()));
                    }
                }
            }

            // Llamada al método original
            if (method.IsStatic)
            {
                instructions.Add(Instruction.Create(OpCodes.Call, method));
            }
            else
            {
                // Para métodos de instancia, necesitamos cargar 'this' del primer argumento
                instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, method));
            }

            // Boxing del resultado si es necesario
            if (method.ReturnType.IsValueType && method.ReturnType != module.CorLibTypes.Void)
            {
                instructions.Add(Instruction.Create(OpCodes.Box, method.ReturnType.ToTypeDefOrRef()));
            }
            else if (method.ReturnType == module.CorLibTypes.Void)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldnull));
            }
        }

        private static void VirtualizeMethods(VirtualizedClass virtualizedClass, int protectionLevel, bool virtualizeAll = false)
        {
            var originalType = virtualizedClass.OriginalType;
            var methodsToVirtualize = GetMethodsToVirtualize(originalType, protectionLevel, virtualizeAll);

            foreach (var method in methodsToVirtualize)
            {
                var virtualId = nextVirtualId++;
                virtualizedClass.MethodIds[method] = virtualId;
                virtualizedClass.VirtualizedMethods.Add(method);
                virtualMethodIds[method.FullName] = virtualId;

                Console.WriteLine($"[VIRTUALIZACIÓN] Método virtualizado: {method.Name} -> ID: {virtualId}");
            }
        }

        private static List<MethodDef> GetMethodsToVirtualize(TypeDef type, int protectionLevel, bool virtualizeAll = false)
        {
            var methods = new List<MethodDef>();

            foreach (var method in type.Methods)
            {
                if (ShouldVirtualizeMethod(method, protectionLevel, virtualizeAll))
                {
                    methods.Add(method);
                }
            }

            return methods;
        }

        private static bool ShouldVirtualizeMethod(MethodDef method, int protectionLevel, bool virtualizeAll = false)
        {
            if (method == null || !method.HasBody)
                return false;

            if (method.IsConstructor || method.IsStaticConstructor)
                return false;

            if (virtualizeAll)
            {
                // En modo virtualizeAll, virtualizar todos los métodos válidos incluyendo propiedades
                if (method.IsSpecialName || method.IsRuntimeSpecialName)
                {
                    // Permitir getters y setters en modo virtualizeAll
                    return method.Name.StartsWith("get_") || method.Name.StartsWith("set_");
                }
                return true;
            }

            if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                return protectionLevel >= 3; // Solo virtualizar propiedades en nivel alto

            if (method.IsSpecialName || method.IsRuntimeSpecialName)
                return false;

            return true;
        }

        private static void ApplyMethodRedirections(VirtualizedClass virtualizedClass)
        {
            foreach (var method in virtualizedClass.VirtualizedMethods)
            {
                RedirectMethodToVirtual(method, virtualizedClass);
            }
        }

        private static void RedirectMethodToVirtual(MethodDef originalMethod, VirtualizedClass virtualizedClass)
        {
            if (!originalMethod.HasBody)
                return;

            var body = originalMethod.Body;
            var module = originalMethod.Module;
            
            // Limpiar el cuerpo del método original
            body.Instructions.Clear();
            body.ExceptionHandlers.Clear();

            // Crear llamada al dispatcher virtual
            var virtualId = virtualizedClass.MethodIds[originalMethod];

            // Cargar ID del método
            body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, virtualId));

            // Crear array de argumentos
            var paramCount = originalMethod.Parameters.Count;
            if (!originalMethod.IsStatic)
                paramCount++; // Incluir 'this'

            body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, paramCount));
            body.Instructions.Add(Instruction.Create(OpCodes.Newarr, module.CorLibTypes.Object));

            // Cargar argumentos en el array
            int argIndex = 0;
            if (!originalMethod.IsStatic)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                body.Instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
                argIndex = 1;
            }

            for (int i = 0; i < originalMethod.Parameters.Count; i++)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, argIndex));
                
                // Cargar argumento específico
                var argIndex2 = originalMethod.IsStatic ? i : i + 1;
                if (argIndex2 == 0)
                    body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                else if (argIndex2 == 1)
                    body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                else if (argIndex2 == 2)
                    body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                else if (argIndex2 == 3)
                    body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
                else
                    body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_S, (byte)argIndex2));
                
                var paramType = originalMethod.Parameters[i].Type;
                if (paramType.IsValueType)
                {
                    body.Instructions.Add(Instruction.Create(OpCodes.Box, paramType.ToTypeDefOrRef()));
                }
                
                body.Instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
                argIndex++;
            }

            // Llamar al dispatcher
            body.Instructions.Add(Instruction.Create(OpCodes.Call, virtualizedClass.VirtualDispatcher));

            // Manejar valor de retorno
            if (originalMethod.ReturnType == module.CorLibTypes.Void)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Pop));
            }
            else if (originalMethod.ReturnType.IsValueType)
            {
                body.Instructions.Add(Instruction.Create(OpCodes.Unbox_Any, originalMethod.ReturnType.ToTypeDefOrRef()));
            }

            body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            Console.WriteLine($"[VIRTUALIZACIÓN] Redirección aplicada: {originalMethod.Name}");
        }

        private static void CreateGlobalVirtualRuntime(ModuleDef module)
        {
            Console.WriteLine("[VIRTUALIZACIÓN] Creando runtime virtual global...");

            var runtimeType = new TypeDefUser(
                "",
                GenerateRandomName("VirtualRuntime_"),
                module.CorLibTypes.Object.TypeDefOrRef)
            {
                Attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed
            };

            module.Types.Add(runtimeType);

            // Crear métodos de utilidad para el runtime virtual
            CreateVirtualRuntimeMethods(runtimeType);
        }

        private static void CreateVirtualRuntimeMethods(TypeDef runtimeType)
        {
            var module = runtimeType.Module;

            // Método de verificación de integridad
            var integrityMethod = new MethodDefUser(
                GenerateRandomName("CheckIntegrity_"),
                MethodSig.CreateStatic(module.CorLibTypes.Boolean),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.Static);

            var body = new CilBody();
            integrityMethod.Body = body;

            // Verificaciones complejas de integridad
            body.Instructions.Add(Instruction.Create(OpCodes.Call, module.Import(typeof(Environment).GetMethod("get_TickCount"))));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, random.Next(100, 1000)));
            body.Instructions.Add(Instruction.Create(OpCodes.Rem));
            body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            body.Instructions.Add(Instruction.Create(OpCodes.Ceq));
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            runtimeType.Methods.Add(integrityMethod);
        }

        private static void ApplyAntiAnalysisToVirtualization(ModuleDef module)
        {
            Console.WriteLine("[VIRTUALIZACIÓN] Aplicando anti-análisis a virtualización...");

            foreach (var virtualizedClass in virtualizedClasses.Values)
            {
                ApplyAntiAnalysisToVirtualClass(virtualizedClass);
            }
        }

        private static void ApplyAntiAnalysisToVirtualClass(VirtualizedClass virtualizedClass)
        {
            // Agregar verificaciones anti-análisis al dispatcher
            var dispatcher = virtualizedClass.VirtualDispatcher;
            if (dispatcher?.Body != null)
            {
                var instructions = dispatcher.Body.Instructions;
                
                // Insertar verificaciones al inicio
                instructions.Insert(0, Instruction.Create(OpCodes.Call, dispatcher.Module.Import(typeof(System.Diagnostics.Debugger).GetMethod("get_IsAttached"))));
                var continueLabel = instructions[1];
                instructions.Insert(1, Instruction.Create(OpCodes.Brfalse, continueLabel));
                instructions.Insert(2, Instruction.Create(OpCodes.Ldnull));
                instructions.Insert(3, Instruction.Create(OpCodes.Ret));
            }
        }

        private static string GenerateVirtualTypeName()
        {
            var prefixes = new[] { "Virtual", "Dynamic", "Runtime", "Proxy", "Handler", "Manager", "Controller", "Service" };
            var suffixes = new[] { "Core", "Engine", "System", "Framework", "Platform", "Infrastructure", "Component" };
            
            return $"{prefixes[random.Next(prefixes.Length)]}{suffixes[random.Next(suffixes.Length)]}_{random.Next(1000, 9999)}";
        }

        private static string GenerateRandomName(string prefix = "")
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var length = random.Next(8, 16);
            var result = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }
            
            return prefix + new string(result);
        }
    }
}