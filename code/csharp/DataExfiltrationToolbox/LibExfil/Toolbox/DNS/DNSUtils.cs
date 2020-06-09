using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using ARSoft.Tools.Net;

namespace LibExfil.Toolbox.DNS
{
    public class DNSUtils
    {
        /// <summary>
        ///  makes any string that may not fit the requirements for a label become compliant
        ///  There was some RFC that describes this.  try googling for it because I dont remember its number as of the time im documenting this
        ///  anyways, if the label input is jiggy baby then it wont be stomped in the nuts
        ///  If the label is not dns commpliant it will give it the smack down and do what is needed (truncate, char replacement) to make it compliant
        ///  does not support extended DNS labeling, sorry euro-jerks and those who speaks moon runes
        /// </summary>
        /// <param name="input">a label (the thing between dots in a host name, muh famalam) to transform</param>
        /// <returns>return is a string of a valid label</returns>

        public String MakeLabelDNSCompliant(String input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9\\.-]", RegexOptions.Compiled | RegexOptions.Singleline);//pattern for any character that is not a valid dns character.  this only examines individual labels, thus a '.' char is not valid.
            String output = regex.Replace(input, "-");
            output = Regex.Replace(output, "-+", "-");
            if (!char.IsLetter(output[0]))
                output = "value-" + output;  //the first character in a label must be an alpha char
            return (output.Length >= 63) ? output.Substring(0, 63) : output;     //truncate label to 63 characters if it is too long

        }
        /// <summary>
        ///  makes any string that may not fit the requirements for a label become compliant
        ///  There was some RFC that describes this.  try googling for it because I dont remember its number as of the time im documenting this
        ///  anyways, if the label input is jiggy baby then it wont be stomped in the nuts
        ///  If the label is not dns commpliant it will give it the smack down and do what is needed (truncate, char replacement) to make it compliant
        ///  does not support extended DNS labeling, sorry euro-jerks and those who speaks moon runes
        /// </summary>
        /// <param name="input">a label (the thing between dots in a host name, muh famalam) to transform</param>
        /// <returns>return is a string arrays of a valid label</returns>

        public String[] MakeLabelsDNSCompliant(String[] input)
        { return input.Select(s => MakeLabelDNSCompliant(s)).ToArray(); }
        /// <summary>
        ///  makes any string that may not fit the requirements for a label become compliant
        ///  There was some RFC that describes this.  try googling for it because I dont remember its number as of the time im documenting this
        ///  anyways, if the label input is jiggy baby then it wont be stomped in the nuts
        ///  If the label is not dns commpliant it will give it the smack down and do what is needed (truncate, char replacement) to make it compliant
        ///  does not support extended DNS labeling, sorry euro-jerks and those who speaks moon runes
        /// </summary>
        /// <param name="input">a n array of labels (the thing between dots in a host name, muh famalam) to transform</param>
        /// <returns>return is a list of a valid string label</returns>

        public List<String> MakeLabelsDNSCompliant(List<String> input)
        { return input.Select(s => MakeLabelDNSCompliant(s)).ToList(); }

        /// <summary>
        /// Extracts a host name from a domain name
        /// </summary>
        /// <param name="host">the host name</param>
        /// <returns>returns a domain name.  only top two labels are included.  sorry to the [domain].co.uk weirdos. your being screwed here.</returns>
        public  String GetDomainFromHost(String host)
        {
            if (host.Count(c => (c == '.')) > 1) //we want to make sure we only have the domain name
                host = String.Join(".", host.Split('.').Reverse().Take(2).Reverse().ToArray());
            return host;
        }

        public IEnumerable<NsRecord> GetAllNSHostNamesForDomain(string domain)
        {
            var resp = DnsClient.Default.Resolve(new DomainName(domain.Split('.')), RecordType.Ns);
            var nsrecords = resp.AnswerRecords.OfType<NsRecord>();
            return (nsrecords != null && nsrecords.Count() > 0)
                ? nsrecords
                : null;
        }
        public string GetFirstNSHostNameForDomain(string domain)
        {
            var resp = DnsClient.Default.Resolve(new DomainName(domain.Split('.')), RecordType.Ns);
            var nsrecords = resp.AnswerRecords.OfType<NsRecord>();
            return (nsrecords!= null && nsrecords.Count() > 0) 
                ? nsrecords.First().NameServer.ToString()
                : null;
        }
        public string GetNSIPAddressForDomain(string domain)
        {
            var nsrecords = GetFirstNSHostNameForDomain(domain);
            return (nsrecords != null && nsrecords.Count() > 0)
                ? Dns.GetHostEntry(nsrecords).ToString()
                : null;
            
        }

        public string GetIPFromName(string name)
        {
            return Dns.GetHostEntry(name).ToString();
        }






