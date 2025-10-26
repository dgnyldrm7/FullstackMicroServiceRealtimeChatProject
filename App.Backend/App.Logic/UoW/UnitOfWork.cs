using App.Core.Entities;
using App.Core.Interface;
using App.Persistance.Context;

namespace App.Logic.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IRepository<AppUser> _userRepository;

        public UnitOfWork(AppDbContext context, IRepository<AppUser> userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public void Dispose()
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken)
        {
            return _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            return _context.Database.CommitTransactionAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return _context.Database.RollbackTransactionAsync(cancellationToken);
        }
    }
}