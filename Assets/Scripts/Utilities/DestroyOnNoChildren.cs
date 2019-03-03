using UnityEngine;

public class DestroyOnNoChildren : MonoBehaviour
{
	private Transform t;

	private void Awake()
	{
		t = transform;
	}
	
	void Update()
	{
		if (t.childCount == 0)
		{
			Destroy(gameObject);
		}
	}
}
