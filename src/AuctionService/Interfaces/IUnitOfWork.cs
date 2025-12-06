namespace AuctionService.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAuctionRepository AuctionRepository { get; }
    Task<bool> CompleteAsync();
    bool HasChanges();
}
