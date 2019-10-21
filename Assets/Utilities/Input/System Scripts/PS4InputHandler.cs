using UnityEngine;

namespace InputHandler
{
	public class Ps4InputHandler : CustomInputHandler
	{
		//https://pbs.twimg.com/media/Dt6UeTLX4AEYylF.jpg

		public override bool ProcessInputs(InputContext context)
		{
			if (Input.GetAxisRaw("PS4_LeftStick_Horizontal") != 0f
				|| Input.GetAxisRaw("PS4_LeftStick_Vertical") != 0f) return true;

			return base.ProcessInputs(context);
		}

		public override float GetLookAngle(Vector2 refLocation)
		{
			Vector2 axisInput = new Vector2(
				Input.GetAxisRaw("PS4_LeftStick_Horizontal"),
				Input.GetAxisRaw("PS4_LeftStick_Vertical"));

			//if the control is not being used then return a non-usable value
			if (Mathf.Approximately(axisInput.x, 0f) && Mathf.Approximately(axisInput.y, 0f))
				return float.PositiveInfinity;

			return -Vector2.SignedAngle(Vector2.up, axisInput);
		}

		public override InputMode GetInputMode() => InputMode.PS4;
	}
}