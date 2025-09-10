using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace HAC.EFTree;

public static class DbSetExtensions
{
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

    static void Shift<T>(this DbSet<T> set, long start, long end, long offset)
        where T : Node
    {
        foreach (var x in set.WhereAll(x => start <= x.Left && x.Left < end).Distinct())
            x.Left += offset;

        foreach (var x in set.WhereAll(x => start <= x.Right && x.Right < end).Distinct())
            x.Right += offset;
    }

    static Expression<Func<T, bool>> GetExpression<T>(bool right, long? a = default, long? b = default)
        where T : Node
        => (a, b) switch
        {
            (not null, null) => right ? x => a.Value <= x.Right
                                      : x => a.Value <= x.Left,
            (null, not null) => right ? x => x.Right < b.Value
                                      : x => x.Left < b.Value,
            (not null, not null) => right ? x => a.Value <= x.Right && x.Right < b.Value :
                                            x => a.Value <= x.Left && x.Left < b.Value,
            _ => throw new ArgumentException($"Both {nameof(a)} and {nameof(b)} could not be null.")
        };

    static IEnumerable<T> WhereAll<T>(this DbSet<T> set, Expression<Func<T, bool>> expression)
        where T : Node
        => set.Where(expression).AsEnumerable().Concat(set.Local.Where(expression.Compile()));

    static void Add<T>(this DbSet<T> set, T node, long start)
        where T : Node
    {
        set.Shift(start, 2);
        node.Left = start;
        node.Right = node.Left + 1;
        node.Added();
        set.Add(node);
    }

    static void ValidateAdd<T>(T node, T? toNode = default, [CallerArgumentExpression(nameof(toNode))] string? name = default)
        where T : Node
    {
        if (node.SafeAdd is true)
            throw new ArgumentException("Can't add node that has been added previously.");
        if (toNode is not null && toNode?.SafeAdd is not true)
            throw new ArgumentException($"Can't add node to a {name} node that is not added yet.");
    }

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

    static long? MaxRight<T>(this DbSet<T> set) where T : Node => set.Extremum(x => x == null ? null : x.Right, false);

    static long? MinLeft<T>(this DbSet<T> set) where T : Node => set.Extremum(x => x == null ? null : x.Left, true);

    public static void AddBefore<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        ValidateAdd(node, sibling);
        var start = sibling?.Left ?? set.MinLeft() ?? default;
        set.Add(node, start);
    }

    public static void AddAfter<T>(this DbSet<T> set, T node, T? sibling = default)
        where T : Node
    {
        ValidateAdd(node, sibling);
        var start = (sibling?.Right + 1) ?? (set.MaxRight() + 1) ?? default;
        set.Add(node, start);
    }

    public static void AddChild<T>(this DbSet<T> set, T node, T? parent = default)
        where T : Node
    {
        ValidateAdd(node, parent);
        var start = parent?.Right ?? (set.MaxRight() + 1) ?? default;
        set.Add(node, start);
    }

    public static void Move<T>(this DbSet<T> set, T node, T? parent = default)
        where T : Node
    {
        var right = parent?.Right ?? (set.MaxRight() + 1) ?? default;
        var patch = new Span(right < node.Left ? node.Left : node.Right + 1, right);
        var hole = new Span(node.Left, node.Right + 1);
        var rtl = node.Right < right;
        var holeMove = rtl ? patch.Length : -patch.Length;
        var patchMove = rtl ? -hole.Length : hole.Length;
        set.UnRegisterAll(hole.Start, hole.End);
        set.Shift(hole.Start, hole.End, null, holeMove);
        set.Shift(patch.Start, patch.End, true, patchMove);
        set.RegisterAll(hole.Start, hole.End);
    }

    static void Shift<T>(this DbSet<T> set, long? start, long? end, bool? registered, long offset)
        where T : Node
    {
        set.UpdateAll((right, x) =>
        {
            if (right)
                x.Right += offset;
            else
                x.Left += offset;
        }, start, end, registered);
    }

    static void RegisterAll<T>(this DbSet<T> set, long? start, long? end)
        where T : Node => set.UpdateAll((_, x) => x.Added(), start, end);

    static void UnRegisterAll<T>(this DbSet<T> set, long? start, long? end)
        where T : Node => set.UpdateAll((_, x) => x.Removed(), start, end);

    static void UpdateAll<T>(this DbSet<T> set, Action<bool, T> action, long? start, long? end, bool? registered = default)
        where T : Node
    {
        foreach (var x in set.WhereAll(GetRangeExpression<T>(false, start, end, registered)).Distinct())
            action(false, x);

        foreach (var x in set.WhereAll(GetRangeExpression<T>(true, start, end, registered)).Distinct())
            action(true, x);
    }

    static Expression<Func<T, bool>> GetRangeExpression<T>(bool right, long? a = default, long? b = default, bool? registered = default)
        where T : Node
    {
        if (registered is not null)
            return (a, b) switch
            {
                (not null, null) => right ? x => a.Value <= x.Right && x.SafeAdd == registered
                                          : x => a.Value <= x.Left && x.SafeAdd == registered,
                (null, not null) => right ? x => x.Right < b.Value && x.SafeAdd == registered
                                          : x => x.Left < b.Value && x.SafeAdd == registered && x.SafeAdd == registered,
                (not null, not null) => right ? x => a.Value <= x.Right && x.Right < b.Value && x.SafeAdd == registered :
                                                x => a.Value <= x.Left && x.Left < b.Value && x.SafeAdd == registered,
                _ => throw new ArgumentException($"Both {nameof(a)} and {nameof(b)} could not be null.")
            };

        return (a, b) switch
        {
            (not null, null) => right ? x => a.Value <= x.Right
                                      : x => a.Value <= x.Left,
            (null, not null) => right ? x => x.Right < b.Value
                                      : x => x.Left < b.Value,
            (not null, not null) => right ? x => a.Value <= x.Right && x.Right < b.Value :
                                            x => a.Value <= x.Left && x.Left < b.Value,
            _ => throw new ArgumentException($"Both {nameof(a)} and {nameof(b)} could not be null.")
        };
    }

    class Span(long a, long b)
    {
        public long Start { get; } = Math.Min(a, b);
        public long End { get; } = Math.Max(a, b);
        public long Length { get; } = Math.Abs(b - a);
    }
}
