﻿using System;

namespace Solria.SAFT.Desktop.Models
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

	public partial class SchemaResults { }

	public partial class HashResults { }
}
