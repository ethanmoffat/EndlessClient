using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EOBot.Interpreter.States;

namespace EOBot.Interpreter
{
    public static class EnumTypeCatalog
    {
        private static readonly Lazy<IReadOnlyList<Type>> _enumTypes = new(LoadEnumTypes);

        public static IReadOnlyList<Type> EnumTypes => _enumTypes.Value;

        public static IReadOnlyList<Type> FindByName(string typeName, string ns = null)
        {
            return EnumTypes
                .Where(x => x.Name == typeName && (ns == null || x.Namespace == ns))
                .ToList();
        }

        public static IReadOnlyList<Type> FindByNamespace(string ns)
        {
            return EnumTypes.Where(x => x.Namespace == ns).ToList();
        }

        private static IReadOnlyList<Type> LoadEnumTypes()
        {
            var assemblies = new Dictionary<string, Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                assemblies.TryAdd(assembly.FullName, assembly);

            var pending = new Queue<Assembly>([typeof(ProgramState).Assembly]);
            var visited = new HashSet<string>();
            while (pending.Count > 0)
            {
                var next = pending.Dequeue();
                if (!visited.Add(next.FullName))
                    continue;

                assemblies.TryAdd(next.FullName, next);

                foreach (var reference in next.GetReferencedAssemblies())
                {
                    if (visited.Contains(reference.FullName))
                        continue;

                    try
                    {
                        pending.Enqueue(Assembly.Load(reference));
                    }
                    catch (Exception ex)
                        when (ex is FileNotFoundException
                                 or FileLoadException
                                 or BadImageFormatException)
                    {
                    }
                }
            }

            return assemblies.Values
                .Where(x => !x.IsDynamic)
                .SelectMany(GetLoadableTypes)
                .Where(x => x.IsEnum && x.IsPublic)
                .Distinct()
                .ToList();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
            catch (Exception ex)
                when (ex is NotSupportedException
                         or FileNotFoundException
                         or FileLoadException)
            {
                return [];
            }
        }
    }
}
