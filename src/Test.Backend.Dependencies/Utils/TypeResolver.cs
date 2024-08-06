using System.Collections.Concurrent;
using System.Reflection;

namespace Test.Backend.Dependencies.Utils
{
    public static class TypeResolver
    {
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new();

        /// <summary>
        /// Retrieves the type of the event class to deserialize the kakfa message and handle the associated handler
        /// </summary>
        /// <param name="typeName">namespace to search</param>
        /// <returns>Type found or null</returns>
        public static Type? GetEventType(string typeName)
        {
            if (TypeCache.TryGetValue(typeName, out var cachedType))
            {
                return cachedType;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = assembly.GetType(typeName);

                if (type != null)
                {
                    TypeCache[typeName] = type;

                    return type;
                }
            }

            return null;
        }
    }
}
