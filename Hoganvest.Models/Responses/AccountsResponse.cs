using System;
using System.Collections.Generic;

namespace Hoganvest.Model.Responses
{

    public class Embedded2
    {
        public CustomData customData { get; set; }
        public List<Account> accounts { get; set; }
    }
    public class Statements
    {
        public string href { get; set; }
    }

    public class AccountLinks
    {
        public Self self { get; set; }
        public Provider provider { get; set; }
        public Credentials credentials { get; set; }
        public Events events { get; set; }
        public CustomData customData { get; set; }
        public Statements statements { get; set; }
        public Download download { get; set; }
    }

    public class Account
    {
        public Embedded _embedded { get; set; }
        public string accountNumber { get; set; }
        public string normalizedAccountNumber { get; set; }
        public string providerName { get; set; }
        public string status { get; set; }
        public string statusDetail { get; set; }
        public bool enabled { get; set; }
        public bool prepaid { get; set; }
        public string type { get; set; }
        public DateTime lastModified { get; set; }
        public DateTime created { get; set; }
        public string createdBy { get; set; }
        public string lastModifiedBy { get; set; }
        public DateTime latestNewStatement { get; set; }
        public string latestStatementDate { get; set; }
        public int numberOfStatements { get; set; }
        public bool mock { get; set; }
        public AccountLinks _links { get; set; }
    }
    public class Embedded3
    {
        public CustomData customData { get; set; }
        public List<Account> accounts { get; set; }
    }
    public class AccountsResponse
    {
        public Embedded3 _embedded { get; set; }
        public AccountLinks _links { get; set; }
        public Page page { get; set; }
    }

}
