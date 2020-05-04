﻿using CustomDataTypes;
using SaveSystem;
using System.Collections.Generic;
using UnityEngine;

public class NebulaSetup : Entity
{
	[Header("Nebula Fields")]
	[SerializeField] private ParticleSystem[] systems;

	[Range(0f, 1f)]
	[SerializeField] private float color1Min = 0f;
	[Range(0f, 1f)]
	[SerializeField] private float color1Max = 0.25f;
	[Range(0f, 1f)]
	[SerializeField] private float color2Min = 0.5f;
	[Range(0f, 1f)]
	[SerializeField] private float color2Max = 0.75f;
	[Range(0f, 1f)]
	[SerializeField] private float alpha = 0.5f;
	private Color col1, col2;
	private bool colorSet = false;
	
	public bool ShouldExpand { get; set; }
	[SerializeField] private int minSystemSize = 4;
	[SerializeField] private int maxSystemSize = 8;
	//when expanding, it won't expand into space that already contains nebula. This will prevent accidental infinite loops
	private const int FAIL_LIMIT = 20;
	private List<NebulaSetup> cluster;

	private void Start()
	{
		//pick a color
		if (!colorSet)
		{
			SetColors();
		}
		//create more clouds
		if (ShouldExpand)
		{
			//parent this object to a group holder
			Transform t = new GameObject("Nebula Group").transform;
			t.parent = transform.parent;
			transform.parent = t;
			Expansion();
			ShouldExpand = false;
		}
	}

	//This will determine where to set up more particle systems and create them in those positions
	private void Expansion()
	{
		int size = Random.Range(minSystemSize, maxSystemSize + 1);
		cluster = new List<NebulaSetup>(size);
		List<ChunkCoords> filled = new List<ChunkCoords>(size);
		ChunkCoords c = coords;
		filled.Add(c);
		int count = 1;
		int failCount = 0;

		while (count < size && failCount < FAIL_LIMIT)
		{
			c = filled[Random.Range(0, filled.Count)];
			//pick a random adjacent coordinate
			float randomVal = Random.value;
			if (randomVal >= 0.5f)
			{
				c.x += randomVal >= 0.75f ? 1 : -1;
			}
			else
			{
				c.y += randomVal >= 0.25f ? 1 : -1;
			}
			c = c.Validate();

			bool alreadyExists = false;
			//this will check for nebulas already created in this group
			for (int i = 0; i < filled.Count; i++)
			{
				ChunkCoords check = filled[i];
				if (c == check)
				{
					alreadyExists = true;
					break;
				}
			}
			//if new coordinates have already been filled with nebula then pick a new coordinate
			if (alreadyExists)
			{
				failCount++;
				continue;
			}

			count++;
			filled.Add(c);
			NebulaSetup newNebula = Instantiate(this, transform.parent);
			cluster.Add(newNebula);
			newNebula.cluster = cluster;
			newNebula.SetColors(col1, col2);
			newNebula.transform.position = ChunkCoords.GetCenterCell(c, EntityNetwork.CHUNK_SIZE);
			newNebula.ShouldExpand = false;
			EntityGenerator.FillChunk(c, true);
		}
	}

	private void SetColors(Color? color1 = null, Color? color2 = null)
	{
		col1 = color1 != null ? (Color)color1 : new Color(
			Random.value * Mathf.Abs(color1Max - color1Min) + Mathf.Min(color1Min, color1Max),
			Random.value * Mathf.Abs(color1Max - color1Min) + Mathf.Min(color1Min, color1Max),
			Random.value * Mathf.Abs(color1Max - color1Min) + Mathf.Min(color1Min, color1Max),
			1f);
		col2 = color2 != null ? (Color)color2 : new Color(
			Random.value * Mathf.Abs(color2Max - color2Min) + Mathf.Min(color2Min, color2Max),
			Random.value * Mathf.Abs(color2Max - color2Min) + Mathf.Min(color2Min, color2Max),
			Random.value * Mathf.Abs(color2Max - color2Min) + Mathf.Min(color2Min, color2Max),
			1f);

		for (int i = 0; i < systems.Length; i++)
		{
			ParticleSystem ps = systems[i];

			//min gradient
			ParticleSystem.ColorOverLifetimeModule colMod = ps.colorOverLifetime;
			ParticleSystem.MinMaxGradient gradient = colMod.color;

			GradientColorKey[] colorKeys = gradient.gradientMin.colorKeys;
			colorKeys[0].color = col1;
			colorKeys[1].color = col2;
			gradient.gradientMin.colorKeys = colorKeys;

			GradientAlphaKey[] alphaKeys = gradient.gradientMin.alphaKeys;
			alphaKeys[1].alpha = alpha;
			alphaKeys[2].alpha = alpha;
			gradient.gradientMin.alphaKeys = alphaKeys;

			//max gradient
			GradientColorKey[] colorKeysMax = gradient.gradientMax.colorKeys;
			colorKeysMax[0].color = col2;
			colorKeysMax[1].color = col1;
			gradient.gradientMax.colorKeys = colorKeysMax;

			GradientAlphaKey[] alphaKeysMax = gradient.gradientMax.alphaKeys;
			alphaKeysMax[1].alpha = alpha;
			alphaKeysMax[2].alpha = alpha;
			gradient.gradientMax.alphaKeys = alphaKeysMax;

			colMod.color = gradient;
		}

		colorSet = true;
	}

	public override EntityType EntityType => EntityType.Nebula;

	public override SaveType SaveType => SaveType.FullSave;
}