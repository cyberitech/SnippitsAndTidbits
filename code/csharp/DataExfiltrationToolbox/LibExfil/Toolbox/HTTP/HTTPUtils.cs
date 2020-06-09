using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibExfil.Toolbox.HTTP
{
    public class HTTPUtils
    {

        public static bool ValidateURL(String url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return true;
            return false;
        }


        public static String GetDomainFromURL(String host)
        {
            if (host.ToLower().Contains("http") || host.Contains("/") || host.Contains(":"))
            {
                Uri uri = new Uri(host);
                host = uri.DnsSafeHost;
                return GetDomainFromHost(host);
            }
            return host;
        }

        public static String GetDomainFromHost(String host)
        {
            if (host.Count(c => (c == '.')) > 1) //we want to make sure we only have the domain name
                host = String.Join(".", host.Split('.').Reverse().Take(2).Reverse().ToArray());
            return host;
        }

        public static String GetHostFromURL(String host)
        {
            if (host.ToLower().Contains("http") || host.Contains("/") || host.Contains(":"))
            {
                Uri uri = new Uri(host);
                return uri.Host;
            }
            else
                return host;
        }

    }
}
