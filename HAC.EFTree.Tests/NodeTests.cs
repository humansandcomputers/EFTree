using Xunit.Abstractions;

namespace HAC.EFTree.Tests;

public class NodeTests(ITestOutputHelper output)
{
    readonly MockDbContextFactory factory = new(output);

    [Fact]
    public async Task Add_SetSafeAdd_Adds()
    {
        var context = factory.CreateDbContext();
        MockNode A = new() { Name = "A" };
        A.Register();
        context.Nodes.Add(A);
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
}
