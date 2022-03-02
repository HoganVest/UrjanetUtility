using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Base;
using Hoganvest.Data.Repository.Models;

namespace Hoganvest.Data.Repository
{
   public class CredentialDetailsRepository : Repository<CredentialDetails>, ICredentialDetailsRepository
    {
        public CredentialDetailsRepository(HoganvestContext context)
           : base(context)
        { }
    }
}
