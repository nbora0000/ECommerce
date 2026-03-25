namespace SharedLibrary.Interfaces
{
    /// <summary>
    /// Unit of Work — coordinates SaveChanges across repositories in a single transaction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
