
using System;
using System.Collections.Generic;

namespace Saro.BT
{
    // TODO, make more easy to use
    public class Blackboard
    {
        public enum Type
        {
            ADD,
            REMOVE,
            CHANGE
        }

        private struct Notification
        {
            public string key;
            public Type type;
            public ReferenceValue value;

            public Notification(string key, Type type, ReferenceValue value)
            {
                this.key = key;
                this.type = type;
                this.value = value;
            }
        }

        public class ReferenceValue
        {
            public object Value
            {
                get => m_value;
                set
                {
                    if (m_value != value)
                    {
                        m_value = value;
                        OnValueChanged?.Invoke();
                    }
                }
            }
            protected object m_value;

            public Action OnValueChanged;

        }

        public class ReferenceValue<T> : ReferenceValue
        {
            public new T Value
            {
                get => (T)m_value;
                set
                {
                    if (!m_value.Equals(value))
                    {
                        m_value = value;
                        OnValueChanged?.Invoke();
                    }
                }
            }

        }

        private string m_blackboardName;
        private Clock m_clock;
        private Dictionary<string, ReferenceValue> m_data = new Dictionary<string, ReferenceValue>();
        private Dictionary<string, List<Action<Type, object>>> m_observers = new Dictionary<string, List<Action<Type, object>>>();
        private Dictionary<string, List<Action<Type, object>>> m_addObservers = new Dictionary<string, List<Action<Type, object>>>();
        private Dictionary<string, List<Action<Type, object>>> m_removeObservers = new Dictionary<string, List<Action<Type, object>>>();
        private bool m_isNotifiying = false;
        private List<Notification> m_notifications = new List<Notification>();
        private List<Notification> m_notificationsDispatch = new List<Notification>();
        private Blackboard m_parentBlackboard;
        private HashSet<Blackboard> m_children = new HashSet<Blackboard>();

        public Blackboard(string blackboardName, Blackboard parrentBlackboard, Clock clock) : this(blackboardName, clock)
        {
            m_parentBlackboard = parrentBlackboard;
        }

        public Blackboard(string blackboardName, Clock clock)
        {
            m_clock = clock;
            m_blackboardName = blackboardName;
        }

        public Blackboard(string blackboardName)
        {
            m_clock = UBTContext.Instance.GetClock();
            m_blackboardName = blackboardName;
        }

        public void Enable()
        {
            if (m_parentBlackboard != null)
            {
                m_parentBlackboard.m_children.Add(this);
            }
        }

        public void Disable()
        {
            if (m_parentBlackboard != null)
            {
                m_parentBlackboard.m_children.Remove(this);
            }
            if (m_clock != null)
            {
                m_clock.RemoveTimer(NotifiyObservers);
            }
        }

        [Obsolete("Use Get/Set instead", true)]
        public object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }


        public void Set<T>(string key, T value = default)
        {
            if (m_parentBlackboard != null && m_parentBlackboard.IsSet(key))
            {
                m_parentBlackboard.Set(key, value);
            }
            else
            {
                if (!m_data.ContainsKey(key))
                {
                    m_data[key] = new ReferenceValue<T>();

                    m_data[key].OnValueChanged = () =>
                    {
                        m_notifications.Add(new Notification(key, Type.ADD, m_data[key]));
                        m_clock.AddTimer(0f, 0, NotifiyObservers);
                        //NotifiyObservers();
                    };

                    m_data[key].Value = value;
                }
                else
                {
                    if ((m_data[key] == null && value != null) || (m_data[key] != null && !m_data[key].Equals(value)))
                    {

                        m_data[key].OnValueChanged = () =>
                        {
                            m_notifications.Add(new Notification(key, Type.CHANGE, m_data[key]));
                            m_clock.AddTimer(0f, 0, NotifiyObservers);
                            //NotifiyObservers();
                        };

                        m_data[key].Value = value;
                    }
                }
            }
        }

        public void UnSet(string key)
        {
            if (m_data.ContainsKey(key))
            {
                m_data[key].OnValueChanged = null;
                m_data.Remove(key);
                m_notifications.Add(new Notification(key, Type.REMOVE, null));
                m_clock.AddTimer(0f, 0, NotifiyObservers);
                //NotifiyObservers();
            }
        }

