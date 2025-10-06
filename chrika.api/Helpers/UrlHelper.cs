// Helpers/UrlHelper.cs
using System.Net;
using System.Net.Sockets;

namespace Chrika.Api.Helpers
{
    public static class UrlHelper
    {
        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "localhost"; // Fallback
        }
    }
}
