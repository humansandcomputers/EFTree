namespace HAC.EFTree.Tests;

public class MovingTests
{
    readonly DbContextFactory<MockDbContext> factory = new();

    /// <summary>
    ///     => \- A
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChildToRoot_Empty_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }
        
        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            TreeAssert.Node(A);
        }
    }
}