        /// <summary>
        /// This function is very simple.  It is more or less a wrapper around SendDensQuery() that will decide whether to use the hosts resolv chain or specify a server.
        /// The term 'Generic' in the name refers to the fact that it does nothing out of the ordinary such as attempting to directly query a specific name resovler
        /// </summary>
        /// <param name="query"></param>
        public async void SendDnsQuery(String query,string custom_resolver, bool synchronous_execution=false)
        {
            try
            {
                switch (custom_resolver)
                {
                    case null: await Dns.GetHostAddressesAsync(query).ConfigureAwait(synchronous_execution); break;
                    default: await DnsQueryCustomResolver(query, custom_resolver).ConfigureAwait(synchronous_execution); break;
                }
            }
            catch (ArgumentOutOfRangeException e1) { Console.WriteLine(e1.Message); }
            catch (SocketException e2) { Console.WriteLine(e2.Message); }
        }



        /*shameless copypasta from https://stackoverflow.com/questions/1315758/specify-which-dns-servers-to-use-to-resolve-hostnames-in-net */
        public async Task<IPHostEntry> DnsQueryCustomResolver(string host, string dns)
        {
            if (string.IsNullOrEmpty(host))
            {
                return null;
            }


            //Check dns server's address or port
            IPHostEntry result = null;
            int dnsPort;
            if (dns != null)
            {
                string[] blocks = dns.Split(':');
                if (blocks.Length == 2 && int.TryParse(blocks[1], out dnsPort))//dns is ip v4
                {
                    dns = blocks[0];
                }
                else if (blocks.Length == 9 && int.TryParse(blocks[8], out dnsPort))//dns is ip v6
                {
                    blocks[0] = blocks[0].TrimStart('[');
                    blocks[7] = blocks[7].TrimStart(']');
                    dns = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}", blocks);
                }
                else
                {
                    dnsPort = 53;
                }
            }
            else
            {
                dnsPort = 53;
            }

            //Check if host is ip address
            if (host[0] == '[' && host[host.Length - 1] == ']')//IPV6 address
            {
                host = host.Substring(1, host.Length - 2);
            }
            if (IPAddress.TryParse(host, out IPAddress address))
            {
                result = new IPHostEntry { AddressList = new IPAddress[] { address } };
            }
            else if (string.IsNullOrEmpty(dns))
            {
                result = await Dns.GetHostEntryAsync(host);
            }
            else
            {
                #region Resolve with customized dns server
                IPAddress dnsAddr;
                if (!IPAddress.TryParse(dns, out dnsAddr))
                {
                    throw new ArgumentException("The dns host must be ip address.", nameof(dns));
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    Random rnd = new Random();
                    //About the dns message:http://www.ietf.org/rfc/rfc1035.txt

                    //Write message header.
                    ms.Write(new byte[] {
                    (byte)rnd.Next(0, 0xFF),(byte)rnd.Next(0, 0xFF),
                    0x01,
                    0x00,
                    0x00,0x01,
                    0x00,0x00,
                    0x00,0x00,
                    0x00,0x00
                }, 0, 12);

                    //Write the host to query.
                    foreach (string block in host.Split('.'))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(block);
                        ms.WriteByte((byte)data.Length);
                        ms.Write(data, 0, data.Length);
                    }
                    ms.WriteByte(0);//The end of query, muest 0(null string)

                    //Query type:A
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x01);

                    //Query class:IN
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x01);

                    Socket socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
                    try
                    {
                        //send to dns server
                        byte[] buffer = ms.ToArray();
                        while (socket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, new IPEndPoint(dnsAddr, dnsPort)) < buffer.Length) ;
                        buffer = new byte[0x100];
                        EndPoint ep = socket.LocalEndPoint;
                        int num = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref ep);

                        //The response message has the same header and question structure, so we move index to the answer part directly.
                        int index = (int)ms.Length;
                        //Parse response records.
                        void SkipName()
                        {
                            while (index < num)
                            {
                                int length = buffer[index++];
                                if (length == 0)
                                {
                                    return;
                                }
                                else if (length > 191)
                                {
                                    return;
                                }
                                index += length;
                            }
                        }

                        List<IPAddress> addresses = new List<IPAddress>();
                        while (index < num)
                        {
                            SkipName();//Seems the name of record is useless in this scense, so we just needs to get the next index after name.
                            byte type = buffer[index += 2];
                            index += 7;//Skip class and ttl

                            int length = buffer[index++] << 8 | buffer[index++];//Get record data's length

                            if (type == 0x01)//A record
                            {
                                if (length == 4)//Parse record data to ip v4, this is what we need.
                                {
                                    addresses.Add(new IPAddress(new byte[] { buffer[index], buffer[index + 1], buffer[index + 2], buffer[index + 3] }));
                                }
                            }
                            index += length;
                        }
                        result = new IPHostEntry { AddressList = addresses.ToArray() };
                    }
                    finally
                    {
                        socket.Dispose();
                    }
                }
                #endregion
            }



            return result;

        }

    }
}
