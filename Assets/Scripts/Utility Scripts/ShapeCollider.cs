using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeCollider {
	public Shape shape;
	public Transform target;
	public bool isActive;
	private List<string> layers = new List<string>();

	public ShapeCollider(Transform target, Shape shape = null, bool isActive = true, string layer = "Default") {
		this.target = target;
		this.shape = shape ?? new Circle();
		this.isActive = isActive;
		this.layers.Add(layer);
	}

	public void AddLayer(string layerName) {
		layers.Add(layerName);
	}

	public bool IsInLayer(string layerName) {
		foreach (string layer in layers) {
			if (layer == layerName) {
				return true;
			}
		}
		return false;
	}

	public bool Intersects(ShapeCollider other) {
		return shape.Intersects(other.shape);
	}

	public bool HasCommonLayer(ShapeCollider other) {
		foreach(string layer in other.layers) {
			if (IsInLayer(layer)) {
				return true;
			}
		}
		return false;
	}

	public List<string> GetLayers() {
		return new List<string>(layers);
	}
}
