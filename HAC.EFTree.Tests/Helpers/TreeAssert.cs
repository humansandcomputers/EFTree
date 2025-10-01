using System.Runtime.CompilerServices;

namespace HAC.EFTree.Tests.Helpers;

class TreeAssert
{
    public static void Single(IEnumerable<MockNode> set, out MockNode node, [CallerArgumentExpression(nameof(node))] string name = "")
        => node = Assert.Single(set, x => x.Name == name.Replace("var ", ""));

    public static void Node(MockNode node) => Assert.True(node.Left < node.Right,
         $"Invalid node '{node}': Left ({node.Left}) must be less than Right ({node.Right}).");

    public static void Child(MockNode child, MockNode parent) => Assert.True(parent.Left < child.Left && child.Right < parent.Right,
        $"Invalid hierarchy: parent node {parent} should be surrounding child node {child}");

    public static void Lineage(params MockNode[] nodes)
    {
        for (var i = 0; i < nodes.Length - 1; i++)
            Child(nodes[i + 1], nodes[i]);
    }

    public static void Siblings(params MockNode[] siblings) => _ = siblings.Aggregate((a, b) =>
        {
            Assert.True(a.Right + 1 == b.Left,
                $"Invalid sibling order: Node {a} should directly precede Node {b}.");
            return b;
        });
}
