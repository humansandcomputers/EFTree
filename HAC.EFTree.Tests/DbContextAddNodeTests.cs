namespace HAC.EFTree.Tests;

public class DbSetAddingTests
{
    readonly DbContextFactory<MockDbContext> factory = new();

    /// <summary>
    ///     => |- A
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChild_ToRoot_CorrectPositioning()
    {
        var context = factory.CreateDbContext();
        var id = Guid.NewGuid();
        var node = new Node() { Id = id };
        context.Nodes.AddChild(node);
        await context.SaveChangesAsync();

        var actualNode = Assert.Single(context.Nodes, x => x.Id == id);
        Assert.NotNull(actualNode);
        AssertNode(actualNode);
    }

    /// <summary>
    /// |- A => |- A
    ///            |- A1
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChild_ToNode_CorrectPositioning()
    {
        var context = factory.CreateDbContext();
        var nodeA = new Node() { Id = Guid.NewGuid() };
        var nodeA1 = new Node() { Id = Guid.NewGuid() };
        context.Nodes.AddChild(nodeA);
        context.Nodes.AddChild(nodeA1, nodeA);
        await context.SaveChangesAsync();
        var A = Assert.Single(context.Nodes, x => x.Id == nodeA.Id);
        var A1 = Assert.Single(context.Nodes, x => x.Id == nodeA1.Id);
        AssertNode(A1);
        AssertChild(A1, A);
    }

    /// <summary>
    /// |- A     => |- A
    ///    |- A1       |- A1
    ///                |- A2
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AddChild_ToAlreadyHavingChildNode_CorrectPositioning()
    {
        var context = factory.CreateDbContext();
        var nodeA = new Node() { Id = Guid.NewGuid() };
        var nodeA1 = new Node() { Id = Guid.NewGuid() };
        var nodeA2 = new Node() { Id = Guid.NewGuid() };
        context.Nodes.AddChild(nodeA);
        context.Nodes.AddChild(nodeA1, nodeA);
        context.Nodes.AddChild(nodeA2, nodeA);
        await context.SaveChangesAsync();

        var A = Assert.Single(context.Nodes, x => x.Id == nodeA.Id);
        var A1 = Assert.Single(context.Nodes, x => x.Id == nodeA1.Id);
        var A2 = Assert.Single(context.Nodes, x => x.Id == nodeA2.Id);
        AssertNode(A1);
        AssertChild(A1, A);
        AssertChild(A2, A);
        AssertSibling(A1, A2);
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

    class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
    {
        public DbSet<Node> Nodes { get; set; }
    }
}
