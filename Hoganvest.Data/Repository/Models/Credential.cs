using System;
using System.Collections.Generic;

namespace Hoganvest.Data.Repository.Models
{
    public partial class Credential
    {
        public Credential()
        {
            CrdentialDetails = new HashSet<CrdentialDetails>();
        }

        public int CrdentialId { get; set; }
        public string UserName { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
        public bool? Enabled { get; set; }
        public string ProviderName { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public bool? RunHistory { get; set; }
        public bool? Mock { get; set; }
        public string Password { get; set; }
        public string Website { get; set; }

        public virtual ICollection<CrdentialDetails> CrdentialDetails { get; set; }
    }
}
