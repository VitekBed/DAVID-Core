using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using DAVID.CodeAnnotations;
using DAVID.Interface;
using kDg.FileBaseContext.Extensions;
using Microsoft.EntityFrameworkCore;


namespace DAVID.EF.FileBasedDatabase;

public class FileBasedDatabase : IDatabase
{
    [FactoryMethod(typeof(IDatabase))]
    public static FileBasedDatabase Factory() => new FileBasedDatabase();
    private FileBasedDatabase()
    { }

    public DbContextOptionsBuilder OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseFileBaseContextDatabase();
    }
}



