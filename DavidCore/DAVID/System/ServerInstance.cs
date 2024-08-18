using System;
using System.Reflection;
using DAVID.CodeAnnotations;
using DAVID.Interface;
using DAVID.Synchronization;

namespace DAVID
{
    /// <summary>
    /// Instance serveru. Zajišťuje registraci a vytváření všech obslužných objektů pro běh serveru
    /// </summary>
    [Singleton]
    public class ServerInstance : IDisposable
    {
        #region static
        private static ServerInstance? _serverInstance;
        private static readonly SlimLock slimLock = new SlimLock("ServerInstanceFactory");
        /// <summary>
        /// Přístup k singleton objektu typu <see cref="ServerInstance"/> 
        /// </summary>
        [FactoryMethod(typeof(ServerInstance))]
        public static ServerInstance CurrentInstance
        {
            get
            {
                if (_serverInstance is not null) return _serverInstance;
                using (slimLock.Lock())
                {
                    if (_serverInstance is not null) return _serverInstance;    //dvojitá kontrola na null
                    _serverInstance = new ServerInstance();
                    return _serverInstance;
                }
            }
        }
        #endregion static
        public IWebSocketServer WebSocketServer {get; init;}
        private ServerInstance()
        {
            _RegisterAssemblyResolvers();
            WebSocketServer = _GetSingletonInstanceFromModule<IWebSocketServer>(Current.Constants.WebSocketServerAssembly);

        }
        /// <summary>
        /// Metoda pro získání instance singletonu požadovaného typu <typeparamref name="T"/> z požadované assembly. 
        /// Očekává, že požadovaný typ bude v načítané assembly označen jako <see cref="SingletonAttribute"/> 
        /// a bude obsahovat metodu označenou <see cref="FactoryMethodAttribute"/> pro zadaný typ <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Požadovaný typ</typeparam>
        /// <param name="assemblyName">Jméno assembly ve kterém je potřeba typ a metodu nalézt.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        private static T _GetSingletonInstanceFromModule<T>(string assemblyName)
        {
            using (Diagnostics.ITraceProvider.Provider.WriteScope(Diagnostics.TraceLevel.Info, nameof(ServerInstance), nameof(_GetSingletonInstanceFromModule), typeof(T).Name, assemblyName))
            {
                //load požadované assembly
                Assembly assembly = Assembly.Load(assemblyName);
                //naleznu všechny singletony implemetující typ a v nich factory metody pro tvorbu požadovaného typu
                IEnumerable<Type> singletons = assembly.GetLoadableTypes().Where(x => x.HasAtribute<SingletonAttribute>());
                foreach (var singletonType in singletons)
                {
                    Type[] interfaces = singletonType.GetInterfaces();
                    if (!interfaces.Contains(typeof(T))) continue;
                    // Implementuje požadovaný typ / interface
                    var factoryMethode = singletonType.GetFactoryMethode<T>();
                    if (factoryMethode is not null && factoryMethode.Invoke(null, null) is T requestedInstance)
                    {
                        return requestedInstance;
                    }
                }
                throw new ApplicationException($"Cannot create instance of '{nameof(T)}'!");
            }
        }
        #region AssemblyResolver
        /// <summary>
        /// Hendler pro assembly resolver. Umožňuje případnou odregistraci handleru za běhu aplikace
        /// </summary>
        private ResolveEventHandler? _DllLoadHandler;
        /// <summary> Zaregistrování assembly resolverů. </summary>
        private void _RegisterAssemblyResolvers()
        {
            using (var trace = Diagnostics.ITraceProvider.Provider.WriteScope(Diagnostics.TraceLevel.Info, nameof(ServerInstance), nameof(_RegisterAssemblyResolvers), null))
            {
                _DllLoadHandler = new ResolveEventHandler(_DllLoad);
                AppDomain.CurrentDomain.AssemblyResolve += _DllLoadHandler;
                trace.AddEndInfo(nameof(_DllLoad));
            }
        }
        /// <summary>
        /// Metoda pro assembly resolver. Načítá assembly z příslušné knihovny v umístění podle konfigurace (<seealso cref="Current.Constants.DllLocation"/>)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly? _DllLoad(object? sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("System."))
            {
                return null; // potřebuji nechat CLR ať si problém vyřeší sám, jinak spadne u System.IO.Path o pár řádků níž ...
            }

            // Extrahujeme název požadovaného sestavení
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";
            var assemblyPath = Path.Combine(Current.Constants.DllLocation, assemblyName);

            if (File.Exists(assemblyPath))
            {
                // Načteme a vrátíme sestavení
                return Assembly.LoadFrom(assemblyPath);
            }

            // Pokud sestavení nebylo nalezeno, vrátíme null a pokračuje se podle pravidel CLR
            return null;
        }
        /// <summary> Odregistrování assembly resolverů. </summary>
        private void _UnregisterAssemblyResolvers()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= _DllLoadHandler;
        }
        #endregion
        public void Dispose()
        {
            _UnregisterAssemblyResolvers();
            _serverInstance = null;
        }

    }
}
