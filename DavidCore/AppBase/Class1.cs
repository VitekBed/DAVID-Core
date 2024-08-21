using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAVID.CodeAnnotations;
using DAVID.Interface;
using Microsoft.EntityFrameworkCore;

namespace DAVID.App;
[DbContextIdentificator("DAVID.App.Base")]
internal sealed class Base : DvdDbContext
{
    [FactoryMethod(typeof(DvdDbContext))]
    public static Base Factory() => new Base();
    public DbSet<Person> People { get; set; }
}
[DbContextIdentificator("DAVID.App.Base.Person")]
internal sealed class Person : DbItem
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public DateOnly? Birthdate { get; set; }
}