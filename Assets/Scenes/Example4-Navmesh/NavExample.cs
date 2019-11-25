using UnityEngine;
using System.Collections;
using Saro.BT;
using System;
using UnityEngine.AI;

public class NavExample : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;

    private Root m_Root;
    private float m_Time = 10f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();


        var bb = UBTContext.Instance.GetGlobalBlackboard("DefaultBB");
        //bb.Set<Transform>("target", target);

        var idleSeq = new Sequence().OpenBranch(
            new Log("idle")
        );

        var chaseSelector = new Selector().OpenBranch(
            new Condition(() => (target.position - transform.position).sqrMagnitude >= .1f * .1f).Decorate(new MoveTo(agent, "target")),
            new Log("wait")
        );

        var seekCondition = new BlackboardCondition<Transform>("target", Operator.IS_NOT_SET, null, ObserverAborts.BOTH).Decorate(idleSeq);

        var selector = new Selector().OpenBranch(
            seekCondition,
            chaseSelector
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
        if (target != null)
        {
            if ((target.position - transform.position).sqrMagnitude >= 5f * 5f)
            {
                m_Root.Blackboard.UnSet("target");
            }
            else
            {
                m_Root.Blackboard.Set("target", target);
            }
        }
    }

    //private void Update()
    //{
    //    m_Time -= Time.deltaTime;
    //}
}
