using System;
using System.Text;

public static class RandomAsciiGenerator
{
	//ascii codes for various characters
	private static readonly int[] NUMBER_CODES = new int[]
	{
		48, 49, 50, 51, 52, 53, 54, 55, 56, 57
	};
	private static readonly int[] LOWERCASE_LETTER_CODES = new int[]
	{
		97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112,
		113, 114, 115, 16, 117, 118, 119, 120, 121, 122
	};
	private static readonly int[] UPPERCASE_LETTER_CODES = new int[]
	{
		65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83,
		84, 85, 86, 87, 88, 89, 90
	};
	private static readonly int[] SYMBOL_CODES = new int[]
	{
		32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 58, 59, 60,
		61, 62, 63, 64, 91, 92, 93, 94, 95, 96, 123, 124, 125, 126
	};
	private static readonly int[] ESCAPE_CHARACTER_CODES = new int[]
	{
		9, 10, 11, 12, 13
	};
	private static readonly int[] CONTROL_CODES = new int[]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26,
		27, 28, 29, 30, 31, 127
	};

	private const char DEFAULT_CHAR = '?';

	private static Random random = new Random(DateTime.Now.Millisecond);

	public static string GetRandomString(int length, bool includeNumbers, bool includeLowercase, bool includeUppercase,
		bool includeSymbols, bool includeEscapeCharacters, bool includeControls, Random randomiser = null)
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < length; i++)
		{
			char randomChar = GetRandomChar(includeNumbers, includeLowercase, includeUppercase, includeSymbols,
				includeEscapeCharacters, includeControls, randomiser);
			builder.Append(randomChar);
		}

		return builder.ToString();
	}

	public static char GetRandomChar(bool includeNumbers, bool includeLowercase, bool includeUppercase,
		bool includeSymbols, bool includeEscapeCharacters, bool includeControls, Random randomiser = null)
	{
		int count = 0;

		if (includeNumbers) count += NUMBER_CODES.Length;
		if (includeLowercase) count += LOWERCASE_LETTER_CODES.Length;
		if (includeUppercase) count += UPPERCASE_LETTER_CODES.Length;
		if (includeSymbols) count += SYMBOL_CODES.Length;
		if (includeEscapeCharacters) count += ESCAPE_CHARACTER_CODES.Length;
		if (includeControls) count += CONTROL_CODES.Length;

		if (count == 0) return DEFAULT_CHAR;

		if (randomiser == null) randomiser = random;

		int randomVal = randomiser.Next(0, count);

		if (includeNumbers)
		{
			if (randomVal < NUMBER_CODES.Length)
			{
				return (char)NUMBER_CODES[randomVal];
			}
			else
			{
				randomVal -= NUMBER_CODES.Length;
			}
		}

		if (includeLowercase)
		{
			if (randomVal < LOWERCASE_LETTER_CODES.Length)
			{
				return (char)LOWERCASE_LETTER_CODES[randomVal];
			}
			else
			{
				randomVal -= LOWERCASE_LETTER_CODES.Length;
			}
		}

		if (includeUppercase)
		{
			if (randomVal < UPPERCASE_LETTER_CODES.Length)
			{
				return (char)UPPERCASE_LETTER_CODES[randomVal];
			}
			else
			{
				randomVal -= UPPERCASE_LETTER_CODES.Length;
			}
		}

		if (includeSymbols)
		{
			if (randomVal < SYMBOL_CODES.Length)
			{
				return (char)SYMBOL_CODES[randomVal];
			}
			else
			{
				randomVal -= SYMBOL_CODES.Length;
			}
		}

		if (includeEscapeCharacters)
		{
			if (randomVal < ESCAPE_CHARACTER_CODES.Length)
			{
				return (char)ESCAPE_CHARACTER_CODES[randomVal];
			}
			else
			{
				randomVal -= ESCAPE_CHARACTER_CODES.Length;
			}
		}

		if (includeControls)
		{
			if (randomVal < CONTROL_CODES.Length)
			{
				return (char)CONTROL_CODES[randomVal];
			}
			else
			{
				randomVal -= CONTROL_CODES.Length;
			}
		}

		return DEFAULT_CHAR;
	}
}
