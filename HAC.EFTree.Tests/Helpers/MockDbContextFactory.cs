using Xunit.Abstractions;
namespace HAC.EFTree.Tests.Helpers;

class MockDbContextFactory(ITestOutputHelper output) : IDbContextFactory<MockDbContext>, IDisposable
{
    readonly StringColumnBuilder stringColumnBuilder = new();
    readonly DbContextOptions<MockDbContext> options = new DbContextOptionsBuilder<MockDbContext>()
         .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    public MockDbContext CreateDbContext() => new(options, stringColumnBuilder);

    public void Dispose() => output.WriteLine(stringColumnBuilder.ToString());
}

class MockDbContext(DbContextOptions<MockDbContext> options, StringColumnBuilder output) : DbContext(options)
{
    public DbSet<MockNode> Nodes { get; set; }

    public override string ToString() => Nodes.ToString(x => x.Name);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        output.AddColumns(this);
        return result;
    }
}