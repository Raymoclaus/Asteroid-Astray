using UnityEngine;
using ValueComponents;

public class HUDMeterController : MonoBehaviour
{
	[SerializeField] private RangedFloatComponent floatComponent;
	[SerializeField] private Material mat;
	[SerializeField] private float secondaryBarWaitDuration;
	[SerializeField] private string fillAmountString = "_FillAmount",
		damageFillAmountString = "_DamageFillAmount";
	private float secondaryBarAmount;
	private float secondaryBarWaitTimer;

	private void Awake()
	{
		floatComponent.OnValueChanged += UpdateValues;
	}

	public void UpdateMeter()
	{
		secondaryBarWaitTimer += Time.deltaTime;
		if (secondaryBarWaitTimer > secondaryBarWaitDuration)
		{
			float damageVal = mat.GetFloat(damageFillAmountString);
			damageVal = Mathf.MoveTowards(damageVal, 0f, Time.deltaTime);
			SetValue(damageFillAmountString, damageVal);
		}
	}

	public void UpdateValues(float oldVal, float newVal)
	{
		SetValue(fillAmountString, newVal);
		if (secondaryBarWaitTimer > secondaryBarWaitDuration)
		{
			SetValue(damageFillAmountString, oldVal);
		}
		secondaryBarWaitTimer = 0f;
	}

	public void SetValue(string propertyName, float value)
	{
		mat.SetFloat(propertyName, value);
	}
}
