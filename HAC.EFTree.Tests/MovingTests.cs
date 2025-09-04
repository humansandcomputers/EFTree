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
            MockNode.Create(out var A);
            MockNode.Create(out var A1);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(A1, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A1);
            context.Nodes.Move(A1);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var A1);
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
    public async Task Move_FromNodeToNode_CorrectPositions2()
    {
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var C);
            MockNode.Create(out var D);
            MockNode.Create(out var E);
            MockNode.Create(out var X);
            MockNode.Create(out var X1);
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
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Single(context.Nodes, out var D);
            context.Nodes.Move(X, D);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var C);
            TreeAssert.Single(context.Nodes, out var D);
            TreeAssert.Single(context.Nodes, out var E);
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Single(context.Nodes, out var X1);
            TreeAssert.Siblings(A, B, C, D, E);
            TreeAssert.Lineage(D, X, X1);
        }
    }
}
