using System.Security.Principal;

namespace ServiceNowWeb.Models
{
    public class LocationServiceNow
    {
        public LocationResult result { get; set; }
    }

    public class LocationResult
    {
        public string? name { get; set; }
    }
}
