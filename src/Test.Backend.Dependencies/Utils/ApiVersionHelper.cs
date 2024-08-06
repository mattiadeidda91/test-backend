using System.Reflection;

namespace Test.Backend.Dependencies.Utils
{
    public static class ApiVersionHelper
    {
        /// <summary>
        /// Get Api Versions
        /// </summary>
        /// <param name="assembly">The assembly from which to extract the controller namespaces.</param>
        /// <returns>A list of ordered api versions.</returns>
        public static List<string> GetApiVersions(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return assembly.GetTypes()
                .Where(t => t.IsClass && t.Namespace != null && t.Namespace.StartsWith(Assembly.GetEntryAssembly()?.GetName().Name + ".Controllers.v"))
                .Select(t => t.Namespace!.Split('.').Last())
                .Distinct()
                .Select(version => new
                {
                    Version = version,
                    VersionNumber = int.TryParse(version.TrimStart('v'), out var number) ? number : 0
                })
                .OrderBy(v => v.VersionNumber)
                .Select(v => v.Version)
                .ToList();
        }
    }
}
