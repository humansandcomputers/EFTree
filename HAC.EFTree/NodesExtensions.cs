using HAC.EFTree.Abstractions;

namespace HAC.EFTree;

public static class NodesExtensions
{
    public static string ToString<T>(this IEnumerable<T> nodes, Func<T, string> toString)
        where T : Node
    {
        var nodeList = nodes.OrderBy(n => n.Left).ToList();
        var result = new List<string>();

        // Build a lookup for children
        var lookup = nodeList.ToLookup(
            n => nodeList.FirstOrDefault(parent =>
                parent.Left < n.Left && n.Right < parent.Right &&
                nodeList.All(other => !(parent.Left < other.Left && other.Right < parent.Right && other.Left < n.Left && n.Right < other.Right))
            ),
            n => n);

        // Find root nodes (nodes with no parent)
        var roots = nodeList.Where(n =>
            !nodeList.Any(parent => parent.Left < n.Left && n.Right < parent.Right)).ToList();

        void Print(T node, string prefix, bool isLast)
        {
            var connector = isLast ? "\\- " : "|- ";
            result.Add(prefix + connector + toString(node));

            var children = lookup[node].OrderBy(n => n.Left).ToList();
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var lastChild = i == children.Count - 1;
                var childPrefix = $"{prefix}{(isLast ? "   " : "|  ")}";
                Print(child, childPrefix, lastChild);
            }
        }

        for (int i = 0; i < roots.Count; i++)
        {
            var root = roots[i];
            bool isLastRoot = i == roots.Count - 1;
            Print(root, "", isLastRoot);
        }

        return string.Join(Environment.NewLine, result);
    }
}
