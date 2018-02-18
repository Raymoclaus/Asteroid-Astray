using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class PhysicsActiveViewer : MonoBehaviour
{
	private Text txt;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		txt.text = string.Format("Physics Active: {0}", Entity.GetActive());
	}
}
