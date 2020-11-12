using System;

namespace Hoganvest.Model.Responses
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<StatementResponse>(myJsonResponse); 
    public class Download
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Download download { get; set; }
    }

    public class StatementResponse
    {
        public DateTime createdDate { get; set; }
        public Links _links { get; set; }
    }


}
