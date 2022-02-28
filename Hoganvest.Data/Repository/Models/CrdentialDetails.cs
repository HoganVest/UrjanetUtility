using System;
using System.Collections.Generic;

namespace Hoganvest.Data.Repository.Models
{
    public partial class CrdentialDetails
    {
        public int CreentialDetailsId { get; set; }
        public int? CrdentialId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountStatus { get; set; }
        public string PropertyId { get; set; }

        public virtual Credential Crdential { get; set; }
    }
}
