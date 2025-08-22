using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HAC.EFTree;

public static class DbSetExtensions
{
    static readonly Node virtualRoot = new() { Left = -1, Right = 0 };
    
    public static void InsertAfter(this DbSet<Node> set, Node node, Node? sibling = default)
    {

    }

    public static void InsertBefore(this DbSet<Node> set, Node node, Node? sibling = default)
    {

    }


    public static void AddChild(this DbSet<Node> set, Node node, Node? parent = default)
    {
        var start = (parent ?? virtualRoot).Right;
        set.Shift(start, 2);
        node.Left = start;
        node.Right = node.Left + 1;
        node.SafeAdd = true;
        set.Add(node);
    }

    static void Shift(this DbSet<Node> set, long start, long offset)
    {
        foreach (var x in set.Local.Where(x => start <= x.Left))
            x.Left += offset;
        foreach (var x in set.Local.Where(x => start <= x.Right))
            x.Right += offset;
    }
}
