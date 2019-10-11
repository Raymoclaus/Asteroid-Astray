using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatComponent : MonoBehaviour
{
	public string valueName = "Health";
	public float currentValue = 100f;
	
	public event Action<float, float> OnValueChanged;

	public virtual void SetValue(float amount)
	{
		float oldValue = currentValue;
		currentValue = amount;
		OnValueChanged?.Invoke(oldValue, currentValue);
	}

	public void AddValue(float amount)
	{
		SetValue(currentValue + amount);
	}

	public void SubtractValue(float amount)
	{
		AddValue(-amount);
	}
}
