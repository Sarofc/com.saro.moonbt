using UnityEngine;
using System.Collections;
using Saro.BT;

public class BBCondtionExample : MonoBehaviour
{
    public class TestType { }

    private Root m_Root;
    private float m_Time = 10f;

    void Start()
    {
        var bb = UBTContext.Instance.GetGlobalBlackboard("DefaultBB");
        var sut = new BlackboardCondition<TestType>("Key1", Operator.IS_SET).Decorate(new Log("yahaha"));

        m_Root = new Root(bb).Decorate(sut);

#if UNITY_EDITOR
        UBTDebugger debugger = gameObject.AddComponent<UBTDebugger>();
        debugger.behaviorTree = m_Root;
#endif
        //m_Root.RepeatRoot = false;
        m_Root.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Root.Blackboard.Set("Key1", new TestType());
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            m_Root.Blackboard.UnSet("Key1");
        }
    }
}
