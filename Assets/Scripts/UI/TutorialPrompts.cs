using UnityEngine;

[RequireComponent(typeof(PromptUI))]
public class TutorialPrompts : MonoBehaviour
{
	private PromptUI ui;
	[SerializeField] private ShuttleTrackers shuttleTracker;
	[SerializeField] private Character mainChar;
	
	[SerializeField] private PromptInfo goInputPromptInfo;
	[SerializeField] private PromptInfo launchInputPromptInfo;

	private void Awake()
	{
		shuttleTracker = shuttleTracker ?? Resources.Load<ShuttleTrackers>("ShuttleTrackerSO");
		ui = GetComponent<PromptUI>();
		mainChar = mainChar ?? FindObjectOfType<Shuttle>();

		shuttleTracker.OnGoInput += () => goInputPromptInfo.Deactivate(ui);
		goInputPromptInfo.SetCondition(() =>
		{
			return !Pause.IsStopped && shuttleTracker.hasControl;
		});

		shuttleTracker.OnLaunchInput += () => launchInputPromptInfo.Deactivate(ui);
		launchInputPromptInfo.SetCondition(() =>
		{
			return mainChar.IsDrilling && mainChar.CanDrillLaunch() && mainChar.ShouldLaunch();
		});
	}

	private void Update()
	{
		goInputPromptInfo.Check(ui);
		launchInputPromptInfo.Check(ui);
	}

	[System.Serializable]
	private struct PromptInfo
	{
		public float delay;
		private float delayTimer;
		[TextArea(1, 1)]
		public string text;
		public bool isRepeatable;
		private int activationCount;
		private bool isActivated;
		public float fadeInTime, fadeOutTime;
		[Tooltip("If true, and if the \"Deactivate\" condition is triggered early, then the prompt will never appear.")]
		public bool ignoreOnDeactivate;
		private bool ignore;
		private System.Func<bool> condition;

		public void Check(PromptUI ui)
		{
			if (condition != null && condition())
			{
				if (AddTimer(Time.deltaTime))
				{
					Activate(ui);
				}
			}
			else
			{
				ResetTimer();
			}
		}

		public void SetCondition(System.Func<bool> condition)
		{
			this.condition = condition;
		}

		private bool AddTimer(float add)
		{
			delayTimer += add;
			bool reached = DelayIsReached();
			if (reached)
			{
				ResetTimer();
			}
			return reached;
		}

		private void ResetTimer()
		{
			delayTimer = 0f;
		}

		private bool DelayIsReached()
		{
			return delayTimer >= delay;
		}
		
		private void Activate(PromptUI ui)
		{
			if (isActivated || ignore) return;
			if (!isRepeatable && activationCount > 0) return;

			ui.ActivatePrompt(text, fadeInTime);
			isActivated = true;
			activationCount++;
		}

		public void Deactivate(PromptUI ui)
		{
			if (ignoreOnDeactivate)
			{
				SetIgnore(true);
			}

			if (!isActivated) return;

			ui.DeactivatePrompt(text, fadeOutTime);
			isActivated = false;
		}

		public void SetIgnore(bool ignore)
		{
			this.ignore = ignore;
		}

		public void Refresh(bool ignore)
		{
			activationCount = 0;
			this.ignore = ignore;
		}
	}
}
