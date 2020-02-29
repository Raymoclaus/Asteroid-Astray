using System.IO;
using System.Linq;

public static class DirectoryInfoExtensions
{
	/// <summary>
	/// Creates a copy of a directory and all files under it recursively at target path.
	/// A name for the new folder can be specified, otherwise a unique name will be generated.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="target"></param>
	/// <param name="copyName"></param>
	/// <returns>Returns the newly created DirectoryInfo. Returns null if none created</returns>
	public static DirectoryInfo CopyToLocation(this DirectoryInfo source, DirectoryInfo target, string copyName = null)
	{
		if (source.FullName.ToLower() == target.FullName.ToLower())
		{
			return null;
		}

		copyName = string.IsNullOrEmpty(copyName) ? source.Name : copyName;
		string newFolderPath = $"{target.FullName}/{copyName}";

		// Check if the target directory exists, if not, create it.
		if (Directory.Exists(target.FullName))
		{
			if (Directory.Exists(newFolderPath))
			{
				copyName = target.EnsureNameUniqueness(copyName);
				newFolderPath = $"{target.FullName}/{copyName}";
			}
		}
		else
		{
			Directory.CreateDirectory(target.FullName);
		}

		target = Directory.CreateDirectory(newFolderPath);

		// Copy each file into it's new directory.
		foreach (FileInfo fi in source.GetFiles())
		{
			fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
		}

		// Copy each subdirectory using recursion.
		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
		{
			DirectoryInfo nextTargetSubDir =
				target.CreateSubdirectory(diSourceSubDir.Name);
			CopyToLocation(diSourceSubDir, nextTargetSubDir);
		}

		return target;
	}

	private const string COPY_SUFFIX = " - Copy";

	public static string EnsureNameUniqueness(this DirectoryInfo source, string name)
	{
		int startOfSuffix = name.IndexOf(COPY_SUFFIX);
		if (startOfSuffix >= 0)
		{
			name = name.Substring(0, startOfSuffix);
		}
		string original = name;

		int count = 1;
		while (Directory.Exists($"{source.FullName}/{name}"))
		{
			if (count == 1)
			{
				name = $"{original}{COPY_SUFFIX}";
			}
			else
			{
				name = $"{original}{COPY_SUFFIX} ({count})";
			}

			count++;
		}

		return name;
	}
}
