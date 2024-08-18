using System;
using System.Reflection;
using DAVID.CodeAnnotations;
using DAVID.Interface;
using DAVID.Synchronization;

namespace DAVID
{
    /// <summary>
    /// Instance serveru. Zajišťuje registraci a vytváření všech obslužných 
    /// </summary>
    [Singleton]
    public class ServerInstance : IDisposable
    {
        private static ServerInstance? _serverInstance;
        private static readonly SlimLock slimLock = new SlimLock("ServerInstanceFactory");
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
        public IWebSocketServer WebSocketServer;
        private ServerInstance()
        {
            _RegisterAssemblyResolvers();
            WebSocketServer = _GetSingletonInstanceFromModule<IWebSocketServer>(Current.Constants.WebSocketServerAssembly);

        }
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
        private ResolveEventHandler? _DllLoadHandler;
        private void _RegisterAssemblyResolvers()
        {
            using (var trace = Diagnostics.ITraceProvider.Provider.WriteScope(Diagnostics.TraceLevel.Info, nameof(ServerInstance), nameof(_RegisterAssemblyResolvers), null))
            {
                _DllLoadHandler = new ResolveEventHandler(_DllLoad);
                AppDomain.CurrentDomain.AssemblyResolve += _DllLoadHandler;
                trace.AddEndInfo(nameof(_DllLoad));
            }
        }

        private Assembly? _DllLoad(object? sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("System."))
            {
                return null; // potřebuji nechat CLR ať si problém vyřeší sám, jinak spadne u System.IO.Path ...
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
