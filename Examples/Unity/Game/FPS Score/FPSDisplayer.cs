using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Text))]
public class FPSDisplayer : MonoBehaviour
{
    public ushort MaxFPS = 360;

    const float FpsMeasurePeriod = 0.5f;
    const string Display = "FPS: {0}";

    private Text m_Text;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;


    private void Start()
    {
        Application.targetFrameRate = MaxFPS;

        m_FpsNextPeriod = Time.realtimeSinceStartup + FpsMeasurePeriod;
        m_Text = GetComponent<Text>();
    }

    private void Update()
    {
        m_FpsAccumulator++;

        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = (int) (m_FpsAccumulator / FpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += FpsMeasurePeriod;
            m_Text.text = string.Format(Display, m_CurrentFps);
        }
    }
}
