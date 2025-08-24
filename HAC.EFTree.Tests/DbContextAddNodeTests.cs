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
            var B = Assert.Single(context.Nodes, x => x.Name == "A");
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

    static void AssertNode(Node node) => Assert.True(node.Left < node.Right,
             $"Left:{node.Left} should be less than Right: {node.Right}.");

    static void AssertChild(Node child, Node parent)
    {
        Assert.True(parent.Left < child.Left,
             $"Child's Left: {child.Left} should be greater than parent's Left: {parent.Left}.");
        Assert.True(child.Right < parent.Right,
             $"Child's Right: {child.Right} should be less than parent's Right: {parent.Left}.");
    }

    static void AssertSibling(params Node[] siblings)
    {
        _ = siblings.Aggregate((a, b) =>
            {
                Assert.True(a.Right + 1 == b.Left,
                $"Sibling's Right: {a.Right} should be less than next sibling's Left: {a.Left}"); return b;
            });
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
