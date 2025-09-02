namespace HAC.EFTree.Tests;

public class MovingTests
{
    readonly DbContextFactory<MockDbContext> factory = new();

    /// <summary>
    /// |- A     => |- A
    ///    \- A1    \- A1
    /// </summary>
    [Fact]
    public async Task Move_FromNodeToRoot_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            var A1 = new MockNode() { Name = "A1" };
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(A1, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            context.Nodes.Move(A1);
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            TreeAssert.Node(A);
            TreeAssert.Node(A1);
            TreeAssert.Siblings(A, A1);
        }
    }

    /// <summary>
    /// |- A        => |- A        
    /// |- B           |- B
    /// |  \- X        |- C
    /// |     \- X1    |- D
    /// |- C           |  \- X
    /// |- D           |     \- X1
    /// |- E           |- E
    /// </summary>
    [Fact]
    public async Task Move_FromNodeToRoot_CorrectPositions2()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            var B = new MockNode() { Name = "B" };
            var C = new MockNode() { Name = "C" };
            var D = new MockNode() { Name = "D" };
            var E = new MockNode() { Name = "E" };
            var X = new MockNode() { Name = "X" };
            var X1 = new MockNode() { Name = "X1" };
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(X, B);
            context.Nodes.AddChild(X1, X);
            context.Nodes.AddChild(C);
            context.Nodes.AddChild(D);
            context.Nodes.AddChild(E);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var X = Assert.Single(context.Nodes, x => x.Name == "X");
            var D = Assert.Single(context.Nodes, x => x.Name == "D");
            context.Nodes.Move(X, D);
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            TreeAssert.Node(A);
            TreeAssert.Node(A1);
            TreeAssert.Siblings(A, A1);
        }
    }
}
