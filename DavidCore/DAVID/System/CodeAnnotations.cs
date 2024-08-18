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
        public static bool HasAtribute<TAttribute>(this Type type) where TAttribute : Attribute => TypeHasAtribute<TAttribute>(type);
        public static bool IsMarkedImmutable(Type type) => ImmutableAttribute.IsMarkedImmutable(type);
        public static MethodInfo? GetFactoryMethode<TTargetType>(this Type type)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var methode in methods)
            {
                IEnumerable<Attribute> attributes = methode.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is not FactoryMethodAttribute factoryMethodAttribute) continue;    //metoda není označena jako Factory
                    if (factoryMethodAttribute.TargetType.Equals(typeof(TTargetType))) return methode;    //metoda je označená jako fatory pro požadovaný typ

                }

            }
            return null;

        }
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
    /// Používá se při dynamickém načítání modulů pro získávání objektů konkrétního interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingletonAttribute : Attribute
    {
        [MethodImpl(MethodImplOptions.NoInlining)]  //bez tohoto dojde k inliningu a atribut se nevyhodnotí!
        public SingletonAttribute()
        {

        }

    }
    /// <summary>
    /// Označuje factory metodu pro vytváření objektů. Při použití s <see cref="SingletonAttribute"/> říká, pomocí které metody
    /// mají být generovány objekty daného typu. Využívá se při dynamickém načítání modulů pro informování jádra, kterou metodu
    /// využít pro tvorbu objektu. Očekává se PUBLIC STATIC property nebo metoda s 0 parametry.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class FactoryMethodAttribute : Attribute
    {
        private Type _type;
        /// <summary>
        /// Interface nebo typ, který jádro potřebuje vytvořit.
        /// </summary>
        public Type TargetType { get => _type; }
        /// <summary>
        /// Informuje jádro, že se jedná o factory metodu pro tvorbu instance implementujícího požadovaný typ '<paramref name="targetType"/>'.
        /// </summary>
        /// <param name="targetType">Zpravidla interface jádra, jehož instanci má jádro vytvořit.</param>
        [MethodImpl(MethodImplOptions.NoInlining)] //bez tohoto dojde k inliningu a atribut se nevyhodnotí!
        public FactoryMethodAttribute(Type targetType)
        {
            _type = targetType;
        }
    }


}