using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace DAVID.Diagnostics
{
    public interface ITraceProvider
    {
        /// <summary>
        /// Zapíše jeden řádek do trace
        /// </summary>
        /// <param name="level">Uroveň zápisu</param>
        /// <param name="type">Typ, ze kterého pochází zápis do trace</param>
        /// <param name="methode">Metoda, ze kzeré pochází zápis do trace</param>
        /// <param name="keyword">Klíčové slovo pro vyhledávání</param>
        /// <param name="userInfo">Další informace, které vývojář chce zapsat do trace</param>
        [StackTraceHidden]
        void Write(TraceLevel level, string type, string methode, string? keyword, params string[] userInfo);
        [StackTraceHidden]
        IDisposable WriteScope(TraceLevel level, string type, string methode, string? keyword, params string[] userInfo);
        /// <summary>
        /// Vrací <see cref="ITraceProvider"/> nastavený při startu aplikace.
        /// </summary>
        static ITraceProvider Provider => _traceProvider is null ? throw new ApplicationException("Trace provider is not inicialized yet!") : _traceProvider;
        /// <summary>
        /// Vytvoří provider pro zápis trace na základě dodaného typu. Pokud provider již existuje, neudělá nic. 
        /// Zavolá konstruktor typu v parametru <paramref name="type"/> a pro běh instance se naplní neměnná hodnota <see cref="ITraceProvider.Provider"/>
        /// </summary>
        /// <param name="type">Typ, který se má použít jako TraceProvider. Musí být potomek <see cref="ITraceProvider"/>.</param>
        internal static void CreateProvider(Type type)
        {
            if (_traceProvider != null) return;
            object? instance = null;
            try
            {
                instance = System.Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Trace provider '{type}' cannot be inicialized!", e);
            }
            if (instance is ITraceProvider provider) _traceProvider = provider;
            else throw new ApplicationException($"Trace provider '{type}' is not ITraceProvider!");
        }
        private static ITraceProvider? _traceProvider;
    }
    public enum TraceLevel
    {
        Debug, Info, Warning, Error, Exception, Panic,
    }
    internal sealed class CsvTraceProvider : ITraceProvider
    {
        const string TRACE_DIRECTORY = "Trace";
        #region ITraceProvider
        [StackTraceHidden]
        public void Write(TraceLevel level, string type, string methode, string? keyword, params string[] userInfo)
        {
            try
            {
                if (traceDisabled) return;
                if (_OpenFileStream() is null) return;
                _WriteTraceRow(level, type, methode, keyword, userInfo);
                fileStream.Flush();
            }
            catch (Exception e)
            {
                //chyba při zápis do trace nesmí shodit aplikaci
            }
        }
        [StackTraceHidden]
        public IDisposable WriteScope(TraceLevel level, string type, string methode, string? keyword, params string[] userInfo)
        {
            throw new NotImplementedException();
        }
        #endregion
        System.IO.FileStream? fileStream;
        private System.IO.FileStream? _OpenFileStream()
        {
            if (fileStream is not null)
            {
                return fileStream;
            }
            else
            {
                if (FreeSpace < Current.Constants.MinFreeSpace)
                {
                    traceDisabled = true;
                    return null;
                }
                using (var slimLock = new Synchronization.SlimLock("TraceWriter").Lock())
                {
                    if (fileStream is not null)
                    { return fileStream; }
                    else
                    {
                        return _OpenNewFileStream();
                    }
                }
            }
        }
        private System.IO.FileStream _OpenNewFileStream()
        {
            var traceDir = System.IO.Directory.CreateDirectory(TRACE_DIRECTORY);
            DateTime now = DateTime.Now;
            string traceFileName = string.Concat(now.Ticks, ".csv");
            string tracePath = System.IO.Path.Combine(traceDir.FullName, traceFileName);
            fileStream = new FileStream(path: tracePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            return fileStream;
        }
        private void _WriteTraceRow(TraceLevel level, string type, string methode, string? keyword, string[] userInfo)
        {
            string row = String.Join(";",DateTime.Now, level, type, methode, keyword, userInfo);
            var encoded = System.Text.Encoding.UTF8.GetBytes(row);
            fileStream?.Write(encoded);
        }
        /// <summary> Volné místo na disku v MB </summary>
        private long FreeSpace => new DriveInfo(AppDomain.CurrentDomain.BaseDirectory).TotalFreeSpace / 1024 / 1024;
        /// <summary>
        /// Určuje zda nedošlo k vypnutí zápisu do trace. 
        /// K tomu může dojít např. z důvodu nedostatku místa na disku.
        /// </summary>
        private bool traceDisabled = false;
    }
    /// <summary>
    /// Třída umožňující řešit scope. Při zavolání konstruktoru provede zadanou metodu. Při zavolání Dispose provede druhou zadanou metodu.
    /// </summary>
    internal class BeginEndScope : IDisposable
    {
        Action _endAction;
        public BeginEndScope(Action startAction, Action endAction)
        {
            _endAction = endAction;
            startAction.Invoke();
        }

        public virtual void Dispose()
        {
            _endAction.Invoke();
        }
    }
}

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
}
