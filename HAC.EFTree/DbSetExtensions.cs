using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HAC.EFTree;

public static class DbSetExtensions
{
    class VirtualNode : Node { };
    readonly static Node virtualNode = new VirtualNode() { Left = -2, Right = -1 };
    public static void InsertAfter<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        var start = (sibling?.Right ?? set.MaxRight()) + 1;
        set.Shift(start, 2);
        node.Left = start;
        node.Right = node.Left + 1;
        node.SafeAdd = true;
        set.Add(node);
    }

    public static void InsertBefore<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {

    }

    public static void AddChild<T>(this DbSet<T> set, T node, T? parent = default)
        where T : Node
    {
        var start = parent?.Right ?? set.MaxRight() + 1;
        set.Shift(start, 2);
        node.Left = start;
        node.Right = node.Left + 1;
        node.SafeAdd = true;
        set.Add(node);
    }

    static long MaxRight<T>(this DbSet<T> set)
            where T : Node
    {
        var localMax = set.Local.DefaultIfEmpty().Max(x => x?.Right);
        var dbMax = set.DefaultIfEmpty().Max(x => x == null ? null : x.Right);
        return new[] { virtualNode.Right, localMax, dbMax }.OfType<long>().Max();
    }

    static void Shift<T>(this DbSet<T> set, long start, long offset)
        where T : Node
    {
        var lefts = set.Where(x => start <= x.Left).AsEnumerable().Concat(set.Local.Where(x => start <= x.Left)).Distinct();
        foreach (var x in lefts)
            x.Left += offset;

        var rights = set.Where(x => start <= x.Right).AsEnumerable().Concat(set.Local.Where(x => start <= x.Right)).Distinct();
        foreach (var x in rights)
            x.Right += offset;
    }
}
