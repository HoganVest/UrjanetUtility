using System;
using System.Collections.Generic;

namespace Hoganvest.Model.Responses
{
    public class UrjanetCredentialsResponse
    {
        public Embedded _embedded { get; set; }
        public Links1 _links { get; set; }
        public Page page { get; set; }
    }
    public class CustomData
    {
        public string PropertyID { get; set; }
        public string href { get; set; }
    }

    public class Embedded
    {
        public CustomData customData { get; set; }
        public List<Credentials> credentials { get; set; }
    }

    public class Provider
    {
        public string href { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Accounts
    {
        public string href { get; set; }
    }

    public class Passwords
    {
        public string href { get; set; }
    }

    public class Events
    {
        public string href { get; set; }
    }

    public class Consent
    {
        public string href { get; set; }
    }

    public class UpdateUrl
    {
        public string href { get; set; }
    }

    public class Download1
    {
        public string href { get; set; }
    }

    public class Links1
    {
        public Provider provider { get; set; }
        public Self self { get; set; }
        public Accounts accounts { get; set; }
        public Passwords passwords { get; set; }
        public Events events { get; set; }
        public CustomData customData { get; set; }
        public Consent consent { get; set; }
        public UpdateUrl updateUrl { get; set; }
        public Download1 download { get; set; }
        public First first { get; set; }
        public Next next { get; set; }
        public Last last { get; set; }
    }

    public class Credentials
    {
        public Embedded _embedded { get; set; }
        public string username { get; set; }
        public string correlationId { get; set; }
        public string status { get; set; }
        public string statusDetail { get; set; }
        public bool enabled { get; set; }
        public string providerName { get; set; }
        public DateTime lastModified { get; set; }
        public DateTime created { get; set; }
        public string createdBy { get; set; }
        public string lastModifiedBy { get; set; }
        public bool runHistory { get; set; }
        public bool mock { get; set; }
        public Links1 _links { get; set; }
    }

    public class First
    {
        public string href { get; set; }
    }

    public class Next
    {
        public string href { get; set; }
    }

    public class Last
    {
        public string href { get; set; }
    }

    public class Page
    {
        public int size { get; set; }
        public int totalElements { get; set; }
        public int totalPages { get; set; }
        public int number { get; set; }
    }

    public class Root
    {
        
    }

}
