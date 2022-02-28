using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Base;
using Hoganvest.Data.Repository.Models;

namespace Hoganvest.Data.Repository
{
   public class CrdentialDetailsRepository : Repository<CrdentialDetails>, ICrdentialDetailsRepository
    {
        public CrdentialDetailsRepository(HoganvestContext context)
           : base(context)
        { }
    }
}
