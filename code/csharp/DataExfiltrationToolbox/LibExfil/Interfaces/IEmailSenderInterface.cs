using LibExfil.DataSenders;
using LibExfil.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibExfil.Interfaces
{
    interface IEmailSenderInterface : IBaseSenderInterface
    {

        int SendAllRecords(DataRecordCollection data, string destination);
        int SendDecrementalEmails(DataRecordCollection data, string destination);
    }
}
