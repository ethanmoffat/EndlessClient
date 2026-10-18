using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EOBot.Interpreter.Hosting
{
    /// <summary>
    /// The public .NET types (classes and enums) that scripts can map.
    /// </summary>
    public class TypeCatalog
    {
        public IReadOnlyList<Type> Types { get; }

        private TypeCatalog(IEnumerable<Assembly> assemblies)
        {
            Types = assemblies
                .Where(x => !x.IsDynamic)
                .SelectMany(GetLoadableTypes)
                .Where(x => x.IsPublic && (x.IsEnum || x.IsClass))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Creates a catalog of the AppDomain's loaded assemblies plus the transitive references of the root assemblies.
        /// </summary>
        public static TypeCatalog FromRootAssemblies(params Assembly[] roots)
        {
            var assemblies = new Dictionary<string, Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                assemblies.TryAdd(assembly.FullName, assembly);

            var pending = new Queue<Assembly>(roots);
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

            return new TypeCatalog(assemblies.Values);
        }

        public IReadOnlyList<Type> FindEnumsByName(string typeName, string ns = null)
        {
            return Types
                .Where(x => x.IsEnum && x.Name == typeName && (ns == null || x.Namespace == ns))
                .ToList();
        }

        public IReadOnlyList<Type> FindEnumsByNamespace(string ns)
        {
            return Types.Where(x => x.IsEnum && x.Namespace == ns).ToList();
        }

        public Type FindClass(string ns, string typeName)
        {
            return Types.FirstOrDefault(x => x.IsClass && x.Namespace == ns && x.Name == typeName);
        }

        private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
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
