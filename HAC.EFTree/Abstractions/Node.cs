using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace HAC.EFTree.Abstractions;

[Index(nameof(Left), IsUnique = true)]
[Index(nameof(Right), IsUnique = true)]
public class Node
{
    public Guid Id { get; set; }
    internal long Left { get; set; }
    internal long Right { get; set; }
    [Required] public bool? SafeAdd { get; internal set; }

    public Node() { }
    internal Node(long position)
    {
        Left = position;
        Right = position + 1;
        SafeAdd = true;
    }
}