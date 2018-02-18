using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MaxPhysicsRangeViewer : MonoBehaviour
{
	private Text txt;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		txt.text = string.Format("Max Physics Range: {0}", Cnsts.MAX_PHYSICS_RANGE);
	}
}
