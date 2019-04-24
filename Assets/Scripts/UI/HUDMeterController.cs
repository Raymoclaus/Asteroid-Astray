using UnityEngine;

public class HUDMeterController : MonoBehaviour
{
	[SerializeField] private MeterValues health, shield, boost;

	private Shuttle mainChar;
	private Shuttle MainChar
	{
		get
		{
			return mainChar ?? (mainChar = FindObjectOfType<Shuttle>());
		}
	}
	[SerializeField] private ShakeEffect shakeEffect;
	[SerializeField] private float shakeMultiplier = 1f;

	private void Awake()
	{
		LoadingController.AddListener(Setup);
	}

	private void Setup()
	{
		MainChar.OnHealthUpdate += health.UpdateValues;
		MainChar.OnShieldUpdated += shield.UpdateValues;
		MainChar.OnBoostAmountChanged += boost.UpdateValues;

		health.SetValue(MeterValues.fillAmountString, MainChar.GetHpRatio());
		shield.SetValue(MeterValues.fillAmountString, MainChar.ShieldRatio);
		boost.SetValue(MeterValues.fillAmountString, MainChar.GetBoostRemaining());

		MainChar.OnHealthUpdate += CheckShake;
	}

	public void CheckShake(float oldHP, float newHP)
	{
		if (newHP < oldHP)
		{
			shakeEffect?.Begin((oldHP - newHP) * shakeMultiplier, 0f, shakeMultiplier * 0.01f);
		}
	}

	private void Update()
	{
		health.UpdateMeter();
		shield.UpdateMeter();
		boost.UpdateMeter();
	}

	[System.Serializable]
	private class MeterValues
	{
		public Material mat;
		public float damageWaitDuration;
		public const string fillAmountString = "_FillAmount", damageFillAmountString = "_DamageFillAmount";
		private float damageAmount;
		private float damageWaitTimer;

		public void UpdateMeter()
		{
			damageWaitTimer += Time.deltaTime;
			if (damageWaitTimer > damageWaitDuration)
			{
				float damageVal = mat.GetFloat(damageFillAmountString);
				damageVal = Mathf.MoveTowards(damageVal, 0f, Time.deltaTime);
				SetValue(damageFillAmountString, damageVal);
			}
		}

		public void UpdateValues(float oldVal, float newVal)
		{
			SetValue(fillAmountString, newVal);
			if (damageWaitTimer > damageWaitDuration)
			{
				SetValue(damageFillAmountString, oldVal);
			}
			damageWaitTimer = 0f;
		}

		public void SetValue(string propertyName, float value)
		{
			mat.SetFloat(propertyName, value);
		}
	}
}
