using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HAC.EFTree;

public static class DbSetExtensions
{
    public static void AddBefore<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        var start = sibling?.Left ?? set.MinLeft() ?? default;
        set.Add(node, start);
    }

    public static void AddAfter<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        var start = (sibling?.Right + 1) ?? (set.MaxRight() + 1) ?? default;
        set.Add(node, start);
    }

    public static void AddChild<T>(this DbSet<T> set, T node, T? parent = default)
        where T : Node
    {
        if (node.SafeAdd is true)
            throw new ArgumentException("Can't node that has been added previously.");
        if (parent is not null && parent?.SafeAdd is null)
            throw new ArgumentException("Parent should be added first.");
        var start = parent?.Right ?? (set.MaxRight() + 1) ?? default;
        set.Add(node, start);
    }

    static void Add<T>(this DbSet<T> set, T node, long start)
        where T : Node
    {
        set.Shift(start, 2);
        node.Left = start;
        node.Right = node.Left + 1;
        node.SafeAdd = true;
        set.Add(node);
    }

    static long? MaxRight<T>(this DbSet<T> set) where T : Node => set.Extremum(x => x == null ? null : x.Right, false);

    static long? MinLeft<T>(this DbSet<T> set) where T : Node => set.Extremum(x => x == null ? null : x.Left, true);

    static long? Extremum<T>(this DbSet<T> set, Expression<Func<T?, long?>> expression, bool min)
            where T : Node
    {
        var func = expression.Compile();
        var local = min ? set.Local.DefaultIfEmpty().Min(func) : set.Local.DefaultIfEmpty().Max(func);
        var db = min ? set.DefaultIfEmpty().Min(expression) : set.DefaultIfEmpty().Max(expression);
        if (local is not null && db is not null)
            return min ? Math.Min(local.Value, db.Value) : Math.Max(local.Value, db.Value);
        if (local is not null)
            return local;
        if (db is not null)
            return db;
        return null;
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
