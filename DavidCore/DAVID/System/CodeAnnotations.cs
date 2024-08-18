using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DAVID.CodeAnnotations
{
    /// <summary>
    /// Označuje třídy a struktury jako neměnné v průběhu jejich existence.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class ImmutableAttribute : Attribute
    {
        public static bool IsMarkedImmutable(Type type) => AttributeHelper.TypeHasAtribute<ImmutableAttribute>(type);
        public static void VerifyTypesAreImmutable(params Assembly[] assemblies)
        {
            var whitelist = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetLoadableTypes();
                foreach (var type in types)
                {
                    if (IsMarkedImmutable(type))
                    {
                        VerifyTypeIsImmutable(type, whitelist);
                        whitelist.Add(type);
                    }
                }
            }
        }

        private static void VerifyTypeIsImmutable(Type type, List<Type> whitelist)
        {
            throw new NotImplementedException();
        }
    }
    internal static class AttributeHelper
    {
        public static bool TypeHasAtribute<TAttribute>(Type type) where TAttribute : Attribute => type == null ? throw new ArgumentNullException(nameof(type)) : Attribute.IsDefined(type, typeof(TAttribute));
        public static bool IsMarkedImmutable(Type type) => ImmutableAttribute.IsMarkedImmutable(type);

    }
    internal static class AssemblyHelper
    {
        internal static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(q => q != null)!;
            }
        }
    }
    
    /// <summary>
    /// Informuje, že třída je singleton. Očekává se, že bude mít metodu označenou <see cref="FactoryMethodAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingletonAttribute : Attribute
    {

    }
    /// <summary>
    /// Označuje factory metodu pro vytváření objektů. Při použití s <see cref="SingletonAttribute"/> říká, pomocí které metody
    /// mají být generovány objekty daného typu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FactoryMethodAttribute : Attribute
    {
        private Type _type;
        public FactoryMethodAttribute(Type type)
        {
            _type = type;
        }
    }


}