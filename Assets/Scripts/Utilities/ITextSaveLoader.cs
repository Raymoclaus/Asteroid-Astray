public interface ITextSaveLoader
{
	void PrepareForSaving();
	void FinishedLoading();
	ITextSaveLoader[] GetObjectsToSave();
	string Tag { get; }
	string EndTag { get; }
	string GetSaveText(int indentLevel);
	void Load(string[] text);
}
