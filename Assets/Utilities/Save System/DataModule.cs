using System;
using System.Reflection;

namespace SaveSystem
{
	public struct DataModule
	{
		private const char separator = '|';
		private const string formattedEntry = "{1}{0}{2}";
		public string parameterName;
		public string data;

		public DataModule(string name, string data)
		{
			parameterName = name;
			this.data = data;
		}

		public DataModule(FieldInfo field, object fieldSupportObject)
			: this(field.Name, field.GetValue(fieldSupportObject)?.ToString())
		{

		}

		public DataModule(string name, int data)
			: this(name, data.ToString())
		{

		}

		public DataModule(string name, float data)
			: this(name, data.ToString())
		{

		}

		public DataModule(string name, bool data)
			: this(name, data.ToString())
		{

		}

		public DataModule(string name, char data)
			: this(name, data.ToString())
		{

		}

		public DataModule(string name, short data)
			: this(name, data.ToString())
		{

		}

		public DataModule(string name, double data)
			: this(name, data.ToString())
		{

		}

		public DataModule(string name, object data)
			: this(name, data.ToString())
		{

		}

		public override string ToString()
			=> string.Format(formattedEntry,
				separator,
				parameterName,
				data);
	}
}
