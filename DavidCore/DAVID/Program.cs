
namespace DAVID
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DavidServerInicialization();

            WebApplicationOptions options = new()
            {
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                Args = args,
                //mWebRootPath = "wwwroot" //možná bude potřeba pro Synology
            };
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }

        private static void DavidServerInicialization()
        {
            DAVID.Diagnostics.ITraceProvider.CreateProvider(typeof(Diagnostics.CsvTraceProvider));
            Diagnostics.ITraceProvider.Provider.Write(Diagnostics.TraceLevel.Info, "PROGRAM", "INICIALIZE", null);
        }
    }
}