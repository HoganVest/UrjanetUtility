using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Base;
using Hoganvest.Data.Repository.Models;

namespace Hoganvest.Data.Repository
{
    class UrjanetStatementRepository : Repository<UrjanetStatements>, IUrjanetStatementRepository
    {
        public UrjanetStatementRepository(HoganvestContext context)
           : base(context)
        { }
    }
}
