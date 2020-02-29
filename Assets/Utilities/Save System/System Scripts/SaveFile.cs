using System.IO;

namespace SaveSystem
{
	public class SaveFile
	{
		public DirectoryInfo dirInfo;

		public SaveFile(DirectoryInfo dirInfo)
		{
			this.dirInfo = dirInfo;
		}

		public string Name => dirInfo.Name;
	} 
}
