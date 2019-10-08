using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Saro.BT
{
    public class UBTContext : MonoBehaviour
    {
        public static UBTContext Instance
        {
            get
            {
                if (m_instance == null)
                {
                    var go = new GameObject("BehaviourTree Context");
                    m_instance = go.AddComponent<UBTContext>();
                    go.isStatic = true;
                }
                return m_instance;
            }
        }

        private static UBTContext m_instance = null;

        private Clock m_clock = new Clock();
        private Dictionary<string, Blackboard> m_blackboards = new Dictionary<string, Blackboard>();

        public Clock GetClock()
        {
            return Instance.m_clock;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">key is also blackboard's name</param>
        /// <returns></returns>
        public Blackboard GetGlobalBlackboard(string key)
        {
            if (!Instance.m_blackboards.ContainsKey(key))
            {
                Instance.m_blackboards.Add(key, new Blackboard(key, Instance.m_clock));
            }
            return Instance.m_blackboards[key];
        }

        private void Update()
        {
            m_clock.Tick(Time.deltaTime);
        }
    }

}