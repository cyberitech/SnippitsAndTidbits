using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibExfil.Structures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LibExfil.Toolbox.Base64;

namespace LibExfil.FileReaders
{


    class JsonFileReader: IBaseFileInterface
    {
        readonly String path = null;
        public JsonFileReader(String fPath)
        {
            if (!File.Exists(fPath))//This will cause an exception to be thrown if the file does not exist.  We are testing or failing based upon a valid file existing.
                throw new FileNotFoundException();
            this.path = fPath;

        }
        public DataRecordCollection GetFileContents(bool b64encode = false)
        {
            DataRecordCollection result = null;
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject obj1 = (JObject)JToken.ReadFrom(reader);
                foreach (JObject obj in obj1.Children<JObject>())
                {
                    Dictionary<String, String> dictObj = obj1.ToObject<Dictionary<string, string>>();
                    List<String> schema = dictObj.Keys.Cast<String>().ToList();
                    List<String> values = dictObj.Values.Cast<String>().ToList();
                    if (b64encode)
                        values = Base64Utils.MakeB64(values);
                    if (result == null)
                        result = new DataRecordCollection(schema.ToArray(), values.ToArray());
                    else
                        result.AddRecord(values.ToArray());
                }
            }
            return result;

        }
    }
}
