using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.Muse.Animate
{
    static class ReflectionUtils
    {
        public static IEnumerable<Type> GetTypesInAllLoadedAssemblies(Predicate<Type> predicate)
        {
            var foundTypes = new List<Type>(1000);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type foundType in GetTypesInAssembly(assembly, predicate))
                {
                    foundTypes.Add(foundType);
                }
            }

            return foundTypes;
        }

        static IEnumerable<Type> GetTypesInAssembly(Assembly assembly, Predicate<Type> predicate)
        {
            if (assembly == null)
                return null;

            Type[] types = new Type[0];
            try
            {
                types = assembly.GetTypes();
            }
            catch (Exception)
            {
                // Can't load the types in this assembly
            }
            types = (from t in types where t != null && predicate(t) select t).ToArray();
            return types;
        }
        
        public static IEnumerable<Type> GetAllTypesWithAttribute<TAttribute>(Predicate<Type> predicate)
            where TAttribute : Attribute =>
            GetTypesInAllLoadedAssemblies(type =>
                type.GetCustomAttribute<TAttribute>() != null && predicate(type));
    }
}
