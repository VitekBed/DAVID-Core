
using System.Runtime.CompilerServices;
using DAVID.Diagnostics;
using DAVID.Interface;

namespace DAVID
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DavidServerInicialization();

            ITraceScope trace = Diagnostics.ITraceProvider.Provider.WriteScope(Diagnostics.TraceLevel.Info, nameof(Program), nameof(Main));

            WebApplicationOptions options = new()
            {
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                Args = args,
                //mWebRootPath = "wwwroot" //možná bude potřeba pro Synology
            };
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");
            app.MapGet("/Exception", ThrowException);
            app.MapGet("/VBE", WritePerson);

            trace.Dispose();    //ruční zápis koncové trace značky
            app.Run();


        }

        private static void DavidServerInicialization()
        {
            DAVID.Diagnostics.ITraceProvider.CreateProvider(typeof(Diagnostics.CsvTraceProvider));
            Diagnostics.ITraceProvider.Provider.Write(Diagnostics.TraceLevel.Info, "PROGRAM", "INICIALIZE", null);
            var server = Current.ServerInstance; //inicializace ServerInstance

        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowException()
        {
            using (ITraceScope trace = Diagnostics.ITraceProvider.WriteScopeInfo(Diagnostics.TraceLevel.Info, nameof(Program), nameof(ThrowException)))
            {
                try
                {
                    Throw();
                }
                catch (Exception e)
                {
                    ITraceProvider.WriteException(nameof(Program), nameof(ThrowException), null, e);
                    throw;
                }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]

        private static void Throw() => throw new Exception("TESTexception");
        private static void WritePerson()
        {
            using DvdDbContext context = ServerInstance._GetInstanceFromModule<DvdDbContext>("AppBase");
            context.AddByFullName("AppBase","DAVID.App.Base.Person",null);
            
            //context.Add(new Person() { Name = "Vít", Surname = "Bednář", Birthdate = new DateOnly(1993, 10, 17) });
            context.SaveChanges();
        }
    }
}