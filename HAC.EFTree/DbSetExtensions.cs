using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HAC.EFTree;

public static class DbSetExtensions
{
    readonly static Node virtualNode = new VirtualNode() { Left = -2, Right = -1 };

    public static void AddAfter<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        var start = (sibling?.Right ?? set.MaxRight() ?? -1) + 1;
        set.Add(node, start);
    }

    public static void AddBefore<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        var start = sibling?.Left ?? set.MinLeft() ?? 0;
        set.Add(node, start);
    }

    public static void AddChild<T>(this DbSet<T> set, T node, T? parent = default)
        where T : Node
    {
        var start = parent?.Right ?? ((set.MaxRight() ?? -1) + 1);
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

    static long? Extremum<T>(this DbSet<T> set, Func<T?, long?> selector, Expression<Func<T?, long?>> expression, bool min)
            where T : Node
    {
        var db = set.DefaultIfEmpty();
        var local = set.Local.DefaultIfEmpty();

        var dbValue = min ? db.Min(expression) : db.Max(expression);
        var localValue = min ? local.Min(selector) : set.Max(selector);

        if (dbValue is not null && localValue is not null)
            return Math.Max(dbValue.Value, localValue.Value);
        if (dbValue is not null)
            return dbValue;
        else if (localValue is not null)
            return localValue;
        return null;
    }

    static long? MaxRight<T>(this DbSet<T> set)
            where T : Node
    {
        var localMax = set.Local.DefaultIfEmpty().Max(x => x?.Right);
        var dbMax = set.DefaultIfEmpty().Max(x => x is null ? null : x.Right);
        if (localMax is { } lm && dbMax is { } dm)
            return Math.Max(lm, dm);
        if (localMax is null)
            return dbMax;
        if (dbMax is null)
            return localMax;
        return null;
    }

    static long? MinLeft<T>(this DbSet<T> set)
            where T : Node
    {
        var localMin = set.Local.DefaultIfEmpty().Min(x => x?.Left);
        var dbMin = set.DefaultIfEmpty().Min(x => x == null ? null : x.Left);
        if (localMin is { } lm && dbMin is { } dm)
            return Math.Max(lm, dm);
        if (localMin is null)
            return dbMin;
        if (dbMin is null)
            return localMin;
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

    class VirtualNode : Node;
}
