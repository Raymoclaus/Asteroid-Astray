using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class EntityCountViewer : MonoBehaviour
{
	private Text txt;

	private void Awake()
	{
		txt = GetComponent<Text>();
	}

	private void Update()
	{
		txt.text = string.Format("Entity Count: {0}", EntityNetwork.GetEntityCount());
	}
}
