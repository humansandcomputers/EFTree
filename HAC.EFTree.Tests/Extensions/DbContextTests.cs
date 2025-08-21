using HAC.EFTree.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAC.EFTree.Tests.Extensions;

[TestClass]
public class DbContextTests
{
    [TestMethod]
    public async Task MyTestMethod()
    {
        var factory = new DbContextFactory<MyDbContext>();
        var context = factory.CreateDbContext();
        context.Set<User>().Add(new User() { Name = "Nima" });
        await context.SaveChangesAsync();
        var u = context.Set<User>().First();

    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}


class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}