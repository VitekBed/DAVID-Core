using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace DAVID.CodeAnnotations
{
    /// <summary>
    /// Označuje třídy a struktury jako neměnné v průběhu jejich existence. Garantuje autor označené třídy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class ImmutableAttribute : Attribute
    {
        /// <summary>
        /// Vyhodnocuje, zda typ na sobě má <see cref="ImmutableAttribute"/> a má tedy garantovanou svou neměnnost. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsMarkedImmutable(Type type) => AttributeHelper.TypeHasAtribute<ImmutableAttribute>(type);
        [Obsolete("Not yet implemeted", true)]
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
    /// <summary>
    /// Třída pomocných statických metod pro práci s atributy.
    /// </summary>
    internal static class AttributeHelper
    {
        /// <summary>
        /// Vrací, zda typ <paramref name="type"/> má na sobě atribut <typeparamref name="TAttribute"/>
        /// </summary>
        /// <typeparam name="TAttribute">Požadovaný atribut hledaný na třídě/struktuře</typeparam>
        /// <param name="type">Zkoumaný typ</param>
        /// <returns><see langword="true"/>, pokud na třídě/struktuře je uvedený <typeparamref name="TAttribute"/> </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TypeHasAtribute<TAttribute>(Type type) where TAttribute : Attribute => type == null ? throw new ArgumentNullException(nameof(type)) : Attribute.IsDefined(type, typeof(TAttribute));
        /// <summary>
        /// Vrací, zda typ má na sobě atribut <typeparamref name="TAttribute"/>
        /// </summary>
        /// <typeparam name="TAttribute">Požadovaný atribut hledaný na třídě/struktuře</typeparam>
        /// <param name="type">Zkoumaný typ</param>
        /// <returns><see langword="true"/>, pokud na třídě/struktuře je uvedený <typeparamref name="TAttribute"/></returns>
        public static bool HasAtribute<TAttribute>(this Type type) where TAttribute : Attribute => TypeHasAtribute<TAttribute>(type);
        /// <summary>
        /// Pro typ dohledává první PUBLIC STATIC metodu nebo property označenou atributem <see cref="FactoryMethodAttribute"/>,
        /// která je zároveň definovaná jako factory metoda pro typ <typeparamref name="TTargetType"/>.
        /// </summary>
        /// <typeparam name="TTargetType">Třída nebo Interface, pro jterý je metoda označena jako factory.</typeparam>
        /// <param name="type">Typ, ve kterém dochází k hledání factory metod </param>
        /// <returns>První nalezená factory metoda vytvářející instanci <typeparamref name="TTargetType"/></returns>
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
        /// <summary>
        /// Načte všechny typy z assembly.
        /// </summary>
        /// <param name="assembly">Zkoumaná assembly</param>
        /// <returns></returns>
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
        internal static bool HasDbContextIdentificator(this Type type, string identificator)
        {
            DbContextIdentificatorAttribute? atribut = type.GetCustomAttribute<DbContextIdentificatorAttribute>(false);
            return atribut is not null && atribut.FullName == identificator;
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
    /// Označuje factory metodu pro vytváření objektů. Při použití s <see cref="SingletonAttribute"/> na třídě říká, pomocí které metody
    /// mají být generovány objekty daného typu (<see cref="TargetType"/>). Využívá se při dynamickém načítání modulů pro informování jádra, kterou metodu
    /// využít pro tvorbu požadovaného objektu. Očekává se PUBLIC STATIC property nebo metoda s 0 parametry. Návratový typ metody může být
    /// libovolný potomek typu, pro který je metoda deklarovaná jako factory.
    /// </summary>
    /// <remarks> 
    /// Očekává se právě jedna factory metoda pro jeden požadovaný typ. Při označení více metod není zaručeno, která bude použita! 
    /// Jedna metoda může být factory metodou pro více typů.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class FactoryMethodAttribute : Attribute
    {
        private Type _type;
        /// <summary>
        /// Interface nebo typ pro který deklarujeme, že se jedná o factory metodu.
        /// </summary>
        public Type TargetType { get => _type; }
        /// <summary>
        /// Informuje jádro, že se jedná o factory metodu pro tvorbu instance implementujícího požadovaný typ '<paramref name="targetType"/>'.
        /// </summary>
        /// <param name="targetType">Interface nebo typ pro který deklarujeme, že se jedná o factory metodu.</param>
        [MethodImpl(MethodImplOptions.NoInlining)] //bez tohoto dojde k inliningu a atribut se nevyhodnotí!
        public FactoryMethodAttribute(Type targetType)
        {
            _type = targetType;
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DbContextIdentificatorAttribute : Attribute
    {
        public string FullName { get; init; }

        public DbContextIdentificatorAttribute(string fullName)
        {
            FullName = fullName;
        }
    }
}