using System;
using System.Collections.Generic;

using LibExfil.Structures;



///
namespace LibExfil.FileReaders
{

    interface IBaseFileInterface
    {
        DataRecordCollection GetFileContents(bool b64encode = false);
    }
}
