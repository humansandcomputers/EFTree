namespace HAC.EFTree.Tests.Helpers;
class TreeNode(string? name, params IEnumerable<TreeNode> children) : List<TreeNode>(children)
{
    public string? Name { get; set; } = name;
    public static implicit operator TreeNode(string name) => new(name);
}

class Examples
{
    public static TreeNode E1 { get; } = new(null)
    {
        "A",
        new("B")
        {
            "B1",
            "B2",
        }
    };
}