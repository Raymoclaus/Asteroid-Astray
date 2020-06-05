using GenericExtensions;
using System;
using System.Text;

public static class DeliveryOrderDetailsGenerator
{
	private const char SEPARATOR = ',';

	public static string Generate(int numberOfItems, int numberOfUniqueItems, string[] orderList)
	{
		StringBuilder builder = new StringBuilder();
		//append numbers
		builder.Append($"{numberOfItems}{SEPARATOR}{numberOfUniqueItems}{SEPARATOR}");
		//loop through order list and append each entry
		for (int i = 0; i < orderList.Length; i++)
		{
			builder.Append(orderList[i]);
			if (i < orderList.Length - 1)
			{
				builder.Append(SEPARATOR);
			}
		}

		return builder.ToString();
	}

	public static bool TryParse(string orderDetails, out int numberOfItems, out int numberOfUniqueItems,
		out string[] orderList)
	{
		string[] detailsArr = orderDetails.Split(SEPARATOR);
		try
		{
			numberOfItems = int.Parse(detailsArr[1]);
			numberOfUniqueItems = int.Parse(detailsArr[2]);
			orderList = detailsArr.SubArray(3, detailsArr.Length - 1);
			return true;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			numberOfItems = default;
			numberOfUniqueItems = default;
			orderList = default;
			return false;
		}
	}
}
