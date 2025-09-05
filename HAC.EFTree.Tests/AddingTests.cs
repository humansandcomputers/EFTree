namespace HAC.EFTree.Tests;

using HAC.EFTree.Tests.Helpers;
using System;
using System.Diagnostics;
using Xunit.Abstractions;

public class AddingTests(ITestOutputHelper output) : IDisposable
{
    readonly MockDbContextFactory factory = new(output);

    /// <summary>
    /// |- A     => |- A
    ///             \- A
    /// </summary>
    [Fact]
    public void AddAfter_PreviouslyAdded_Throws()
    {
        // Arrange
        using var context = factory.CreateDbContext();
        MockNode.Create(out var A);
        context.Nodes.AddChild(A);
        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.Nodes.AddAfter(A));
    }

    /// <summary>
    /// |- A     => |- A
    /// |  |- A1    |  |- A1
    /// |  \- A2    |  |- A1_2
    /// \- B        |  \- A2
    ///             \- B
    /// </summary>
    [Fact]
    public async Task AddAfter_AfterNodeWithOtherSiblings_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var A1);
            MockNode.Create(out var A2);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(A1, A);
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A1);
            MockNode.Create(out var A1_2);
            context.Nodes.AddAfter(A1_2, A1);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var A1);
            TreeAssert.Single(context.Nodes, out var A1_2);
            TreeAssert.Single(context.Nodes, out var A2);
            TreeAssert.Node(A1);
            TreeAssert.Node(A1_2);
            TreeAssert.Node(A2);
            TreeAssert.Child(A1, A);
            TreeAssert.Child(A1_2, A);
            TreeAssert.Child(A2, A);
            TreeAssert.Siblings(A, B);
            TreeAssert.Siblings(A1, A1_2, A2);
        }
    }

    /// <summary>
    /// |- A     => |- A
    ///             \- B
    /// </summary>
    [Fact]
    public async Task AddAfter_AfterRoot_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var B);
            context.Nodes.AddAfter(B);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Node(A);
            TreeAssert.Node(B);
            TreeAssert.Siblings(A, B);
        }
    }

    /// <summary>
    ///          => |- A
    ///             \- B
    /// </summary>
    [Fact]
    public void AddAfter_AfterNodeNotPreviouslyAdded_Throws()
    {
        // Arrange
        using var context = factory.CreateDbContext();
        MockNode.Create(out var A);
        MockNode.Create(out var B);
        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.Nodes.AddAfter(A, B));
    }

    /// <summary>
    /// |- A     => |- A
    ///             \- A
    /// </summary>
    [Fact]
    public void AddBefore_PreviouslyAdded_Throws()
    {
        // Arrange
        using var context = factory.CreateDbContext();
        MockNode.Create(out var A);
        context.Nodes.AddChild(A);
        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.Nodes.AddBefore(A));
    }

    /// <summary>
    /// |- A     => |- A
    /// |  |- A1    |  |- A1
    /// |  \- A2    |  |- A1_2
    /// \- B        |  \- A2
    ///             \- B
    /// </summary>
    [Fact]
    public async Task AddBefore_BeforeNodeWithOtherSiblings_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var A1);
            MockNode.Create(out var A2);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(A1, A);
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A2);
            MockNode.Create(out var A1_2);
            context.Nodes.AddBefore(A1_2, A2);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var A1);
            TreeAssert.Single(context.Nodes, out var A1_2);
            TreeAssert.Single(context.Nodes, out var A2);
            TreeAssert.Node(A1);
            TreeAssert.Node(A1_2);
            TreeAssert.Node(A2);
            TreeAssert.Child(A1, A);
            TreeAssert.Child(A1_2, A);
            TreeAssert.Child(A2, A);
            TreeAssert.Siblings(A, B);
            TreeAssert.Siblings(A1, A1_2, A2);
        }
    }

    /// <summary>
    /// \- B     => |- A
    ///    \- B1    \- B
    ///                \- B1
    /// </summary>
    [Fact]
    public async Task AddBefore_BeforeRoot_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var B);
            MockNode.Create(out var B1);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(B1, B);
            await context.SaveChangesAsync();
        }
        // Act
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            context.Nodes.AddBefore(A);
            await context.SaveChangesAsync();
        }
        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var B1);
            TreeAssert.Node(A);
            TreeAssert.Node(B);
            TreeAssert.Node(B1);
            TreeAssert.Siblings(A, B);
            TreeAssert.Child(B1, B);
        }
    }

    /// <summary>
    ///          => |- A
    ///             \- B
    /// </summary>
    [Fact]
    public void AddBefore_BeforeNodeNotPreviouslyAdded_Throws()
    {
        // Arrange
        using var context = factory.CreateDbContext();
        MockNode.Create(out var A);
        MockNode.Create(out var B);
        // Act & Assert
        Assert.Throws<ArgumentException>(() => context.Nodes.AddBefore(A, B));
    }

    /// <summary>
    /// |- A     => |- A
    ///             \- A
    /// </summary>
    [Fact]
    public void AddChild_PreviouslyAddedNode_Throws()
    {
        using var context = factory.CreateDbContext();
        MockNode.Create(out var A);
        context.Nodes.AddChild(A);
        Assert.Throws<ArgumentException>(() => context.Nodes.AddChild(A));
    }

    /// <summary>
    /// \- A => \- A
    ///            \- A1
    /// </summary>
    [Fact]
    public async Task AddChild_ToParent_ValidHierarchy()
    {
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            MockNode.Create(out var A1);
            context.Nodes.AddChild(A1, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var A1);
            TreeAssert.Node(A1);
            TreeAssert.Child(A1, A);
        }
    }

    /// <summary>
    /// \- A     => \- A
    ///    \- A1       |- A1
    ///                \- A2
    /// </summary>
    [Fact]
    public async Task AddChild_ToParentWithChildren_ValidHierarchy()
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
            TreeAssert.Single(context.Nodes, out var A);
            MockNode.Create(out var A2);
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var A1);
            TreeAssert.Single(context.Nodes, out var A2);
            TreeAssert.Node(A1);
            TreeAssert.Node(A2);
            TreeAssert.Child(A1, A);
            TreeAssert.Child(A2, A);
            TreeAssert.Siblings(A1, A2);
        }
    }

    /// <summary>
    /// |- A     => |- A
    /// |  \- A1    |  |- A1
    /// \- B        |  \- A2
    ///             \- B
    /// </summary>
    [Fact]
    public async Task AddChild_ToParentWithChildrenAndSiblings_ValidHierarchy()
    {
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            MockNode.Create(out var B);
            MockNode.Create(out var A1);
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(A1, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            MockNode.Create(out var A2);
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Single(context.Nodes, out var A1);
            TreeAssert.Single(context.Nodes, out var A2);
            TreeAssert.Node(A1);
            TreeAssert.Node(A2);
            TreeAssert.Child(A1, A);
            TreeAssert.Child(A2, A);
            TreeAssert.Siblings(A1, A2);
            TreeAssert.Siblings(A, B);
        }
    }

    /// <summary>
    ///          => |- A
    ///             \- B
    /// </summary>
    [Fact]
    public void AddChild_ToParentPreviouslyNotAdded_Throws()
    {
        using var context = factory.CreateDbContext();
        MockNode.Create(out var A);
        MockNode.Create(out var B);
        Assert.Throws<ArgumentException>(() => context.Nodes.AddChild(A, B));
    }

    /// <summary>
    ///     => \- A
    /// </summary>
    [Fact]
    public async Task AddChild_ToRoot_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
        }

        // Act
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Node(A);
        }
    }

    /// <summary>
    /// \- A => |- A
    ///         \- B
    /// </summary>
    [Fact]
    public async Task AddChild_ToRootWithChildren_ValidHierarchy()
    {
        // Arrange
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var A);
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = factory.CreateDbContext())
        {
            MockNode.Create(out var B);
            context.Nodes.AddChild(B);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = factory.CreateDbContext())
        {
            TreeAssert.Single(context.Nodes, out var A);
            TreeAssert.Single(context.Nodes, out var B);
            TreeAssert.Node(A);
            TreeAssert.Node(B);
            TreeAssert.Siblings(A, B);
        }
    }

    public void Dispose() => factory.Dispose();
}
