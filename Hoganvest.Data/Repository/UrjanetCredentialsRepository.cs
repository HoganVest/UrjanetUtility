using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Base;
using Hoganvest.Data.Repository.Models;

namespace Hoganvest.Data.Repository
{
   public class UrjanetCredentialsRepository : Repository<Credential>, IUrjanetCredentialsRepository
    {
        public UrjanetCredentialsRepository(HoganvestContext context)
           : base(context)
        { }
    }
}
