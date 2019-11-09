using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ValueComponents
{
	public interface IRangedValue<T>
	{
		T UpperLimit { get; }
		T LowerLimit { get; }
		event Action OnValueReachedUpperLimit, OnValueReachedLowerLimit;
		event Action<float, float> OnRatioChanged;
		float CurrentRatio { get; }
		float GetRatio(T value);
		T DifferenceBetweenUpperAndLowerLimit { get; }
		bool IsAboveLowerLimit(T value);
		bool IsAboveOrEqualToLowerLimit(T value);
		bool IsBelowUpperLimit(T value);
		bool IsBelowOrEqualToUpperLimit(T value);
		bool IsBetweenLimits(T value);
		void SetToUpperLimit();
		void SetToLowerLimit();
		void SetRatio(float ratio);
		void AddRatio(float ratio);
		void SubtractRatio(float ratio);
		void MultiplyRatio(float multiplier);
		void SetUpperLimit(T value, bool keepCurrentRatio);
		void SetLowerLimit(T value, bool keepCurrentRatio);
	}
}