#if FIXED_POINT_MATH
using Single = Saro.FPMath.sfloat;
#else
using Single = System.Single;
#endif

using System;
using Newtonsoft.Json;
using Saro.Utility;
using UnityEngine;

namespace Saro.BT.Utility
{
    [Serializable]
    public class Timer
    {
        [Min(0)]
        public Single interval = 1;

        [Tooltip("Adds a random range value to the interval between [-Deviation, +Deviation]")]
        [Min(0)]
#if FIXED_POINT_MATH
        public Single deviation = 1 / (Single)10;
#else
        public Single deviation = 0.1f;
#endif

        [JsonIgnore]
        public bool AutoRestart { get; set; } = false;

        [JsonIgnore]
        public bool IsDone => TimeLeft <= 0;

        [JsonIgnore]
        public bool IsRunning => !IsDone;

        [JsonIgnore]
        public Action OnTimeout; // TODO 使用的地方，缓存action

        [JsonIgnore]
        public Single TimeLeft { get; private set; } = 0;

        public Timer() { }

        public Timer(Timer timer)
        {
            interval = timer.interval;
            deviation = timer.deviation;
            AutoRestart = timer.AutoRestart;
        }

        public void Start()
        {
            TimeLeft = interval;
            if (deviation > Single.Epsilon)
            {
                TimeLeft += GRandom.NextFloat(-deviation, deviation);
                //TimeLeft += UnityEngine.Random.Range(-deviation, deviation);
            }
        }

        public void Tick(Single delta)
        {
            if (TimeLeft > 0)
            {
                TimeLeft -= delta;
                if (IsDone)
                {
                    OnTimeout?.Invoke();
                    if (AutoRestart)
                    {
                        Start();
                    }
                }
            }
        }

        /// <summary>
        /// for
        /// <see cref="BTRunTimeValueAttribute"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => TimeLeft.ToString();

        public string GetIntervalInfo()
        {
            if (deviation > Single.Epsilon)
                return $"{interval}±{deviation}";
            else
                return interval.ToString();
        }
    }
}