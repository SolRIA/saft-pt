using System;

namespace Solria.SAFT.Parser.Models
{
    public class BaseData
	{
		public BaseData()
		{
			Pk = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Unique id
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public string Pk { get; }
	}
}
