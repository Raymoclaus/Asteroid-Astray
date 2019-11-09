using System;

public interface IPoolable
{
	event Action<IPoolable> OnReturnToPool;
	bool IsAttachedToPool { get; set; }
}
