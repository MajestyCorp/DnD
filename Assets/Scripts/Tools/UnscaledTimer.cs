using UnityEngine;

namespace DnD
{
    public class UnscaledTimer
    {
        public bool IsActive { get { return Time.unscaledTime < timeEnd; } }
        public bool IsFinished { get { return Time.unscaledTime >= timeEnd; } }
        public float Progress { get { return CalcProgress(); } }
        private float timeEnd = 0f, timeStart = 0f;


        public void Activate(float timeAmount)
        {
            timeStart = Time.unscaledTime;
            timeEnd = timeStart + timeAmount;
        }

        public void Activate(float timeAmount, float progress = 0f)
        {
            timeStart = Time.unscaledTime - timeAmount * progress;
            timeEnd = timeStart + timeAmount;
        }

        private float CalcProgress()
        {
            if (IsFinished)
                return 1f;
            else
                return (Time.unscaledTime - timeStart) / (timeEnd - timeStart);
        }
    }
}