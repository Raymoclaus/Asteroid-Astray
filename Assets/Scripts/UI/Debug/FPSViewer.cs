using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSViewer : MonoBehaviour
{
	public float fpsMeasurePeriod = 0.5f;
	public int maxFPS = 1000;

	private int _accumulatedFPS;
	private float _nextFlushTime;
	private int _currentFPS;
	private string[] _fpsStrings;

	private Text _textComponent;

	private void Start()
	{
		_nextFlushTime = Time.realtimeSinceStartup + fpsMeasurePeriod;
		_textComponent = GetComponent<Text>();

		_fpsStrings = new string[maxFPS + 1];
		for (int i = 0; i < maxFPS; i++)
		{
			_fpsStrings[i] = i.ToString() + " FPS";
		}
		_fpsStrings[maxFPS] = maxFPS.ToString() + "+ FPS";
	}

	private void Update()
	{
		_accumulatedFPS++;

		if (Time.realtimeSinceStartup >= _nextFlushTime)
		{
			_currentFPS = (int)(_accumulatedFPS / fpsMeasurePeriod);
			_accumulatedFPS = 0;
			_nextFlushTime += fpsMeasurePeriod;
			if (_currentFPS <= maxFPS)
			{
				_textComponent.text = _fpsStrings[_currentFPS];
			}
			else
			{
				_textComponent.text = _fpsStrings[maxFPS];
			}
		}
	}
}