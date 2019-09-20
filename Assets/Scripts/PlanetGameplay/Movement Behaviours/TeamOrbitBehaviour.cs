using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamOrbitBehaviour : OrbitBehaviour
{
	[SerializeField] private List<TeamOrbitBehaviour> team;
	[SerializeField] private int orbitID;

	private void Awake()
	{
		JoinTeam(this);
	}

	private void OnDisable()
	{
		LeaveTeam();
	}

	private void JoinTeam(TeamOrbitBehaviour member)
	{
		AddToTeam(member);
		ShareTeamList();
		UpdateOrbitIDsOfTeam();
	}

	private void LeaveTeam()
	{
		for (int i = team.Count - 1; i >= 0; i--)
		{
			TeamOrbitBehaviour otherMember = team[i];
			otherMember.RemoveFromTeam(this);
		}

		UpdateOrbitIDsOfTeam();
	}

	private void RemoveFromTeam(TeamOrbitBehaviour member) => team.Remove(member);

	private void UpdateOrbitIDsOfTeam()
	{
		for (int i = 0; i < team.Count; i++)
		{
			TeamOrbitBehaviour member = team[i];
			if (!HasUniqueAndValidID(member))
			{
				member.orbitID = GetUniqueID(member);
			}
		}
	}

	private void ShareTeamList()
	{
		for (int i = 0; i < team.Count; i++)
		{
			TeamOrbitBehaviour otherMember = team[i];
			for (int j = 0; j < otherMember.team.Count; j++)
			{
				for (int k = 0; k < team.Count; k++)
				{
					otherMember.team[j].AddToTeam(team[k]);
					AddToTeam(otherMember.team[j]);
				}
			}
		}
	}

	public void AddToTeam(TeamOrbitBehaviour member)
	{
		if (CheckMemberInList(member)) return;
		team.Add(member);
		if (!HasUniqueAndValidID(member))
		{
			member.orbitID = GetUniqueID(member);
		}
	}

	private bool HasUniqueAndValidID(TeamOrbitBehaviour member)
	{
		int id = member.orbitID;
		if (id >= team.Count || id < 0) return false;
		for (int i = 0; i < team.Count; i++)
		{
			if (team[i] != member && team[i].orbitID == id) return false;
		}
		return true;
	}

	private int GetUniqueID(TeamOrbitBehaviour member)
	{
		for (int i = 0; i < team.Count; i++)
		{
			bool IDTaken = false;
			for (int j = 0; j < team.Count; j++)
			{
				if (team[j] != member && team[j].orbitID == i)
				{
					IDTaken = true;
					break;
				}
			}
			if (!IDTaken) return i;
		}
		Debug.Log("No unique ID found");
		return -1;
	}

	private bool CheckMemberInList(TeamOrbitBehaviour member)
	{
		for (int i = 0; i < team.Count; i++)
		{
			if (team[i] == member) return true;
		}
		return false;
	}

	protected override float GetIntendedAngle()
		=> base.GetIntendedAngle() - (Mathf.PI * 2f / team.Count * orbitID);
}
