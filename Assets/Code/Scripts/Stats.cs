using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Stats : MonoBehaviour
{
    [SerializeField] private TMP_Text ms;
    [SerializeField] private TMP_Text fpsText;

    private Queue<float> frameTimes = new Queue<float>();
    private Queue<float> fpsValues = new Queue<float>();
    private int maxSamples = 60; // Number of frames over which to average

    void Update()
    {
        // Record the current frame's delta time
        float currentFrameTime = Time.deltaTime;
        float currentFPS = 1.0f / currentFrameTime;

        // Add current values to the queues
        frameTimes.Enqueue(currentFrameTime);
        fpsValues.Enqueue(currentFPS);

        // Remove oldest if exceeding max samples
        if (frameTimes.Count > maxSamples)
        {
            frameTimes.Dequeue();
        }
        if (fpsValues.Count > maxSamples)
        {
            fpsValues.Dequeue();
        }

        // Calculate moving averages
        float averageFrameTime = 0f;
        foreach (float ft in frameTimes)
        {
            averageFrameTime += ft;
        }
        averageFrameTime /= frameTimes.Count;

        float averageFPS = 0f;
        foreach (float fps in fpsValues)
        {
            averageFPS += fps;
        }
        averageFPS /= fpsValues.Count;

        // Update UI text with moving averages
        ms.text = $"Time between frames: {(int)(averageFrameTime * 1000)} ms";
        fpsText.text = $"FPS: {(int)averageFPS}";
    }
}