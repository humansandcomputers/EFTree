namespace HAC.EFTree.Tests.Helpers;

class TreeAssert
{
    public static void Node(MockNode node) => Assert.True(node.Left < node.Right,
         $"Invalid node '{node}': Left ({node.Left}) must be less than Right ({node.Right}).");

    public static void Child(MockNode child, MockNode parent) => Assert.True(parent.Left < child.Left && child.Right < parent.Right,
        $"Invalid hierarchy: parent node {parent} should be surrounding child node {child}");

    public static void Siblings(params MockNode[] siblings) => _ = siblings.Aggregate((a, b) =>
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