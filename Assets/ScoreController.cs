using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreController : MonoBehaviour
{
    public UIDocument uiDocument;

    private readonly List<float> _missedPlanesAtTime = new();

    private void Start()
    {
        StartCoroutine(nameof(DisplayScore));
    }

    private IEnumerator DisplayScore()
    {
        for (;;)
        {
            var aMinuteAgo = Time.time - 60f;
            var missedPlanesWithinOneMinute = _missedPlanesAtTime.Count(missedPlaneAtTime => missedPlaneAtTime > aMinuteAgo);
            var secondsSinceLastMiss = _missedPlanesAtTime.Count > 0 ? Mathf.RoundToInt(Time.time - _missedPlanesAtTime.Last()) : 0;
            
            var root = uiDocument.rootVisualElement;
            root.Q<Label>("ScoreValue").text = $"{missedPlanesWithinOneMinute}";
            root.Q<Label>("TimeSinceLastMissValue").text = $"{secondsSinceLastMiss}";
            yield return new WaitForSeconds(1f);
        }
    }

    public void RecordMissedPlane()
    {
        _missedPlanesAtTime.Add(Time.time);
    }
}
