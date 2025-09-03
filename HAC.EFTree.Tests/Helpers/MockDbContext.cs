namespace HAC.EFTree.Tests.Helpers;

class MockDbContext(DbContextOptions<MockDbContext> options) : DbContext(options)
{
    public DbSet<MockNode> Nodes { get; set; }
}