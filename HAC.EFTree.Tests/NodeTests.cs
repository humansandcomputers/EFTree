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
        context.Nodes.Add(new MockNode() { Name = "A", SafeAdd = true });
        await context.SaveChangesAsync();
        Assert.NotNull(context.Nodes.FirstOrDefault());
    }

    [Fact]
    public async Task Add_NotSetSafeAdd_Throws()
    {
        var context = factory.CreateDbContext();
        context.Nodes.Add(new MockNode() { Name = "A" });
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    class MockNode : Node
    {
        public required string Name { get; set; }
    }

    class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
    {
        public DbSet<MockNode> Nodes { get; set; }
    }
}
