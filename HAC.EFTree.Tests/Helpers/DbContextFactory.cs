namespace HAC.EFTree.Tests.Helpers;

class DbContextFactory<TContext> : IDbContextFactory<TContext> where TContext : DbContext
{
    readonly DbContextOptions<TContext> options = new DbContextOptionsBuilder<TContext>()
         .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    public TContext CreateDbContext() => (TContext)Activator.CreateInstance(typeof(TContext), options)!;
}