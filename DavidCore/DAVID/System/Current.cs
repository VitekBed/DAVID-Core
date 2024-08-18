
namespace DAVID
{
    /// <summary>
    /// Přístup k základním vlastnostem aktuální instance.
    /// </summary>
    public static class Current{
        public static class Constants {
            public const long MinFreeSpace = 25_000; /*25 GB*/
            public static string DllLocation => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"../../CompleteProject/net8.0");
            public const string WebSocketServerAssembly = "DAVIDSocketServer";
        }
        public static ServerInstance ServerInstance = DAVID.ServerInstance.CurrentInstance;
    }
}