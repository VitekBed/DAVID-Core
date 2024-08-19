using System.Diagnostics;

namespace DAVID.Diagnostics
{
    /// <summary> Interface provideru pro zápis trace </summary>
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
        void Write(TraceLevel level, string type, string methode, string? keyword = null, params string[] userInfo);
        /// <summary>
        /// Zapáše úvodní blok pro párový zápis do trace. Při zavolání <see cref="ITraceScope.Dispose()"/> zapíše
        /// koncovou značku. Obsah koncové značky lze ovlivnit zavoláním <see cref="ITraceScope.AddEndInfo(string[])"/>
        /// </summary>
        /// <param name="level"></param>
        /// <param name="type"></param>
        /// <param name="methode"></param>
        /// <param name="keyword"></param>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        ITraceScope WriteScope(TraceLevel level, string type, string methode, string? keyword = null, params string[] userInfo);
        /// <summary>
        /// Vrací <see cref="ITraceProvider"/> nastavený při startu aplikace.
        /// </summary>
        static ITraceProvider Provider => _traceProvider is null ? throw new Exception("Trace provider is not inicialized yet!") : _traceProvider;
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
                throw new Exception($"Trace provider '{type}' cannot be inicialized!", e);
            }
            if (instance is ITraceProvider provider) _traceProvider = provider;
            else throw new Exception($"Trace provider '{type}' is not ITraceProvider!");
        }
        private static ITraceProvider? _traceProvider;
    }
    /// <summary> Scope pro zápis trace, umožňuje zadat a načíst UserInfo, které bude zapsáno na konci scope. </summary>
    public interface ITraceScope : IDisposable
    {
        /// <summary>
        /// Přidá informace do UserInfo, bude zapsáno na konec scope
        /// </summary>
        /// <param name="userInfo"></param>
        void AddEndInfo(params string[] userInfo);
        /// <summary>
        /// Vrátí kolekci všech UserInfo, které budou zapsány
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetEndInfos();
    }
    /// <summary> Úroveň zápisu do trace, může být konfiguračně omezeno, jaká úroveň bude zapisována </summary>
    public enum TraceLevel
    {
        Debug, Info, Warning, Error, Exception, Panic,
    }
    /// <summary> Určuje zda se jedná o zápis začátku/konce bloku nebo jen stavový zápis </summary>
    internal enum TraceState
    {
        Begin, State, End,
    }
    /// <summary> Scope zápisu do trace. Při zavolání <see cref="Dispose()"/> zapíše koncový zápis. </summary>
    /// <remarks> Doporučuji používat v using patternu </remarks>
    internal sealed class TraceScope : BeginEndScope, ITraceScope
    {
        /// <summary>
        /// Scope neobsahující zahajovací ani ukončovací událost.
        /// </summary>
        public static TraceScope Empty => new TraceScope(null, null);
        /// <summary>
        /// Vytvoří scope, který při vytvoření a ukončení zavolá zadané akce
        /// </summary>
        /// <param name="startAction">Akce spuštěna při vytvoření <see cref="TraceScope"/></param>
        /// <param name="endAction">Akce spuštěna při dispose <see cref="TraceScope"/></param>
        public TraceScope(Action? startAction, Action? endAction) : base(startAction, endAction) { }
        /// <summary>
        /// Vytvoří scope, který při vytvoření zavolá zadanou akci. Ukončovací akci je možné nastavit pomocí <see cref="AddEndInfo(string[])"/>
        /// </summary>
        /// <param name="startAction">Akce spuštěna při vytvoření <see cref="TraceScope"/></param>
        public TraceScope(Action startAction) : base(startAction, null) { }
        private List<string> endInfos = new List<string>();
        void ITraceScope.AddEndInfo(params string[] userInfo)
        {
            endInfos.AddRange(userInfo);
        }
        /// <summary>
        /// Nastaví akci, která má být spuštěna po ukončení scope. Lze nastavit pouze jednou.
        /// </summary>
        /// <param name="action"></param>
        internal void SetEndAction(Action action)
        {
            if (this._endAction is not null) throw new Exception("EndAction was already set.");
            this._endAction = action;
        }

        IEnumerable<string> ITraceScope.GetEndInfos()
        {
            return new List<string>(endInfos);
        }
    }
    /// <summary> Provider pro zápis trace formou CSV souboru </summary>
    internal sealed class CsvTraceProvider : ITraceProvider
    {
        /// <summary> Adresář umístění trace </summary>
        const string TRACE_DIRECTORY = "Trace";
        #region ITraceProvider
        [StackTraceHidden]
        void ITraceProvider.Write(TraceLevel level, string type, string methode, string? keyword, params string[] userInfo)
        {
            try
            {
                if (traceDisabled) return;  //trace je nouzově deaktivovaný
                if (_OpenFileStream() is null) return;  //nepodařilo se otevřít filestram
                _WriteTraceRow(level, TraceState.State, type, methode, keyword, userInfo);
                fileStream!.Flush();
            }
            catch
            {
                //chyba při zápis do trace nesmí shodit aplikaci
            }
        }

        [StackTraceHidden]
        ITraceScope ITraceProvider.WriteScope(TraceLevel level, string type, string methode, string? keyword, params string[] userInfo)
        {
            if (traceDisabled) return TraceScope.Empty;
            try
            {
                if (_OpenFileStream() is null) return TraceScope.Empty;
                TraceScope traceScope = new TraceScope(
                    () => _WriteTraceRow(level, TraceState.Begin, type, methode, keyword, userInfo)
                );
                traceScope.SetEndAction(
                    () => _WriteTraceRow(level, TraceState.End, type, methode, keyword, ((ITraceScope)traceScope).GetEndInfos().ToArray())
                );
                return traceScope;
            }
            catch
            {
                //chyba při zápis do trace nesmí shodit aplikaci
            }
            return TraceScope.Empty;
        }
        #endregion
        System.IO.FileStream? fileStream;
        /// <summary> 
        /// Otevře existující FileStream nebo založí nový. 
        /// Může vrátit <see langword="null"/>, pokud se nepodaří otevřít FileStream. 
        /// </summary>
        private System.IO.FileStream? _OpenFileStream()
        {
            if (fileStream is not null)
            {
                return fileStream;
            }
            else
            {
                if (FreeSpace < Current.Constants.MinFreeSpace) //bezpečnostní pojistka na vyčerpání místa na disku
                {
                    traceDisabled = true;
                    return null;
                }
                using (var slimLock = new Synchronization.SlimLock("TraceWriter").Lock())
                {
                    if (fileStream is not null) //dvojitá kontrola na null
                        return fileStream;
                    else
                        return _OpenNewFileStream();
                }
            }
        }
        /// <summary>
        /// Vytvoří nový FileStream pro CSV trace a zapíše hlavičku do souboru.
        /// Naplní <see cref="fileStream"/> stejným objektem jako navrací.
        /// </summary>
        /// <returns>Vrací objekt uložený do <see cref="fileStream"/> </returns>
        private System.IO.FileStream _OpenNewFileStream()
        {
            var traceDir = System.IO.Directory.CreateDirectory(TRACE_DIRECTORY);
            DateTime now = DateTime.Now;
            string traceFileName = string.Concat(now.Ticks, ".csv");
            string tracePath = System.IO.Path.Combine(traceDir.FullName, traceFileName);
            fileStream = new FileStream(path: tracePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            string row = String.Join(";", "DateTime", "Level", "State", "Type", "Methode", "Keyword", Environment.NewLine);
            var encoded = System.Text.Encoding.UTF8.GetBytes(row);
            fileStream.Write(encoded);
            return fileStream;
        }
        /// <summary> Zápis jednoho řádku trace </summary>
        private void _WriteTraceRow(TraceLevel level, TraceState state, string type, string methode, string? keyword, params string[] userInfo)
        {
            string stateString = state switch
            {
                TraceState.Begin => "B",
                TraceState.End => "E",
                TraceState.State => "S",
                _ => "?",
            };
            string row = String.Join(";", DateTime.Now, level, stateString, type, methode, keyword, String.Join(";", userInfo), Environment.NewLine);
            var encoded = System.Text.Encoding.UTF8.GetBytes(row);
            fileStream?.Write(encoded);
            fileStream?.Flush();
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
    /// Třída umožňující řešit scope. Při zavolání konstruktoru provede zadanou metodu. Při zavolání <see cref="Dispose"/> provede druhou zadanou metodu.
    /// </summary>
    internal class BeginEndScope : IDisposable
    {
        /// <summary>
        /// <see cref="Action"/>, která má být vykonána při ukončení scope (v rámci <see cref="Dispose"/>)
        /// </summary>
        protected Action? _endAction;
        /// <summary>
        /// Vytovří scope a provede invoke <paramref name="startAction"/>.
        /// </summary>
        /// <param name="startAction"> <see cref="Action"/> provedená při vytvoření objektu </param>
        /// <param name="endAction"> <see cref="Action"/> provedená při zavolání <see cref="Dispose"/> </param>
        public BeginEndScope(Action? startAction, Action? endAction)
        {
            _endAction = endAction;
            startAction?.Invoke();
        }
        private bool _disposing = false;
        /// <summary>
        /// Zavolá akci, která se má provést na konci scope a standardně uvolní objekt
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposing) return; //zajištění aby se Invoke zavolal jen jednou
            _disposing = true;
            _endAction?.Invoke();
        }
    }
}
