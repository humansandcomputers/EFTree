using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HAC.EFTree.Abstractions;

[Index(nameof(Left), IsUnique = true)]
[Index(nameof(Right), IsUnique = true)]
public abstract class Node
{
    public Guid Id { get; set; }
    internal long Left { get; set; }
    internal long Right { get; set; }
    [Required] public bool? SafeAdd { get; private set; }

    public Node() { }
    internal Node(long position)
    {
        Left = position;
        Right = position + 1;
        SafeAdd = true;
    }

    internal void Removed() => SafeAdd = null;
    internal void Added() => SafeAdd = true;
}