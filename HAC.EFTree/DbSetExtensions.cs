using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace HAC.EFTree;

public static class DbSetExtensions
{
    static IEnumerable<T> WhereAll<T>(this DbSet<T> set, Expression<Func<T, bool>> predicate)
        where T : Node
        => set.Where(predicate).AsEnumerable().Concat(set.Local.Where(predicate.Compile()));

    static void Shift<T>(this DbSet<T> set, long offset, long? start = default, long? end = default, bool? registered = default)
        where T : Node => set.UpdateAll((right, x) => x.SetOffset(offset, right), start, end, registered);

    static void UpdateAll<T>(this DbSet<T> set, Action<T> action, long? start = default, long? end = default, bool? registered = default)
        where T : Node => set.UpdateAll((_, x) => action(x), start, end, registered);

    static void UpdateAll<T>(this DbSet<T> set, Action<bool, T> action, long? start = default, long? end = default, bool? registered = default)
        where T : Node => set.UpdateAll(action, right => GetRangeExpression<T>(right, start, end, registered));

    static void UpdateAll<T>(this DbSet<T> set, Action<bool, T> action, Func<bool, Expression<Func<T, bool>>> predicate)
        where T : Node
    {
        foreach (var x in set.WhereAll(predicate(false)).Distinct())
            action(false, x);

        foreach (var x in set.WhereAll(predicate(true)).Distinct())
            action(true, x);
    }

    static Expression<Func<T, bool>> GetRangeExpression<T>(bool right, long? start = default, long? end = default, bool? registered = default)
        where T : Node
    {
        var param = Expression.Parameter(typeof(T), "x");
        var side = Expression.Property(param, right ? nameof(Node.Right) : nameof(Node.Left));
        var safeAdd = Expression.Property(param, nameof(Node.SafeAdd));

        var all = new List<Expression>();
        if (start is not null)
            all.Add(Expression.GreaterThanOrEqual(side, Expression.Constant(start.Value)));
        if (end is not null)
            all.Add(Expression.LessThan(side, Expression.Constant(end.Value)));
        if (registered is not null)
            all.Add(Expression.Equal(safeAdd, Expression.Constant((bool?)registered.Value, typeof(bool?))));

        if (all.Count is 0)
            throw new Exception();
        if (all.Count is 1)
            return Expression.Lambda<Func<T, bool>>(all.First(), param);
        return Expression.Lambda<Func<T, bool>>(all.Aggregate(Expression.And), param);
    }

    static void Add<T>(this DbSet<T> set, T node, long start)
        where T : Node
    {
        set.Shift(2, start);
        node.Left = start;
        node.Right = node.Left + 1;
        node.Register();
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
        set.UpdateAll(x => x.UnRegister(), hole.Start, hole.End);
        set.Shift(holeMove, hole.Start, hole.End);
        set.Shift(patchMove, patch.Start, patch.End, true);
        set.UpdateAll(x => x.Register(), hole.Start, hole.End);
    }

    class Span(long a, long b)
    {
        public long Start { get; } = Math.Min(a, b);
        public long End { get; } = Math.Max(a, b);
        public long Length { get; } = Math.Abs(b - a);
    }
}
