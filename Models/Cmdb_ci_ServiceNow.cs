namespace ServiceNowWeb.Models
{
    public class Cmdb_ci_ServiceNow
    {
        public Cmdb_ci_Result result { get; set; }
    }

    public class Cmdb_ci_Result
    {
        public Location location { get; set; }
        public string ip_address { get; set; }
    }

    public class Location
    {
        public string? link { get; set; }
    }
}
