using HAC.EFTree.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml;

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
        var min = Math.Min(start, end);
        var max = Math.Max(start, end);

        Expression<Func<T, bool>> leftExpression = x => min <= x.Left && x.Left < max;
        var leftFunc = leftExpression.Compile();
        var lefts = set.Where(leftExpression).AsEnumerable().Concat(set.Local.Where(leftFunc)).Distinct();
        foreach (var x in lefts)
            x.Left += offset;

        Expression<Func<T, bool>> rightExpression = x => min <= x.Right && x.Right < max;
        var rightFunc = rightExpression.Compile();
        var rights = set.Where(rightExpression).AsEnumerable().Concat(set.Local.Where(rightFunc)).Distinct();
        foreach (var x in rights)
            x.Right += offset;
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

    /*
    0   7     10      14    17  19
        L__S__R       L__T__R
        |-----||===========|
         hole      patch
               L______T_____R
                     L__S__R
        |===========||-----|
            patch      hole

    0   2     3       7     10  19
        L__T__R       L__S__R
              |======||-----|
               patch   hole
        L_____T______R
              L__S__R
              |-----||======|
               hole   patch
     */
    public static void Move<T>(this DbSet<T> set, T node, T? parent = default)
        where T : Node
    {
        var target = parent?.Right ?? (set.MaxRight() + 1) ?? default;
        var ts = parent is not null && node.Left > parent.Right ? node.Left : node.Right + 1;
        var patch = new Span(ts, target);
        var hole = new Span(node.Left, node.Right + 1);
        var back = parent is not null && node.Left > parent.Right ? hole.Length : -hole.Length;
        set.Shift(hole.Start, hole.End, -hole.End);
        set.Shift(patch.Start, patch.End, back);
        set.Shift(hole.Start - hole.End, 0, hole.End + patch.Length);
    }

    record Span(long Start, long End)
    {
        public long Length => End - Start;
    }
}
