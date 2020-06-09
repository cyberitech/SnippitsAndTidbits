using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibExfil.Structures;

namespace LibExfil.DataSenders
{
    interface IBaseSenderInterface
    {
        int Send(DataRecordCollection data,String destination);

    }
}
