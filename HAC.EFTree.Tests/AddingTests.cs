namespace HAC.EFTree.Tests;
public class AddingTests
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

    /// <summary>
    /// \- A => |- A
    ///         \- B
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChildToRoot_WithChildren_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var B = new MockNode() { Name = "B" };
            context.Nodes.AddChild(B);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var B = Assert.Single(context.Nodes, x => x.Name == "B");
            TreeAssert.Node(A);
            TreeAssert.Node(B);
            TreeAssert.Siblings(A, B);
        }
    }

    /// <summary>
    /// \- A => \- A
    ///            \- A1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChildToParent_Empty_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = context.Nodes.Single(x => x.Name == "A");
            var A1 = new MockNode() { Name = "A1" };
            context.Nodes.AddChild(A1, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            TreeAssert.Node(A1);
            TreeAssert.Child(A1, A);
        }
    }

    /// <summary>
    /// \- A     => \- A
    ///    \- A1       |- A1
    ///                \- A2
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChildToParent_WithChildren_CorrectPositions()
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
            var A = context.Nodes.Single(x => x.Name == "A");
            var A2 = new MockNode() { Name = "A2" };
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            var A2 = Assert.Single(context.Nodes, x => x.Name == "A2");
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
    /// <returns></returns>
    [Fact]
    public async Task AddChildToParent_WithChildrenAndSiblings_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            var B = new MockNode() { Name = "B" };
            var A1 = new MockNode() { Name = "A1" };
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(A1, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = context.Nodes.Single(x => x.Name == "A");
            var A2 = new MockNode() { Name = "A2" };
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var B = Assert.Single(context.Nodes, x => x.Name == "B");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            var A2 = Assert.Single(context.Nodes, x => x.Name == "A2");
            TreeAssert.Node(A1);
            TreeAssert.Node(A2);
            TreeAssert.Child(A1, A);
            TreeAssert.Child(A2, A);
            TreeAssert.Siblings(A1, A2);
            TreeAssert.Siblings(A, B);
        }
    }

    /// <summary>
    /// |- A     => |- A
    ///             \- B
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddAfterRoot_WithOtherSiblings_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            context.Nodes.AddChild(A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var B = new MockNode() { Name = "B" };
            context.Nodes.AddAfter(B);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var B = Assert.Single(context.Nodes, x => x.Name == "B");
            TreeAssert.Node(A);
            TreeAssert.Node(B);
            TreeAssert.Siblings(A, B);
        }
    }

    /// <summary>
    /// |- A     => |- A
    /// |  |- A1    |  |- A1
    /// |  \- A2    |  |- A1_2
    /// \- B        |  \- A2
    ///             \- B
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddAfterNode_WithOtherSiblings_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            var B = new MockNode() { Name = "B" };
            var A1 = new MockNode() { Name = "A1" };
            var A2 = new MockNode() { Name = "A2" };
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(A1, A);
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A1 = context.Nodes.Single(x => x.Name == "A1");
            var A1_2 = new MockNode() { Name = "A1_2" };
            context.Nodes.AddAfter(A1_2, A1);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var B = Assert.Single(context.Nodes, x => x.Name == "B");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            var A1_2 = Assert.Single(context.Nodes, x => x.Name == "A1_2");
            var A2 = Assert.Single(context.Nodes, x => x.Name == "A2");
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
    ///             \- B
    ///                |- B1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddBeforeRoot_WithOtherSiblings_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var B = new MockNode() { Name = "B" };
            var B1 = new MockNode() { Name = "B1" };
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(B1, B);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            context.Nodes.AddBefore(A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var B = Assert.Single(context.Nodes, x => x.Name == "B");
            var B1 = Assert.Single(context.Nodes, x => x.Name == "B1");
            TreeAssert.Node(A);
            TreeAssert.Node(B);
            TreeAssert.Node(B1);
            TreeAssert.Siblings(A, B);
            TreeAssert.Child(B1, B);
        }
    }

    /// <summary>
    /// |- A     => |- A
    /// |  |- A1    |  |- A1
    /// |  \- A2    |  |- A1_2
    /// \- B        |  \- A2
    ///             \- B
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddBeforeNode_WithOtherSiblings_CorrectPositions()
    {
        using (var context = factory.CreateDbContext())
        {
            var A = new MockNode() { Name = "A" };
            var B = new MockNode() { Name = "B" };
            var A1 = new MockNode() { Name = "A1" };
            var A2 = new MockNode() { Name = "A2" };
            context.Nodes.AddChild(A);
            context.Nodes.AddChild(B);
            context.Nodes.AddChild(A1, A);
            context.Nodes.AddChild(A2, A);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A2 = context.Nodes.Single(x => x.Name == "A2");
            var A1_2 = new MockNode() { Name = "A1_2" };
            context.Nodes.AddBefore(A1_2, A2);
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var A = Assert.Single(context.Nodes, x => x.Name == "A");
            var B = Assert.Single(context.Nodes, x => x.Name == "B");
            var A1 = Assert.Single(context.Nodes, x => x.Name == "A1");
            var A1_2 = Assert.Single(context.Nodes, x => x.Name == "A1_2");
            var A2 = Assert.Single(context.Nodes, x => x.Name == "A2");
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

    [Fact]
    public void AddChild_PreviouslyAddedNode_Throws()
    {
        using var context = factory.CreateDbContext();
        var A = new MockNode() { Name = "A" };
        context.Nodes.AddChild(A);
        Assert.Throws<ArgumentException>(() => context.Nodes.AddChild(A));
    }

    [Fact]
    public void AddChild_ToPreviouslyNotAddedParent_Throws()
    {
        using var context = factory.CreateDbContext();
        var A = new MockNode() { Name = "A" };
        var B = new MockNode() { Name = "B" };
        Assert.Throws<ArgumentException>(() => context.Nodes.AddChild(A, B));
    }

    [Fact]
    public void AddBefore_PreviouslyAddedNode_Throws()
    {
        using var context = factory.CreateDbContext();
        var A = new MockNode() { Name = "A" };
        context.Nodes.AddChild(A);
        Assert.Throws<ArgumentException>(() => context.Nodes.AddBefore(A));
    }

    [Fact]
    public void AddBefore_ToPreviouslyNotAddedSibling_Throws()
    {
        using var context = factory.CreateDbContext();
        var A = new MockNode() { Name = "A" };
        var B = new MockNode() { Name = "B" };
        Assert.Throws<ArgumentException>(() => context.Nodes.AddBefore(A, B));
    }

    [Fact]
    public void AddAfter_PreviouslyAddedNode_Throws()
    {
        using var context = factory.CreateDbContext();
        var A = new MockNode() { Name = "A" };
        context.Nodes.AddChild(A);
        Assert.Throws<ArgumentException>(() => context.Nodes.AddAfter(A));
    }

    [Fact]
    public void AddAfter_ToPreviouslyNotAddedSibling_Throws()
    {
        using var context = factory.CreateDbContext();
        var A = new MockNode() { Name = "A" };
        var B = new MockNode() { Name = "B" };
        Assert.Throws<ArgumentException>(() => context.Nodes.AddAfter(A, B));
    }
}
