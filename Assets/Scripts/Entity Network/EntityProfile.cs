using UnityEngine;

[System.Serializable]
public class EntityProfile
{
	public string entityName;
	public Sprite face;

	public EntityProfile(string entityName, Sprite face)
	{
		this.entityName = entityName;
		this.face = face;
	}
}
