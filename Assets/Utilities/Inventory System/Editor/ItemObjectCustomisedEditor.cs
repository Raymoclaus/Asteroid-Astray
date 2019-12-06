using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.CustomisedEditor
{
	[CustomEditor(typeof(ItemObject), true)]
	[CanEditMultipleObjects]
	public class ItemObjectCustomisedEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
}
