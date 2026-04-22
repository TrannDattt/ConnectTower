using System;
using System.Collections.Generic;

namespace Assets._Scripts.Patterns.EventBus
{
    public static class PredefinedAssemblyUtil
    {
        enum EAssemblyType
        {
            AssemblyCSharp,
            AssemblyCSharpEditor,
            AssemblyCSharpFirstpass,
            AssemblyCSharpEditorFirstpass
        }

        private static EAssemblyType? GetAssemblyType(string assemblyName)
        {
            return assemblyName switch
            {
                "Assembly-CSharp" => EAssemblyType.AssemblyCSharp,
                "Assembly-CSharp-Editor" => EAssemblyType.AssemblyCSharpEditor,
                "Assembly-CSharp-Firstpass" => EAssemblyType.AssemblyCSharpFirstpass,
                "Assembly-CSharp-Editor-Firstpass" => EAssemblyType.AssemblyCSharpEditorFirstpass,
                _ => null
            };
        }

        private static void AddTypesFromAssembly(Type interfaceType, ICollection<Type> types, Type[] assembly)
        {
            if (assembly == null) return;
            foreach (var type in assembly)
            {
                if (interfaceType.IsAssignableFrom(type) && type != interfaceType)
                {
                    types.Add(type);
                }
            }
        }

        public static List<Type> GetTypes(Type interfaceType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<EAssemblyType, Type[]> assemblyTypes = new();
            var types = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var assemblyType = GetAssemblyType(assembly.GetName().Name);
                if (assemblyType != null) 
                {
                    assemblyTypes[assemblyType.Value] = assembly.GetTypes();
                }
            }

            AddTypesFromAssembly(interfaceType, types, assemblyTypes.GetValueOrDefault(EAssemblyType.AssemblyCSharp));
            AddTypesFromAssembly(interfaceType, types, assemblyTypes.GetValueOrDefault(EAssemblyType.AssemblyCSharpFirstpass));

            return types;
        }
    }
}
