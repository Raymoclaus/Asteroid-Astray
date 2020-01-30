using System.Collections.Generic;
using System.Text;

namespace SaveSystem
{
	public class SaveTag
	{
		public string Tag { get; private set; }
		public SaveTag PriorTag { get; private set; }

		/// <summary>
		/// Used for indented tags with a parent tag
		/// </summary>
		/// <param name="tag">Name of tag</param>
		/// <param name="priorTag">Name of parent tag</param>
		public SaveTag(string tag, SaveTag priorTag)
		{
			Tag = tag;
			if (priorTag != this)
			{
				PriorTag = priorTag;
			}
		}

		/// <summary>
		/// Used for base level tags with no parent tags
		/// </summary>
		/// <param name="tag"></param>
		public SaveTag(string tag) : this(tag, null)
		{

		}

		/// <summary>
		/// Gets a sorted list of tag's parents where the innermost tag is at 0 and the base tag is last.
		/// </summary>
		/// <returns></returns>
		public List<SaveTag> GetTagHistory()
		{
			List<SaveTag> history = new List<SaveTag> { this };
			SaveTag check = PriorTag;
			while (check != null)
			{
				history.Add(check);
				check = check.PriorTag;
			}

			return history;
		}

		public int IndentCount
		{
			get
			{
				int count = 0;
				SaveTag check = PriorTag;

				while (check != null)
				{
					check = check.PriorTag;
					count++;
				}

				return count;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(Tag);
			SaveTag check = PriorTag;
			while (check != null)
			{
				sb.Insert(0, $"{check.Tag} > ");
				check = check.PriorTag;
			}

			return sb.ToString();
		}

		public static bool operator ==(SaveTag a, SaveTag b)
		{
			//null check
			bool aIsNull = ReferenceEquals(a, null);
			bool bIsNull = ReferenceEquals(b, null);
			if (aIsNull || bIsNull)
			{
				return aIsNull == bIsNull;
			}

			string checkA = a.Tag;
			string checkB = b.Tag;

			//check if tags are the same
			if (checkA != checkB) return false;

			//if both tags have no parent tag then return true
			bool aHasNoParent = a.PriorTag == null;
			bool bHasNoParent = b.PriorTag == null;

			//if both a and b have no parent, return true
			if (aHasNoParent && bHasNoParent) return true;

			//if only a or only b has no parent, return false
			if (aHasNoParent != bHasNoParent) return false;

			return a.PriorTag == b.PriorTag;
		}

		public static bool operator !=(SaveTag a, SaveTag b)
		{
			//null check
			bool aIsNull = ReferenceEquals(a, null);
			bool bIsNull = ReferenceEquals(b, null);
			if (aIsNull || bIsNull)
			{
				return aIsNull != bIsNull;
			}

			string checkA = a.Tag;
			string checkB = b.Tag;

			//check if tags are the same
			if (checkA != checkB) return true;

			//check whether each tag has a parent
			bool aHasNoParent = a.PriorTag == null;
			bool bHasNoParent = b.PriorTag == null;

			//if both a and b have no parent, tags match
			if (aHasNoParent && bHasNoParent) return false;

			//if only a or only b has no parent, tags don't match
			if (aHasNoParent != bHasNoParent) return true;

			//if both tags are equal and have parents, check the parents in the next iteration
			return a.PriorTag != b.PriorTag;
		}
	}
}