        public ReferenceValue<T> Get<T>(string key)
        {
            return Get(key) as ReferenceValue<T>;
        }

        public ReferenceValue Get(string key)
        {
            if (m_data.ContainsKey(key))
            {
                return m_data[key];
            }
            else if (m_parentBlackboard != null)
            {
                return m_parentBlackboard.Get(key);
            }
            else
            {
                return null;
            }
        }

        public bool IsSet(string key)
        {
            return m_data.ContainsKey(key) || (m_parentBlackboard != null && m_parentBlackboard.IsSet(key));
        }

        public void AddObserver(string key, Action<Type, object> observer)
        {
            var observers = GetObserverList(m_observers, key);
            if (!m_isNotifiying)
            {
                if (!observers.Contains(observer))
                {
                    observers.Add(observer);
                }
            }
            else
            {
                if (!observers.Contains(observer))
                {
                    var addObservers = GetObserverList(m_addObservers, key);
                    if (!addObservers.Contains(observer))
                    {
                        addObservers.Add(observer);
                    }
                }

                var removeObservers = GetObserverList(m_removeObservers, key);
                if (removeObservers.Contains(observer))
                {
                    removeObservers.Remove(observer);
                }
            }
        }

        public void RemoveObserver(string key, Action<Type, object> observer)
        {
            var observers = GetObserverList(m_observers, key);
            if (!m_isNotifiying)
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
            else
            {
                var removeObservers = GetObserverList(m_removeObservers, key);
                if (!removeObservers.Contains(observer))
                {
                    if (observers.Contains(observer))
                    {
                        removeObservers.Add(observer);
                    }
                }

                var addObservers = GetObserverList(m_addObservers, key);
                if (addObservers.Contains(observer))
                {
                    addObservers.Remove(observer);
                }
            }
        }

        private void NotifiyObservers()
        {
            if (m_notifications.Count == 0) return;

            m_notificationsDispatch.Clear();
            m_notificationsDispatch.AddRange(m_notifications);
            foreach (Blackboard child in m_children)
            {
                child.m_notifications.AddRange(m_notifications);
                child.m_clock.AddTimer(0f, 0, child.NotifiyObservers);
            }
            m_notifications.Clear();

            m_isNotifiying = true;
            foreach (Notification notification in m_notificationsDispatch)
            {
                if (!m_observers.ContainsKey(notification.key)) continue;

                List<Action<Type, object>> observers = GetObserverList(m_observers, notification.key);
                foreach (Action<Type, object> observer in observers)
                {
                    if (m_removeObservers.ContainsKey(notification.key) && m_removeObservers[notification.key].Contains(observer))
                    {
                        continue;
                    }
                    observer(notification.type, notification.value);
                }
            }

            foreach (string key in m_addObservers.Keys)
            {
                GetObserverList(m_observers, key).AddRange(m_addObservers[key]);
            }
            foreach (string key in m_removeObservers.Keys)
            {
                foreach (Action<Type, object> action in m_removeObservers[key])
                {
                    GetObserverList(m_observers, key).Remove(action);
                }
            }
            m_addObservers.Clear();
            m_removeObservers.Clear();

            m_isNotifiying = false;
        }

        private List<Action<Type, object>> GetObserverList(Dictionary<string, List<Action<Type, object>>> target, string key)
        {
            List<Action<Type, object>> observers;
            if (target.ContainsKey(key))
            {
                observers = target[key];
            }
            else
            {
                observers = new List<Action<Type, object>>();
                target[key] = observers;
            }
            return observers;
        }

        public string GetStaticDescription()
        {
            return m_blackboardName;
        }


#if UNITY_EDITOR
        public List<string> Keys
        {
            get
            {
                if (m_parentBlackboard != null)
                {
                    List<string> keys = m_parentBlackboard.Keys;
                    keys.AddRange(m_data.Keys);
                    return keys;
                }
                else
                {
                    return new List<string>(m_data.Keys);
                }
            }
        }

        public int NumObservers
        {
            get
            {
                int count = 0;
                foreach (string key in m_observers.Keys)
                {
                    count += m_observers[key].Count;
                }
                return count;
            }
        }
#endif
    }
}