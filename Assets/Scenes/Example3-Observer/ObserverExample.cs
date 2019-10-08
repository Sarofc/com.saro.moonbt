using UnityEngine;
using System.Collections;
using Saro.BT;
using System;

public class ObserverExample : MonoBehaviour
{

    private Root m_Root;
    private float m_Time = 10f;
    
    void Start()
    {
        var bb = UBTContext.Instance.GetGlobalBlackboard("DefaultBB");
        bb.Set("energy", 10);
        bb.Set("sleep", false);

        var working = new Sequence().OpenBranch(
            new Wait(1f),
            new Actions(() => bb.Get<int>("energy").Value--),
            new Log("working")
        );

        var eating = new Sequence().OpenBranch(
            new Wait(1f),
            new Actions(() => bb.Get<int>("energy").Value++),
            new Log("eating")
        );

        var sleep = new Sequence().OpenBranch(
            new Wait(1f),
            new Log("sleep")
        );

        var sleepCondition = new BlackboardCondition<bool>("sleep", Operator.IS_EQUAL, true, ObserverAborts.BOTH).Decorate(sleep);
        var eatingCondition = new BlackboardCondition<int>("energy", Operator.IS_SMALLER, 5, ObserverAborts.BOTH).Decorate(eating);
        
        var selector = new Selector().OpenBranch(
            sleepCondition,
            eatingCondition,
            working
        );

        var service = new DefaultService(UpdateStatus).Decorate(selector);

        m_Root = new Root(bb).Decorate(service);

#if UNITY_EDITOR
        UBTDebugger debugger = gameObject.AddComponent<UBTDebugger>();
        debugger.behaviorTree = m_Root;
#endif
        //m_Root.RepeatRoot = false;
        m_Root.Start();
    }

    private void UpdateStatus()
    {
        if (m_Time <= 0f)
        {
            m_Root.Blackboard.Get<bool>("sleep").Value = true;
        }
    }

    private void Update()
    {
        m_Time -= Time.deltaTime;
    }
}
