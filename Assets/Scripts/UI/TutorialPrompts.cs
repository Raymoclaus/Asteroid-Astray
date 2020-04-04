using UnityEngine;
using System;
using System.Collections.Generic;
using PR = PromptSystem.PromptRequests;
using InventorySystem;

public class TutorialPrompts : MonoBehaviour
{
	private Shuttle m_mainChar;
	private Shuttle MainChar => m_mainChar ?? (m_mainChar = FindObjectOfType<Shuttle>());

	public PromptInfo goInputPromptInfo;
	public PromptInfo launchInputPromptInfo;
	public PromptInfo drillInputPromptInfo;
	public PromptInfo pauseInputPromptInfo;
	public PromptInfo repairKitInputPromptInfo;
	[SerializeField] private ItemObject repairKit;

	private List<PromptInfo> prompts = new List<PromptInfo>();

	private void Awake()
	{
		LoadingController.AddListener(SetUp);
	}

	private void SetUp()
	{
		prompts.Add(goInputPromptInfo);
		prompts.Add(launchInputPromptInfo);
		prompts.Add(drillInputPromptInfo);
		prompts.Add(pauseInputPromptInfo);
		prompts.Add(repairKitInputPromptInfo);

		SetUpGoInputPrompt();
		SetUpLaunchInputPrompt();
		SetUpDrillInputPrompt();
		SetUpPauseInputPrompt();
		SetUpRepairKitInputPrompt();
	}

	private void OnDestroy()
	{
		prompts.ForEach(t => t.SetIgnore(true));
	}

	private void SetUpGoInputPrompt()
	{
		goInputPromptInfo.SetListeners(() =>
		{
			MainChar.OnGoInput += goInputPromptInfo.Deactivate;
		}, () =>
		{
			MainChar.OnGoInput -= goInputPromptInfo.Deactivate;
		});

		goInputPromptInfo.SetCondition(() =>
		{
			return !Pause.IsStopped && MainChar.hasControl;
		});
	}

	private void SetUpLaunchInputPrompt()
	{
		Shuttle.DrillCompleteEventHandler action = (bool successful) =>
		{
			launchInputPromptInfo.Deactivate();
		};
		launchInputPromptInfo.SetListeners(() =>
		{
			MainChar.OnLaunchInput += launchInputPromptInfo.Deactivate;
			MainChar.OnDrillComplete += action;
		}, () =>
		{
			MainChar.OnLaunchInput -= launchInputPromptInfo.Deactivate;
			MainChar.OnDrillComplete -= action;
		});

		launchInputPromptInfo.SetCondition(() =>
		{
			return MainChar.IsDrilling && MainChar.CanDrillLaunch;
		});
	}

	private void SetUpDrillInputPrompt()
	{
		Shuttle.DrillCompleteEventHandler action = (bool successful) =>
		{
			if (successful) return;
			drillInputPromptInfo.Activate();
		};
		drillInputPromptInfo.SetListeners(() =>
		{
			MainChar.OnDrillComplete += action;
		}, () =>
		{
			MainChar.OnDrillComplete -= action;
		});
	}

	private void SetUpPauseInputPrompt()
	{
		Action action = () =>
		{
			pauseInputPromptInfo.Deactivate();
		};

		pauseInputPromptInfo.SetListeners(() =>
		{
			Pause.OnPause += action;
		}, () =>
		{
			Pause.OnPause -= action;
		});

		pauseInputPromptInfo.SetCondition(() =>
		{
			return true;
		});
	}

	private void SetUpRepairKitInputPrompt()
	{
		Action<ItemObject, int> action = (ItemObject type, int amount) =>
		{
			if (type != repairKit) return;
			repairKitInputPromptInfo.Deactivate();
		};
		repairKitInputPromptInfo.SetListeners(() =>
		{
			MainChar.OnItemUsed += action;
		}, () =>
		{
			MainChar.OnItemUsed -= action;
		});

		repairKitInputPromptInfo.SetCondition(() =>
		{
			int id = MainChar.DefaultInventory.FirstInstanceId(repairKit);
			if (id < 0 || Pause.IsStopped) return false;
			string typeName = Item.TypeName(repairKit);
			string text = id < 8 ? $"Press [Slot {id + 1}:] to use the {typeName}"
			: $"Place the {typeName} in one of the first 8 inventory slots";
			if (repairKitInputPromptInfo.text != text)
			{
				repairKitInputPromptInfo.SetText(text);
				return true;
			}
			return false;
		});
	}

	private void Update()
	{
		prompts.ForEach(t => t.Check());
	}

	[Serializable]
	public class PromptInfo
	{
		private const string TO_STRING = "Prompt: \"{0}\", Active: {1}";
		public float delay;
		private float delayTimer;
		[TextArea(1, 1)]
		public string text;
		public bool isTimedPrompt;
		public float promptDuration;
		public bool isRepeatable;
		private int activationCount;
		private bool isActivated;
		[Tooltip("If true, and if the \"Deactivate\" condition is triggered early, then the prompt will never appear.")]
		public bool ignoreOnDeactivate;
		public bool ignore;
		private Func<bool> condition;
		private Action startListening, stopListening;
		private bool isListening;

		public void Check()
		{
			if (condition?.Invoke() ?? false)
			{
				if (AddTimer(Time.deltaTime))
				{
					Activate();
				}
			}
			else
			{
				ResetTimer();
			}
		}

		public void SetCondition(Func<bool> condition)
		{
			this.condition = condition;
		}

		public void SetListeners(Action start, Action stop)
		{
			startListening = start;
			stopListening = stop;

			if (!ignore)
			{
				startListening?.Invoke();
				isListening = true;
			}
		}

		public void SetText(string s)
		{
			text = s;
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
		
		public void Activate()
		{
			if (isActivated || ignore) return;
			if (!isRepeatable && activationCount > 0) return;
			
			if (isTimedPrompt)
			{
				PR.PromptSendRequest(text, text);
				TimerTracker.SetTimer(text, promptDuration);
				TimerTracker.SetFinishAction(text, Deactivate);
			}
			else
			{
				PR.PromptSendRequest(text, text);
			}
			SetIsActivated(true);
			activationCount++;
		}

		public void Deactivate()
		{
			if (ignoreOnDeactivate)
			{
				SetIgnore(true);
			}
			
			if (!isActivated) return;

			PR.PromptRemovalRequest(text);
			SetIsActivated(false);
		}

		private void SetIsActivated(bool active)
		{
			isActivated = active;
		}

		public void SetIgnore(bool ignore)
		{
			this.ignore = ignore;
			if (ignore)
			{
				if (isListening)
				{
					stopListening?.Invoke();
					isListening = false;
				}
			}
			else
			{
				if (!isListening)
				{
					startListening?.Invoke();
					isListening = true;
				}
			}
		}

		public void Refresh(bool ignore)
		{
			activationCount = 0;
			this.ignore = ignore;
		}

		public override string ToString()
		{
			return string.Format(TO_STRING, text, isActivated);
		}
	}
}
