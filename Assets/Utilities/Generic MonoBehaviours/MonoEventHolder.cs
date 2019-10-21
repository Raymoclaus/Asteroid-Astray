using System;
using UnityEngine;

public class MonoEventHolder : MonoBehaviour
{
	public event Action OnAwakeEvent, OnStartEvent, OnUpdateEvent,
		OnFixedUpdateEvent, OnLateUpdateEvent, OnEnableEvent,
		OnDisableEvent, OnDestroyEvent;

	private void Awake() => OnAwakeEvent?.Invoke();

	private void Start() => OnStartEvent?.Invoke();
	
	private void Update() => OnUpdateEvent?.Invoke();

	private void FixedUpdate() => OnFixedUpdateEvent?.Invoke();

	private void LateUpdate() => OnLateUpdateEvent?.Invoke();

	private void OnEnable() => OnEnableEvent?.Invoke();

	private void OnDisable() => OnDisableEvent?.Invoke();

	private void OnDestroy() => OnDestroyEvent?.Invoke();
}
