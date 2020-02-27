using System.Collections.Generic;

namespace SaveSystem
{
	public interface ISaveable
	{
		bool ShouldSave { get; }
		string GetTag();
		List<DataModule> GetData();
	} 
}
