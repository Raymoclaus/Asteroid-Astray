using System.Collections;
using System.Collections.Generic;

namespace CustomDataTypes
{
	public class DirectionalArray<T> : IEnumerable<T>
	{
		private T[] array;

		public DirectionalArray()
		{
			array = new T[Length];
		}

		public T this[Direction direction]
		{
			get
			{
				return array[(int)direction];
			}
			set
			{
				array[(int)direction] = value;
			}
		}

		public T Up
		{
			get => array[(int)Direction.Up];
			set => array[(int)Direction.Up] = value;
		}

		public T Right
		{
			get => array[(int)Direction.Right];
			set => array[(int)Direction.Right] = value;
		}

		public T Down
		{
			get => array[(int)Direction.Down];
			set => array[(int)Direction.Down] = value;
		}

		public T Left
		{
			get => array[(int)Direction.Left];
			set => array[(int)Direction.Left] = value;
		}

		public int Length => System.Enum.GetValues(typeof(Direction)).Length;

		public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)array).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)array).GetEnumerator();
	}

}