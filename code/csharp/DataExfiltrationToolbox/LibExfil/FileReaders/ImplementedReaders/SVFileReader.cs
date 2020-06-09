using LibExfil.Structures;
using LibExfil.Toolbox.Base64;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibExfil.FileReaders
{
    class SVFileReader: IBaseFileInterface
    {
        readonly String path = null;
        readonly char delim;


        public SVFileReader(String fPath, char delimiter)
        {

            if (!File.Exists(fPath))//This will cause an exception to be thrown if the file does not exist.  We are testing or failing based upon a valid file existing.
                throw new FileNotFoundException();
            path = fPath;
            delim = delimiter;
        }
            

        public DataRecordCollection GetFileContents(bool b64encode)
        {
            DataRecordCollection result = null;
            var csvLines = File.ReadLines(path);
            List<String> schema = new List<String>();
            //---Loop through each line in target file and send POST request to server---
            foreach (String line in csvLines)
            {
                if (schema.Count() == 0)//if we dont yet have the schema, assume that the first line of the SV file contains the schema header.  If it doesnt, well... that sucks
                {
                    schema = line.Split(delim).ToList();
                    continue;
                }
                string[] values = (b64encode==true) ? Base64Utils.MakeB64(line.Split(delim)):line.Split(delim);
                if (values.Length > 0)    //Ensure that we are returning data, and not the empty final line from the file
                {

                    if (result == null)
                        result = new DataRecordCollection(schema.ToArray());
                    else
                        result.AddRecord(values.ToArray());
                }
            }
            return result;
        }
    }
}
