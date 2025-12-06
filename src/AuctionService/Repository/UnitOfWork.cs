using AuctionService.Data;
using AuctionService.Interfaces;
using AutoMapper;

namespace AuctionService.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public UnitOfWork(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        AuctionRepository = new AuctionRepository(_context, _mapper);
    }

    public IAuctionRepository AuctionRepository { get; private set; }

    public async Task<bool> CompleteAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return _context.ChangeTracker.HasChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
