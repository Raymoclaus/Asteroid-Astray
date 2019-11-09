using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : IPoolable
{
	protected Stack<T> pool = new Stack<T>();
	public event Action<T> OnResetObject, OnReleaseObject;
	private Func<T> ObjCopyAction { get; set; }

	public ObjectPool(Func<T> objCopyAction)
	{
		ObjCopyAction = objCopyAction;
	}

	public virtual T Get
	{
		get
		{
			if (pool.Count == 0)
			{
				CreateNewObject();
			}

			T obj = pool.Pop();
			OnResetObject?.Invoke(obj);
			return obj;
		}
	}

	public virtual void Release(IPoolable obj)
	{
		pool.Push((T)obj);
		OnReleaseObject?.Invoke((T)obj);
	}

	protected virtual void CreateNewObject()
	{
		T obj = ObjCopyAction();
		obj.OnReturnToPool += Release;
		obj.IsAttachedToPool = true;
		pool.Push(obj);
	}
}
