using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SaveSystem
{
	public static class UnifiedSaveLoad
	{
		private const string SAVE_FILENAME = "Unified Save";
		private const string TAG_FORMAT = "{2}[{0}]|{1}";
		private const char SEPARATOR = '|';
		private static Dictionary<string, List<string>> openedFiles = new Dictionary<string, List<string>>();

		/// <summary>
		/// Iterates over all opened files, saves them to text files and closes them.
		/// </summary>
		private static void SaveAllOpenedFiles()
		{
			string[] keys = openedFiles.Keys.ToArray();

			foreach (string key in keys)
			{
				SaveOpenedFile(key);
			}
		}

		/// <summary>
		/// Saves opened lines to a text file and then closes the file.
		/// </summary>
		/// <param name="filename"></param>
		public static void SaveOpenedFile(string filename)
		{
			if (!openedFiles.ContainsKey(filename)) return;

			SaveLoad.SaveText(filename, openedFiles[filename].ToArray());
			CloseFile(filename);
		}

		/// <summary>
		/// Saves the Unified Save file only.
		/// </summary>
		public static void SaveUnifiedSaveFile()
		{
			SaveOpenedFile(SAVE_FILENAME);
		}

		/// <summary>
		/// Removes a filename from dictionary of opened files.
		/// </summary>
		/// <param name="filename"></param>
		public static void CloseFile(string filename) => openedFiles.Remove(filename);

		/// <summary>
		/// Saves data to a file named after given key
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		/// <param name="data"></param>
		public static void UpdateOpenedFile(string filename, SaveTag tag, DataModule data)
		{
			//check if tag already exists
			int tagLine = TagLine(filename, tag);

			//no error found, adding data to tag in file
			if (tagLine >= 0)
			{
				AddData(filename, tag, data);
				return;
			}

			//file not found
			if (tagLine == -1)
			{
				Debug.Log($"Error: File with name {filename} not found.");
				return;
			}

			//no pre-existing tag found, create a new one in file
			if (tagLine == -2)
			{
				InsertTag(filename, tag);
				AddData(filename, tag, data);
				return;
			}

			if (tagLine == -3)
			{
				Debug.Log($"Error: Tag is not valid.");
				return;
			}

			if (tagLine == -4)
			{
				Debug.Log($"Error: File not formatted correctly.");
				return;
			}
		}

		/// <summary>
		/// Adds a tag to the file.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		private static void InsertTag(string filename, SaveTag tag)
		{
			//get the lines of the file
			List<string> lines = openedFiles[filename];
			//get the history of the tag
			List<SaveTag> tagHistory = tag.GetTagHistory();

			//iterate backwards through the tag and its parents to find one that might already exist
			SaveTag check = tag;
			int historyID = -1;
			while (check != null)
			{
				//get the line ID of where the tag exists
				int tagLine = TagLine(filename, check);
				//if tag exists, tagLine should be >= 0
				if (tagLine >= 0)
				{
					//if the tag found is the main one we're looking for, then there is no need to insert another one
					if (check == tag) return;

					//get the line of the tag
					string line = lines[tagLine];
					string[] parts = line.Split(SEPARATOR);
					if (parts.Length < 2) return;

					//check the line count under the tag
					bool parseLineCountSuccessful = int.TryParse(parts[1], out int lineCount);
					if (!parseLineCountSuccessful) return;
					
					//store the history ID of the tag
					historyID = tagHistory.IndexOf(check);
					break;
				}
				//if tag doesn't exist, check to see if a parent of the tag exists
				else if (tagLine == -2)
				{
					check = check.PriorTag;
				}
				//some other error occurred and file cannot be edited
				else
				{
					return;
				}
			}

			//if neither the tag nor its parents exist in the file
			if (check == null)
			{
				//get the base level parent tag in the tag's history
				historyID = tagHistory.Count - 1;
			}

			//iterate over tag history and add all the tags that don't exist in the file
			for (int i = historyID; i >= 0; i--)
			{
				//get the most base-level tag that doesn't exist in file
				SaveTag currentTag = tagHistory[i];

				//if tag already exists, go to next iteration
				if (TagExists(filename, currentTag)) continue;

				int linesBeingAdded = 1;
				//create a string for the tag
				string tagString = string.Format(TAG_FORMAT,
					currentTag.Tag,
					linesBeingAdded - 1,
					new string('\t', currentTag.IndentCount));

				//get the position in the file to add the tag
				int position = GetIndexOfEndOfTagContents(filename, currentTag.PriorTag);

				//if tag doesn't exist, get the position at the end of the file
				if (position == -1)
				{
					position = lines.Count;
				}

				//add the string to the end of the file
				lines.Insert(position, tagString);
				IncreaseLineCountUnderTag(filename, currentTag.PriorTag, linesBeingAdded);
			}
		}

		private static bool TagExists(string filename, SaveTag tag)
			=> TagLine(filename, tag) >= 0;

		/// <summary>
		/// Searches a file for specified tag and gets the position of the tag + the line count of the tag + 1.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		/// <returns>Returns end position of the tag's contents. -1 if tag does not exist.</returns>
		private static int GetIndexOfEndOfTagContents(string filename, SaveTag tag)
		{
			//check if tag exists
			int tagLine = TagLine(filename, tag);

			//if tag doesn't exist, exit with -1
			if (tagLine < 0) return -1;

			//get the tag's line from the file
			string line = openedFiles[filename][tagLine];
			string[] parts = line.Split(SEPARATOR);
			if (parts.Length < 2) return -1;

			//get the line count from the tag string
			bool parseLineCountSuccessful = int.TryParse(parts[1], out int lineCount);
			if (!parseLineCountSuccessful) return -1;

			//return the position of the tag + lineCount + 1, to get the position after the last line of contents
			return tagLine + lineCount + 1;
		}

		/// <summary>
		/// Locates the line number of the tag in the given filename and updates the lineCount integer
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		/// <param name="amountToAdd"></param>
		private static void IncreaseLineCountUnderTag(string filename, SaveTag tag, int amountToAdd)
		{
			//tag cannot be null
			if (tag == null) return;

			//ignore if there is nothing to add
			if (amountToAdd == 0) return;

			//attempt to open the file
			bool fileOpened = OpenFile(filename, false);
			//ignore if file could not be opened
			if (!fileOpened) return;

			//find the lineID of the tag
			int tagLine = TagLine(filename, tag);

			//ignore if tag not found
			if (tagLine < 0) return;

			//get the string at the tagLine
			string line = openedFiles[filename][tagLine];
			string[] parts = line.Split(SEPARATOR);

			//ignore if string doesn't meet validation criteria
			if (parts.Length < 2) return;

			//parse the lineCount integer under the tag
			bool parseLineCountSuccessful = int.TryParse(parts[1], out int lineCount);
			if (!parseLineCountSuccessful) return;

			//create string to overwrite the line
			parts[1] = (lineCount + amountToAdd).ToString();
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < parts.Length; i++)
			{
				sb.Append($"{(i != 0 ? "|" : string.Empty)}{parts[i]}");
			}

			//overwrite the line with the updated line number
			OverwriteLine(filename, tagLine, sb.ToString());

			//update parent of the tag too
			IncreaseLineCountUnderTag(filename, tag.PriorTag, amountToAdd);
		}

		/// <summary>
		/// Replaces a line in the file with the given text
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="lineID"></param>
		/// <param name="text"></param>
		private static void OverwriteLine(string filename, int lineID, string text)
		{
			//check to see if file is open
			bool fileOpened = OpenFile(filename, false);
			if (!fileOpened) return;

			//make sure lineID is valid for the file
			if (openedFiles[filename].Count <= lineID || lineID < 0) return;

			//replace line with given text
			openedFiles[filename][lineID] = text;
		}

		/// <summary>
		/// Adds data under tag. Doesn't work if tag doesn't exist.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		/// <param name="data"></param>
		private static void AddData(string filename, SaveTag tag, DataModule data)
		{
			//tag cannot be null
			if (tag == null) return;

			//attempt to open the file
			bool fileOpened = OpenFile(filename, false);
			//ignore if file could not be opened
			if (!fileOpened) return;

			//find the lineID of the tag
			int tagLine = TagLine(filename, tag);

			//ignore if tag not found
			if (tagLine < 0) return;

			//get the string at the tagLine
			string line = openedFiles[filename][tagLine];
			string[] parts = line.Split(SEPARATOR);

			//ignore if string doesn't meet validation criteria
			if (parts.Length < 2) return;

			//parse the lineCount integer under the tag
			bool parseLineCountSuccessful = int.TryParse(parts[1], out int lineCount);
			if (!parseLineCountSuccessful) return;

			//create string to insert into file
			string text = $"{new string('\t', tag.IndentCount + 1)}{data.ToString()}";

			//get the line index to insert text
			List<string> lines = openedFiles[filename];
			int index = GetIndexOfParameter(filename, tag, data.parameterName);
			if (index == -1)
			{
				index = GetIndexOfEndOfTagContents(filename, tag);
				lines.Insert(index, text);

				//update linecount of tag and parents
				IncreaseLineCountUnderTag(filename, tag, 1);
			}
			else
			{
				OverwriteLine(filename, index, text);
			}
		}

		/// <summary>
		/// Search a file for a parameter name under the given tag.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		/// <param name="parameterName"></param>
		/// <returns>Returns index of line containing parameterName. -1 if not found.</returns>
		private static int GetIndexOfParameter(string filename, SaveTag tag, string parameterName)
		{
			//tag cannot be null
			if (tag == null) return -1;

			//attempt to open the file
			bool fileOpened = OpenFile(filename, false);
			//ignore if file could not be opened
			if (!fileOpened) return -1;

			//find the lineID of the tag
			int tagLine = TagLine(filename, tag);

			//ignore if tag not found
			if (tagLine < 0) return -1;

			//get the end of tags contents
			int endOfTagContentsIndex = GetIndexOfEndOfTagContents(filename, tag);

			//get lines from file
			List<string> lines = openedFiles[filename];

			//iterate from the first parameter to the end of the contents
			for (int i = tagLine + 1; i < endOfTagContentsIndex; i++)
			{
				//check to see if i is a valid index for lines
				if (i < 0 || i >= lines.Count) continue;

				//get current line at i
				string line = lines[i];

				//if current line contains a tag, skip ahead based on line count of tag contents
				if (IsTag(line))
				{
					int count = GetLineCountFromTag(line);
					if (count == -1) return -1;

					i += count;
					continue;
				}

				//if current line contains a parameter, check to see if the name matches and then return current index
				if (IsParameter(line))
				{
					string otherParamName = GetParameterName(line);
					if (otherParamName == parameterName) return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Gets the parameter name from a line.
		/// </summary>
		/// <param name="line"></param>
		/// <returns>Returns null if not a valid parameter line.</returns>
		private static string GetParameterName(string line)
		{
			if (!IsParameter(line)) return null;
			
			string removeTabs = line.Replace("\t", string.Empty);
			string[] parts = removeTabs.Split(SEPARATOR);

			return parts[0];
		}

		/// <summary>
		/// Checks to see if a line is a valid parameter.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool IsParameter(string line)
		{
			if (IsTag(line)) return false;

			if (line.Length < 4) return false;

			if (line[0] != '\t') return false;

			string removeTabs = line.Replace("\t", string.Empty);
			string[] parts = removeTabs.Split(SEPARATOR);

			if (parts.Length < 2) return false;

			if (parts[0].Length == 0) return false;

			return true;
		}

		/// <summary>
		/// Parses the line count from a tag.
		/// </summary>
		/// <param name="line"></param>
		/// <returns>Returns an integer. -1 if given line is not a valid or integer could not be parsed.</returns>
		private static int GetLineCountFromTag(string line)
		{
			//check to see if line is a valid tag
			if (!IsTag(line)) return -1;

			//split the line into its different parts
			string[] parts = line.Split(SEPARATOR);
			if (parts.Length < 2) return -1;

			//parse the line count integer
			bool parseLineCountSuccessful = int.TryParse(parts[1], out int lineCount);
			if (!parseLineCountSuccessful) return -1;

			//return line count
			return lineCount;
		}

		/// <summary>
		/// Saves data to a universal file name
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="data"></param>
		public static void UpdateUnifiedSaveFile(SaveTag tag, DataModule data)
		{
			bool fileOpened = OpenFile(SAVE_FILENAME, true);
			UpdateOpenedFile(SAVE_FILENAME, tag, data);
		}

		/// <summary>
		/// Gets all fields in an object and updates the unified save file with each field individually.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="obj"></param>
		public static void UpdateUnifiedSaveFile(SaveTag tag, object obj)
		{
			FieldInfo[] fields = obj.GetType().GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				UpdateUnifiedSaveFile(tag, new DataModule(field, obj));
			}
		}

		/// <summary>
		/// Gets the line that the given tag appears on in given file name.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="tag"></param>
		/// <returns>Returns line number of tag. -1 if file not found. -2 if tag not found in file. -3 if tag is not valid. -4 if file contained errors.</returns>
		private static int TagLine(string filename, SaveTag tag)
		{
			//null check
			if (tag == null) return -3;

			//attempt to open the file
			bool fileOpened = OpenFile(filename, false);

			//file was not successfully opened
			if (!fileOpened) return -1;

			//get lines from file
			List<string> lines = openedFiles[filename];

			//get chain of tag and its parents
			List<SaveTag> tagHistory = tag.GetTagHistory();

			//get base tag
			SaveTag check = tagHistory.Last();
			tagHistory.RemoveAt(tagHistory.Count - 1);

			//iterate over lines in file
			for (int i = 0; i < lines.Count; i++)
			{
				string line = lines[i];
				string[] parts = line.Split(SEPARATOR);
				if (parts.Length < 1) return -4;

				//get tag at current line, after removing additional characters
				if (!IsTag(parts[0])) continue;
				string tagString = parts[0]
					.Replace("[", string.Empty)
					.Replace("]", string.Empty)
					.Replace("\t", string.Empty);

				//compare the found tag with the tag we're looking for
				if (tagString == check.Tag)
				{
					//if the history is empty, this was the original tag we're looking for
					if (tagHistory.Count == 0) return i;
					//otherwise, check the next tag in the history list
					check = tagHistory.Last();
					tagHistory.RemoveAt(tagHistory.Count - 1);
				}
				else
				{
					//found the wrong tag, skip ahead based on line count in file
					if (parts.Length < 2) return -4;
					bool parseLineCountSuccessful = int.TryParse(parts[1], out int lineCount);
					if (!parseLineCountSuccessful) return -4;
					i += lineCount;
				}
			}

			//tag not found
			return -2;
		}

		/// <summary>
		/// Checks to see if a string contains a tag with expected formatting. (i.e. contains a left square bracket followed by a right bracket with any number of characters in between).
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private static bool IsTag(string s)
		{
			bool foundLeftBracket = false;
			
			//iterate over string
			for (int i = 0; i < s.Length; i++)
			{
				//look for left bracket if not yet found
				if (!foundLeftBracket && s[i] == '[')
				{
					foundLeftBracket = true;
					continue;
				}

				//look for right bracket if left bracket has been found
				if (s[i] == ']' && foundLeftBracket) return true;
			}

			//criteria not met
			return false;
		}

		/// <summary>
		/// Adds the lines of a file to a dictionary for caching.
		/// If a file was already open but was somehow deleted unexpectedly, then the file is then closed.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="createFileIfNotFound"></param>
		/// <returns>Returns whether the file was opened and cached or not</returns>
		public static bool OpenFile(string filename, bool createFileIfNotFound)
		{
			//check if file exists
			bool fileExists = SaveLoad.SaveExists(filename);
			//check if file has been opened previously
			bool fileOpened = FileOpened(filename);
			if (!fileOpened)
			{
				if (!fileExists)
				{
					if (createFileIfNotFound)
					{
						//create file
						SaveLoad.SaveText(filename, string.Empty);
					}
					else
					{
						//return that no file was opened
						return false;
					}
				}

				//get lines from file and save them
				string text = SaveLoad.LoadText(filename);
				List<string> lines = text.Split('\n', '\r').ToList();
				RemoveBlankLines(lines);
				openedFiles.Add(filename, lines);
			}
			else if (!fileExists)
			{
				CloseFile(filename);
			}

			//return that the file was opened and cached
			return true;
		}

		/// <summary>
		/// Iterates over the list of strings given and removes blank entries.
		/// </summary>
		/// <param name="lines"></param>
		private static void RemoveBlankLines(List<string> lines)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				if (string.IsNullOrWhiteSpace(lines[i]))
				{
					lines.RemoveAt(i--);
				}
			}
		}

		/// <summary>
		/// Checks to see if the file has been previously read and not closed.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		private static bool FileOpened(string filename)
			=> openedFiles.ContainsKey(filename);

		[MenuItem("Save System/Save Example Data")]
		private static void SaveTest()
		{
			blah a = new blah();
			a.blahName = "asdrfgadfv";
			a.position = Vector3.left;

			blah b = new blah();
			b.blahName = "adfhdghj";
			b.position = Vector3.right;

			SaveTag tag = new SaveTag("blah1");
			SaveTag tag2 = new SaveTag("blah2", tag);
			UpdateUnifiedSaveFile(tag, a);
			UpdateUnifiedSaveFile(tag2, b);
			SaveUnifiedSaveFile();
		}

		private struct blah
		{
			public string blahName;
			public Vector3 position;
		}
	} 
}
