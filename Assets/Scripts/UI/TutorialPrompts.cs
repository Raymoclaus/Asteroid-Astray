using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PromptUI))]
public class TutorialPrompts : MonoBehaviour
{
	private PromptUI ui;
	private Shuttle mainChar;
	private Shuttle MainChar
	{
		get { return mainChar ?? (mainChar = FindObjectOfType<Shuttle>()); }
	}
	
	public PromptInfo goInputPromptInfo;
	public PromptInfo launchInputPromptInfo;
	public PromptInfo drillInputPromptInfo;
	public PromptInfo pauseInputPromptInfo;
	public PromptInfo repairKitInputPromptInfo;

	private List<PromptInfo> prompts = new List<PromptInfo>();

	private void Awake()
	{
		ui = GetComponent<PromptUI>();
		mainChar = mainChar ?? FindObjectOfType<Shuttle>();

		prompts.Add(goInputPromptInfo);
		prompts.Add(launchInputPromptInfo);
		prompts.Add(drillInputPromptInfo);
		prompts.Add(pauseInputPromptInfo);
		prompts.Add(repairKitInputPromptInfo);

		IterateAll((PromptInfo pi) => pi.SetUI(ui));

		SetUpGoInputPrompt();
		SetUpLaunchInputPrompt();
		SetUpDrillInputPrompt();
		SetUpPauseInputPrompt();
		SetUpRepairKitInputPrompt();
	}

	private void OnDestroy()
	{
		IterateAll((PromptInfo pi) => pi.SetIgnore(true));
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
			return mainChar.IsDrilling && mainChar.CanDrillLaunch();
		});
	}

	private void SetUpDrillInputPrompt()
	{
		MainChar.OnDrillComplete += (bool successful) =>
		{
			if (successful) return;
			drillInputPromptInfo.Activate();
		};
	}

	private void SetUpPauseInputPrompt()
	{
		Pause.PauseEventHandler action = (bool pausing) =>
		{
			if (!pausing) return;
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
		Character.ItemUsedEventHandler action = (Item.Type type) =>
		{
			if (type != Item.Type.RepairKit) return;
			repairKitInputPromptInfo.Deactivate();
		};
		repairKitInputPromptInfo.SetListeners(() =>
		{
			mainChar.OnItemUsed += action;
		}, () =>
		{
			mainChar.OnItemUsed -= action;
		});

		repairKitInputPromptInfo.SetCondition(() =>
		{
			Item.Type repairKit = Item.Type.RepairKit;
			int id = mainChar.storage.FirstInstanceId(Item.Type.RepairKit);
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
		IterateAll((PromptInfo pi) => pi.Check());
	}

	private void IterateAll(System.Action<PromptInfo> action)
	{
		if (action == null) return;

		for (int i = 0; i < prompts.Count; i++)
		{
			action(prompts[i]);
		}
	}

	[System.Serializable]
	public class PromptInfo
	{
		private PromptUI ui;
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
		private System.Func<bool> condition;
		private System.Action startListening, stopListening;
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

		public void SetCondition(System.Func<bool> condition)
		{
			this.condition = condition;
		}

		public void SetUI(PromptUI ui)
		{
			this.ui = ui;
		}

		public void SetListeners(System.Action start, System.Action stop)
		{
			startListening = start;
			stopListening = stop;

			if (!ignore)
			{
				startListening?.Invoke();
			}
		}

		public void SetText(string s)
		{
			text = s;
			if (isActivated)
			{
				ui.ActivatePrompt(text);
			}
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
				ui.ActivatePromptTimer(text, promptDuration);
				PromptUI.PromptUpdatedEventHandler action = null;
				action = (string promptText, bool activating) =>
				{
					if (promptText != text || (promptText == text && !activating))
					{
						SetIsActivated(false);
					}
					PromptUI.OnPromptUpdated -= action;
				};
				PromptUI.OnPromptUpdated += action;
			}
			else
			{
				ui.ActivatePrompt(text);
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

			ui.DeactivatePrompt(text);
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
