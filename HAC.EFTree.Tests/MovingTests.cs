using Xunit.Abstractions;

namespace HAC.EFTree.Tests;

public class MovingTests(ITestOutputHelper output) : IDisposable
{
    readonly MockDbContextFactory factory = new(output);

    public void Dispose() => factory.Dispose();

    /// <summary>
    /// |- A     => |- A
    ///    \- X     \- X
    /// </summary>
    [Fact]
    public async Task Move_FromParentToRoot_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var X);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(X, A);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var X);
            context.Nodes.Move(X);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Node(A);
            TreeAssert.Node(X);
            TreeAssert.Siblings(A, X);
        }
    }

    /// <summary>
    /// |- A  => |- A     
    /// \- X        \- X 
    /// </summary>
    [Fact]
    public async Task Move_FromRootAfterToParent_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var X);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(X);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var X);
            context.Nodes.Move(X, A);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Node(A);
            TreeAssert.Node(X);
            TreeAssert.Child(X, A);
        }
    }

    /// <summary>
    /// |- X  => |- A     
    /// \- A        \- X 
    /// </summary>
    [Fact]
    public async Task Move_FromRootBehindToParent_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var X);
            MockNode.Create(out var A);
            context.Nodes.AddChild(X);
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var X);
            context.Nodes.Move(X, A);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Node(A);
            TreeAssert.Node(X);
            TreeAssert.Child(X, A);
        }
    }

    /// <summary>
    /// |- A       	|- A       	
    /// |- B       	|- B       	
    /// |  |- B1   	|  |- B1   	
    /// |  |- B2   	|  \- B2   	
    /// |  \- X    	|- C       
    /// |     \- X1	|- D       
    /// |- C       	|  |- D1   
    /// |- D       	|  \- X    
    /// |  \- D1   	|     \- X1
    /// \- E       	\- E
    /// </summary>
    [Fact]
    public async Task Move_LeftToRightComplex_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var B1);
            MockNode.Create(out var B2);
            MockNode.Create(out var X);
            MockNode.Create(out var X1);
            MockNode.Create(out var C);
            MockNode.Create(out var D);
            MockNode.Create(out var D1);
            MockNode.Create(out var E);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(B1, B);
            context.Nodes.AddChild(B2, B);
            context.Nodes.AddChild(X, B);
            context.Nodes.AddChild(X1, X);
            context.Nodes.AddChild(C);
            context.Nodes.AddChild(D);
            context.Nodes.AddChild(D1, D);
            context.Nodes.AddChild(E);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Single(context.Nodes, out var D);
            context.Nodes.Move(X, D);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var B1);
            TreeAssert.Single(context.Nodes, out var B2);
            TreeAssert.Single(context.Nodes, out var C);
            TreeAssert.Single(context.Nodes, out var D);
            TreeAssert.Single(context.Nodes, out var D1);
            TreeAssert.Single(context.Nodes, out var E);
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Single(context.Nodes, out var X1);
            TreeAssert.Siblings(A, B, C, D, E);
            TreeAssert.Siblings(B1, B2);
            TreeAssert.Lineage(D, X, X1);
            TreeAssert.Siblings(D1, X);
        }
    }

    /// <summary>
    /// |- A       	|- A       
    /// |- B       	|- B       
    /// |  \- B1   	|  |- B1   
    /// |- C       	|  \- X    
    /// |- D       	|     \- X1
    /// |  |- D1   	|- C       
    /// |  |- D2   	|- D       
    /// |  \- X    	|  |- D1   
    /// |     \- X1	|  \- D2   
    /// \- E       	\- E
    /// </summary>
    [Fact]
    public async Task Move_RightToLeftComplex_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var B1);
            MockNode.Create(out var C);
            MockNode.Create(out var D);
            MockNode.Create(out var D1);
            MockNode.Create(out var D2);
            MockNode.Create(out var E);
            MockNode.Create(out var X);
            MockNode.Create(out var X1);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(B1, B);
            context.Nodes.AddChild(C);
            context.Nodes.AddChild(D);
            context.Nodes.AddChild(D1, D);
            context.Nodes.AddChild(D2, D);
            context.Nodes.AddChild(X, D);
            context.Nodes.AddChild(X1, X);
            context.Nodes.AddChild(E);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Single(context.Nodes, out var B);
            context.Nodes.Move(X, B);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var B1);
            TreeAssert.Single(context.Nodes, out var C);
            TreeAssert.Single(context.Nodes, out var D);
            TreeAssert.Single(context.Nodes, out var D1);
            TreeAssert.Single(context.Nodes, out var D2);
            TreeAssert.Single(context.Nodes, out var E);
            TreeAssert.Single(context.Nodes, out var X);
            TreeAssert.Single(context.Nodes, out var X1);
            TreeAssert.Siblings(A, B, C, D, E);
            TreeAssert.Siblings(B1, X);
            TreeAssert.Lineage(B, X, X1);
            TreeAssert.Siblings(B1, X);
        }
    }
}
