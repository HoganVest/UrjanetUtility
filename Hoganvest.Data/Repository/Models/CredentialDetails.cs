using System;
using System.Collections.Generic;

namespace Hoganvest.Data.Repository.Models
{
    public partial class CredentialDetails
    {
        public int CredentialDetailsId { get; set; }
        public int? CredentialId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountStatus { get; set; }
        public string PropertyId { get; set; }

        public virtual Credential Credential { get; set; }
    }
}
