using System;
using UnityEngine;

namespace GenericExtensions
{
	public static class Vector3Extensions
	{
		public static bool TryParse(string input, out Vector3 value)
		{
			value = new Vector3();
			try
			{
				input = input.Replace("(", string.Empty)
					.Replace(")", string.Empty)
					.Replace(" ", string.Empty);
				string[] parts = input.Split(',');
				value.x = float.Parse(parts[0]);
				value.y = float.Parse(parts[1]);
				value.z = float.Parse(parts[2]);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}
	} 
}
