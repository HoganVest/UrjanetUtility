using System;
using System.Threading.Tasks;


namespace Hoganvest.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> CommitAsync();
        IUrjanetStatementRepository UrjanetStatement { get; }
    }
}
