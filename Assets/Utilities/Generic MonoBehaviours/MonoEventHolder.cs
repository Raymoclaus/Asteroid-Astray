using System;
using UnityEngine;
using UnityEngine.Events;

public class MonoEventHolder : MonoBehaviour
{
	public event Action OnAwakeAction, OnStartAction, OnUpdateAction,
		OnFixedUpdateAction, OnLateUpdateAction, OnEnableAction,
		OnDisableAction, OnDestroyAction;

	public UnityEvent OnAwakeEvent, OnStartEvent, OnUpdateEvent,
		OnFixedUpdateEvent, OnLateUpdateEvent, OnEnableEvent,
		OnDisableEvent, OnDestroyEvent;

	private void Awake()
	{
		OnAwakeEvent?.Invoke();
		OnAwakeAction?.Invoke();
	}

	private void Start()
	{
		OnStartEvent?.Invoke();
		OnStartAction?.Invoke();
	}

	private void Update()
	{
		OnUpdateEvent?.Invoke();
		OnUpdateAction?.Invoke();
	}

	private void FixedUpdate()
	{
		OnFixedUpdateEvent?.Invoke();
		OnFixedUpdateAction?.Invoke();
	}

	private void LateUpdate()
	{
		OnLateUpdateEvent?.Invoke();
		OnLateUpdateAction?.Invoke();
	}

	private void OnEnable()
	{
		OnEnableEvent?.Invoke();
		OnEnableAction?.Invoke();
	}

	private void OnDisable()
	{
		OnDisableEvent?.Invoke();
		OnDisableAction?.Invoke();
	}

	private void OnDestroy()
	{
		OnDestroyEvent?.Invoke();
		OnDestroyAction?.Invoke();
	}
}
