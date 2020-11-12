using System.Collections.Generic;

namespace Hoganvest.Core.Common
{
    public class Response
    {
        public bool IsSuccess { get; set; } = false;
        public List<string> Messages { get; set; } = new List<string>();
    }
}
