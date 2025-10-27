using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SolRIA.SAFT.Parser;

/// <summary>
/// Old parser, loads the file in memory
/// </summary>
public class SaftXmlParser
{

    public static T DeserializeXml<T>(string xmlFileName, Encoding encoding)
    {
        if (File.Exists(xmlFileName) == false)
            return default;

        StreamReader tw = null;
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            tw = new StreamReader(xmlFileName, encoding);

            var x = new XmlSerializer(typeof(T));
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

    public static async Task SerializeXml<T>(T content, string filename, XmlWriterSettings settings)
    {
        using var writer = XmlWriter.Create(filename, settings);
        var x = new XmlSerializer(typeof(T));
        x.Serialize(writer, content);
        await writer.FlushAsync();
    }
}
