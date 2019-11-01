using System;
using System.Timers;

namespace TimerUtilities
{
	public class SmartTimer
	{
		private Timer timer;
		private int playCount;
		private int counter;

		public event Action OnStarted, OnStopped;
		public event Action<int> OnElapsed;

		public SmartTimer()
		{
			timer = new Timer();
		}

		private void SetupTimer(double interval, int playCount, Action<int> intervalAction, bool dispose)
		{
			if (!IsValidTimeInterval(interval)
				|| intervalAction == null
				|| playCount <= 0) return;
			timer.Interval = interval;
			this.playCount = playCount;
			counter = 0;
			timer.AutoReset = playCount != 1;
			timer.Elapsed += (object sender, ElapsedEventArgs e) =>
			{
				counter++;
				intervalAction(counter);
				OnElapsed?.Invoke(counter);
				if (counter >= playCount && playCount > 0)
				{
					StopTimer(dispose);
				}
			};
		}

		private void StartTimer()
		{
			timer.Start();
			OnStarted?.Invoke();
		}

		public void StopTimer(bool dispose)
		{
			timer.Stop();
			counter = 0;
			OnStopped?.Invoke();
			if (dispose)
			{
				timer.Dispose();
			}
		}

		/// <summary>
		/// Calls an action once after a given time interval has elapsed.
		/// </summary>
		/// <param name="interval">Time before the action is called.</param>
		/// <param name="onFinish">The action to be called.</param>
		/// <param name="autoDispose">Cleans up resources on the timer when it is finished running. Give false if you plan on using it again.</param>
		public void DelayedAction(double interval, Action onFinish, bool autoDispose)
		{
			if (onFinish == null || !IsValidTimeInterval(interval)) return;
			SetupTimer(interval, 1, (int count) => onFinish(), autoDispose);
			StartTimer();
		}

		/// <summary>
		/// Calls an action a certain number of times at a given time interval between each call.
		/// </summary>
		/// <param name="interval">Time between each call. Must be between 0 and double.MaxValue</param>
		/// <param name="onElapsed">The action to be called. int parameter represents the number of times the action has been called.</param>
		/// <param name="repeatCount">Number of times to repeat the action. Use a negative value for infinite repetition.</param>
		public void RepeatingAction(double interval, Action<int> onElapsed, int repeatCount, bool autoDispose)
		{
			if (onElapsed == null || !IsValidTimeInterval(interval)) return;
			SetupTimer(interval, repeatCount, onElapsed, autoDispose);
			StartTimer();
		}

		public void DurationAction(double duration, double interval, Action<int> onElapsed, Action onCompleted, bool autoDispose)
		{
			if (onElapsed == null || !IsValidTimeInterval(interval)) return;
			if (duration < 0) return;
			int repeatCount = (int)(duration / interval);
			RepeatingAction(interval, onElapsed, repeatCount, autoDispose);

			if (onCompleted != null)
			{
				SmartTimer tempTimer = new SmartTimer();
				tempTimer.DelayedAction(duration, onCompleted, true);
			}
		}

		private bool IsValidTimeInterval(double interval)
			=> interval < 0 || interval > double.MaxValue;

		public bool IsRunning => timer.Enabled;
	}
}