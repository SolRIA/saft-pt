using System;
using System.IO;
using System.Text;

namespace SolRIA.SAFT.Parser.Services
{
    public class XmlParserService
    {
        public static T DeserializeXml<T>(string xmlFileName, Encoding encoding)
        {
            if (File.Exists(xmlFileName) == false)
                return default;

            TextReader tw = null;
            try
            {
                tw = new StreamReader(xmlFileName, encoding);

                var x = new System.Xml.Serialization.XmlSerializer(typeof(T));
                var config = (T)x.Deserialize(tw);

                return config;

            }
            catch (Exception)
            {
                return default;
            }
            finally
            {
                if (tw != null)
                {
                    tw.Close();
                    tw.Dispose();
                }
            }
        }
    }
}
