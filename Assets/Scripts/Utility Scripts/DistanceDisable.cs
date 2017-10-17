using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceDisable : MonoBehaviour {
	public MonoBehaviour[] scriptComponents;
	public Renderer rend;
	public Animator anim;

	void Start() {
		if (scriptComponents.Length == 0) {
			scriptComponents = GetComponents<MonoBehaviour>();
		}
		if (rend == null) {
			rend = GetComponent<Renderer>();
		}
		if (anim == null) {
			anim = GetComponent<Animator>();
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Activation")) {
			SetAll(true);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Activation")) {
			SetAll(false);
		}
	}

	private void SetAll(bool active) {
		foreach (MonoBehaviour script in scriptComponents) {
			if (script != null) {
				script.enabled = active;
			}
		}
		if (rend != null) {
			rend.enabled = active;
		}
		if (anim != null) {
			anim.enabled = active;
		}
	}
}
