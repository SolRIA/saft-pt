using System;

namespace Solria.SAFT.Desktop.Models
{
	public class BaseData
    {
		string pk;
		/// <summary>
		/// Primary key of the registry
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public string Pk
		{
			get
			{
				if (string.IsNullOrEmpty(pk))
					pk = Guid.NewGuid().ToString();
				return pk;
			}
			private set { pk = value; }
		}
	}

	public partial class SchemaResults { }

	public partial class HashResults { }
}
