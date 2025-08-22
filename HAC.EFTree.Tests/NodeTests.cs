using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace HAC.EFTree.Tests;

public class NodeTests
{
    readonly DbContextFactory<MockDbContext> factory = new();

    [Fact]
    public async Task Add_SetSafeAdd_Adds()
    {
        var context = factory.CreateDbContext();
        context.Nodes.Add(new Node() { SafeAdd = true });
        await context.SaveChangesAsync();
        Assert.NotNull(context.Nodes.FirstOrDefault());
    }

    [Fact]
    public async Task Add_NotSetSafeAdd_Throws()
    {
        var context = factory.CreateDbContext();
        context.Nodes.Add(new Node());
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
    {
        public DbSet<Node> Nodes { get; set; }
    }
}
