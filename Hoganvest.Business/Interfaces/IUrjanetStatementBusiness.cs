using Hoganvest.Core.Common;
using System.Threading.Tasks;

namespace Hoganvest.Business.Interfaces
{
    public interface IUrjanetStatementBusiness
    {
        ValueTask<Response> AddStatement(string token, string[] args);
        ValueTask<string> getToken();
    }
}
