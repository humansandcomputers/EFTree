using System.Runtime.CompilerServices;

namespace HAC.EFTree.Tests.Helpers;

class MockNode : Node
{
    public static void Create(out MockNode node,
        [CallerArgumentExpression(nameof(node))] string name = "") => node = new() { Name = name.Replace("var ", "") };
    public required string Name { get; set; }
    public override string ToString() => $"{Name} ({Left} {Right})";
}
