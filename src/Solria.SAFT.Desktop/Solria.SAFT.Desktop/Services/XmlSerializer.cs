using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Services
{
	public class XmlSerializer : IXmlSerializer
	{
		public async Task<T> Deserialize<T>(string xmlFileName)
		{
			TextReader tw = null;
			try
			{
				return await Task.Run(() =>
				{
					tw = new StreamReader(xmlFileName, CodePagesEncodingProvider.Instance.GetEncoding(1252));

					System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(T));
					T config = (T)x.Deserialize(tw);

					return config;
				});
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
