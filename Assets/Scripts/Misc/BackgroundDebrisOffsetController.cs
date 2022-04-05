using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundDebrisOffsetController : MonoBehaviour
{
	[SerializeField] private Transform _cameraTransform;
	[SerializeField] private string _backgroundOffsetShaderVarName;
	[SerializeField] private Renderer _renderer;
	private Vector4 _offsetValues;

	private void Update()
	{
		if (_cameraTransform == null
			|| string.IsNullOrWhiteSpace(_backgroundOffsetShaderVarName)
			|| _renderer == null) return;

		_offsetValues.x = _cameraTransform.position.x;
		_offsetValues.y = _cameraTransform.position.y;
		_renderer.material.SetVector(_backgroundOffsetShaderVarName, _offsetValues);
	}
}
