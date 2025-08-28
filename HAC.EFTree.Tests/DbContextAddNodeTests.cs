namespace HAC.EFTree.Tests;

public class DbSetAddingTests
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
            AssertNode(A);
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
            AssertNode(A);
            AssertNode(B);
            AssertSibling(A, B);
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
            AssertNode(A1);
            AssertChild(A1, A);
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
            AssertNode(A1);
            AssertNode(A2);
            AssertChild(A1, A);
            AssertChild(A2, A);
            AssertSibling(A1, A2);
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
            AssertNode(A1);
            AssertNode(A2);
            AssertChild(A1, A);
            AssertChild(A2, A);
            AssertSibling(A1, A2);
            AssertSibling(A, B);
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
            AssertNode(A);
            AssertNode(B);
            AssertSibling(A, B);
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
            AssertNode(A1);
            AssertNode(A1_2);
            AssertNode(A2);
            AssertChild(A1, A);
            AssertChild(A1_2, A);
            AssertChild(A2, A);
            AssertSibling(A, B);
            AssertSibling(A1, A1_2, A2);
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
            AssertNode(A);
            AssertNode(B);
            AssertNode(B1);
            AssertSibling(A, B);
            AssertChild(B1, B);
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
            AssertNode(A1);
            AssertNode(A1_2);
            AssertNode(A2);
            AssertChild(A1, A);
            AssertChild(A1_2, A);
            AssertChild(A2, A);
            AssertSibling(A, B);
            AssertSibling(A1, A1_2, A2);
        }
    }

    static void AssertNode(MockNode node) => Assert.True(node.Left < node.Right,
        $"Invalid node '{node}': Left ({node.Left}) must be less than Right ({node.Right}).");

    static void AssertChild(MockNode child, MockNode parent)
    {
        Assert.True(parent.Left < child.Left && child.Right < parent.Right,
            $"Invalid hierarchy: parent node {parent} should be surrounding child node {child}");
    }

    static void AssertSibling(params MockNode[] siblings)
    {
        _ = siblings.Aggregate((a, b) =>
        {
            Assert.True(a.Right + 1 == b.Left,
                $"Invalid sibling order: Node {a} should directly precede Node {b}.");
            return b;
        });
    }

    class MockNode : Node
    {
        public required string Name { get; set; }
        public override string ToString() => $"{Name} ({Left} {Right})";
    }

    class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
    {
        public DbSet<MockNode> Nodes { get; set; }
    }
}
