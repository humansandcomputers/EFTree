using HAC.EFTree.Tests.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HAC.EFTree.Tests;

public class DbContextTests
{
    [Fact]
    public async Task MyTestMethod()
    {
        var factory = new DbContextFactory<MyDbContext>();
        var context = factory.CreateDbContext();
        context.Set<Node>().Add(new Node() { Left = 1, Right = 2});
        await context.SaveChangesAsync();
        var u = context.Set<Node>().First();
    }
}

public class Node
{
    public Guid Id { get; set; }
    [Required, AllowNull] public long Left { get; internal set; }
    [Required, AllowNull] public long Right { get; internal set; }
}


class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Node> Nodes { get; set; }
}