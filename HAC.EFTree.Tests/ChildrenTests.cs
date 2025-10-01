using Xunit.Abstractions;

namespace HAC.EFTree.Tests;

public sealed class ChildrenTests(ITestOutputHelper output) : IDisposable
{
    readonly MockDbContextFactory factory = new(output);

    public void Dispose() => factory.Dispose();

    /// <summary>
    /// |- A   
    ///    \- B
    /// </summary>
    [Fact]
    public async Task Children_ParentWithANode_ValidChildren()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B, A);
            await context.SaveChangesAsync();
        }
        // Act & Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            var children = context.Nodes.Children(A).ToArray();
            TreeAssert.Single(children, out var B);
        }
    }

    /// <summary>
    /// |- A   
    ///    |- B
    ///    \- C
    /// </summary>
    [Fact]
    public async Task Children_ParentWithMultipleNodes_ValidChildren()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var C);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B, A);
            context.Nodes.AddChild(C, A);
            await context.SaveChangesAsync();
        }
        // Act & Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            var children = context.Nodes.Children(A).ToArray();
            Assert.Equal(2, children.Length);
            TreeAssert.Single(children, out var B);
            TreeAssert.Single(children, out var C);
        }
    }

    /// <summary>
    /// |- A   
    ///    |- B
    ///       |- B1
    ///       \- B2
    ///    \- C
    ///       |- C1
    ///       \- C2
    /// </summary>
    [Fact]
    public async Task Children_ParentWithMultipleNodesWithChildren_ValidChildren()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var B1);
            MockNode.Create(out var B2);
            MockNode.Create(out var C);
            MockNode.Create(out var C1);
            MockNode.Create(out var C2);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B, A);
            context.Nodes.AddChild(B1, B);
            context.Nodes.AddChild(B2, B);
            context.Nodes.AddChild(C, A);
            context.Nodes.AddChild(C1, C);
            context.Nodes.AddChild(C2, C);
            await context.SaveChangesAsync();
        }
        // Act & Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            var children = context.Nodes.Children(A).ToArray();
            Assert.Equal(2, children.Length);
            TreeAssert.Single(children, out var B);
            TreeAssert.Single(children, out var C);
        }
    }

    /// <summary>
    /// |- A   
    ///    \- B
    /// </summary>
    [Fact]
    public async Task AllChildren_ParentWithANode_ValidChildren()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B, A);
            await context.SaveChangesAsync();
        }
        // Act & Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            var children = context.Nodes.AllChildren(A).ToArray();
            TreeAssert.Single(children, out var B);
        }
    }

    /// <summary>
    /// |- A   
    ///    |- B
    ///    \- C
    /// </summary>
    [Fact]
    public async Task AllChildren_ParentWithMultipleNodes_ValidChildren()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var C);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B, A);
            context.Nodes.AddChild(C, A);
            await context.SaveChangesAsync();
        }
        // Act & Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            var children = context.Nodes.AllChildren(A).ToArray();
            Assert.Equal(2, children.Length);
            TreeAssert.Single(children, out var B);
            TreeAssert.Single(children, out var C);
        }
    }

    /// <summary>
    /// |- A   
    ///    |- B
    ///       |- B1
    ///       \- B2
    ///    \- C
    ///       |- C1
    ///       \- C2
    /// </summary>
    [Fact]
    public async Task AllChildren_ParentWithMultipleNodesWithChildren_ValidChildren()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var B1);
            MockNode.Create(out var B2);
            MockNode.Create(out var C);
            MockNode.Create(out var C1);
            MockNode.Create(out var C2);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B, A);
            context.Nodes.AddChild(B1, B);
            context.Nodes.AddChild(B2, B);
            context.Nodes.AddChild(C, A);
            context.Nodes.AddChild(C1, C);
            context.Nodes.AddChild(C2, C);
            await context.SaveChangesAsync();
        }
        // Act & Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            var children = context.Nodes.AllChildren(A).ToArray();
            Assert.Equal(6, children.Length);
            TreeAssert.Single(children, out var B);
            TreeAssert.Single(children, out var B1);
            TreeAssert.Single(children, out var B2);
            TreeAssert.Single(children, out var C);
            TreeAssert.Single(children, out var C1);
            TreeAssert.Single(children, out var C2);
        }
    }
}
