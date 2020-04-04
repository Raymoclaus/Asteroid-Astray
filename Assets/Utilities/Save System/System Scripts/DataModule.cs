using System;
using System.Collections.Generic;
using System.Reflection;

namespace SaveSystem
{
	public struct DataModule
	{
		private const char separator = '|';
		private const string formattedEntry = "{1}{0}{2}{0}{3}";
		public string parameterName, parameterType, data;

		public static DataModule INVALID_DATA_MODULE = new DataModule(null, null, null);

		public DataModule(string pName, string pType, string data)
		{
			parameterName = pName;
			parameterType = pType;
			this.data = data;
		}

		public DataModule(FieldInfo field, object fieldSupportObject)
			: this(field.Name, field.FieldType.ToString(), field.GetValue(fieldSupportObject)?.ToString())
		{

		}

		public DataModule(string pName, int data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public DataModule(string pName, float data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public DataModule(string pName, bool data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public DataModule(string pName, char data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public DataModule(string pName, short data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public DataModule(string pName, double data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public DataModule(string pName, object data)
			: this(pName, data.GetType().ToString(), data.ToString())
		{

		}

		public override string ToString()
			=> string.Format(formattedEntry,
				separator,
				parameterName,
				parameterType,
				data);
	}
}
