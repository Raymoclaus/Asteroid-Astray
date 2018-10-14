using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUIController : MonoBehaviour
{
	[SerializeField]
	private RectTransform[] tabs;

	public void ClickedTab(int tabID)
	{
		bool organised = false;
		int increment = 0;
		while (!organised)
		{
			int check = tabID + increment;
			bool reachedLeftEdge = check >= tabs.Length;
			if (!reachedLeftEdge)
			{
				tabs[check].SetAsFirstSibling();
			}
			check = tabID - increment;
			bool reachedRightEdge = check < 0;
			if (!reachedRightEdge)
			{
				tabs[check].SetAsFirstSibling();
			}
			if (reachedLeftEdge && reachedRightEdge)
			{
				organised = true;
			}
			increment++;
		}
	}
}