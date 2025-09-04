namespace HAC.EFTree.Tests.Helpers;

class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
{
    public DbSet<MockNode> Nodes { get; set; }

    public override string ToString() => Nodes.ToString(x => x.Name);
}