using UnityEngine;
using System;

namespace vrm
{
    public class SystemTimeClock : MonoBehaviour
    {
        const float degreesPerHour = 30f, degreesPerMinute = 6f, degreesPerSecond = 6f;
        [SerializeField] private GameObject m_HourHand;
        [SerializeField] private GameObject m_MinuteHand;
        [SerializeField] private GameObject m_SecondHand;

        private void Awake()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;

            m_HourHand.transform.localRotation = Quaternion.Euler(0f, 0f, (float)time.TotalHours * degreesPerHour);
            m_MinuteHand.transform.localRotation = Quaternion.Euler(0f, 0f, (float)time.TotalMinutes * degreesPerMinute);
            m_SecondHand.transform.localRotation = Quaternion.Euler(0f, 0f, (float)time.TotalSeconds * degreesPerSecond);
        }

        private void Update()
        {
            Awake();
        }
    }
}
