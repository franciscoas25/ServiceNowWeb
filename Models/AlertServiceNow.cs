using System.Security.Principal;

namespace ServiceNowWeb.Models
{
    public class AlertServiceNow
    {
        public List<Alert> result { get; set; }

    }

    public class Alert : AlertResult
    {
        //public string? number { get; set; }
        //public string? node { get; set; }
        //public string? short_description { get; set; }
        //public string? sys_created_on { get; set; }
        //public string? ip_address { get; set; }
        //public string? name { get; set; }
        public Cmdb_ci? cmdb_ci { get; set; }
    }

    public class Cmdb_ci
    {
        public string? link { get; set; }
    }

    public class AlertResult
    {
        public string? number { get; set; }
        public string? node { get; set; }
        public string? short_description { get; set; }
        public string? sys_created_on { get; set; }
        public string? ip_address { get; set; }
        public string? name { get; set; }
    }
}
