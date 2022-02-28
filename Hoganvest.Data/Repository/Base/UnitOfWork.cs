using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Models;
using System.Threading.Tasks;

namespace Hoganvest.Data.Repository.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Private Variables

        private HoganvestContext _context;
        private UrjanetStatementRepository _urjanetStatementRepository;
        private UrjanetCredentialsRepository _urjanetCredentialsRepository;
        private CrdentialDetailsRepository _crdentialDetailsRepository;
        #endregion Private Variables

        public UnitOfWork(HoganvestContext context)
        {
            this._context = context;
        }
        public IUrjanetStatementRepository UrjanetStatement => _urjanetStatementRepository = _urjanetStatementRepository ?? new UrjanetStatementRepository(_context);

        public IUrjanetCredentialsRepository UrjanetCredentials => _urjanetCredentialsRepository = _urjanetCredentialsRepository ?? new UrjanetCredentialsRepository(_context);
        public ICrdentialDetailsRepository CrdentialDetails => _crdentialDetailsRepository = _crdentialDetailsRepository ?? new CrdentialDetailsRepository(_context);
        #region Base Methods
        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #endregion Base Methods
    }
}
