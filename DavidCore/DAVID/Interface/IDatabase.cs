using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using DAVID.CodeAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DAVID.Interface;

public interface IDatabase
{
    public DbContextOptionsBuilder OnConfiguring(DbContextOptionsBuilder optionsBuilder);
}

public abstract class DvdDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder = ServerInstance.CurrentInstance.DatabaseProvider.OnConfiguring(optionsBuilder);
        base.OnConfiguring(optionsBuilder);
    }
    public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry AddByFullName(string assemblyName, string dbContextIdentificator, IDictionary<string, object> keyValues)
    {
        using (Diagnostics.ITraceProvider.WriteScopeInfo(Diagnostics.TraceLevel.Info, nameof(DvdDbContext), nameof(AddByFullName), null, assemblyName, dbContextIdentificator, keyValues?.Count.ToString() ?? "0"))
        {
            //load požadované assembly
            Assembly assembly = Assembly.Load(assemblyName);
            //naleznu všechny singletony implemetující typ a v nich factory metody pro tvorbu požadovaného typu
            Type? type = assembly.GetLoadableTypes().FirstOrDefault(x => x.HasDbContextIdentificator(dbContextIdentificator));
            if (type is null) throw new Exception($"Identificator '{dbContextIdentificator}' was not found in '{assemblyName}'.");

            object q = Activator.CreateInstance(type) ?? throw new Exception($"Cannot create instance of '{type}.'");
            return this.Add(q);
        }
    }
}

public abstract class DbItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Identity { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
}