using System.Text;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Saro.BT
{
    /*
     * TODO
     * 
     * 1. 看看有什么限制，例如 支不支持 装饰器打断
     * 
     */

    [BTNode("Run_Behaviour_24x", "“运行”")]
    public sealed class RunBehaviour : BTTask
    {
        [HideInInspector]
        public string subtreeAsset;

#if UNITY_EDITOR
        [Tooltip("The sub-tree to run when this task executes.")]
        [SerializeField]
        internal BehaviorTree m_SubTreeAsset;
#endif

        private BehaviorTree m_RunningSubTree;

        public override BTNode Clone()
        {
            var newNode = new RunBehaviour();
            newNode.subtreeAsset = subtreeAsset;
            return newNode;
        }

        protected internal override void OnValidate()
        {
            base.OnValidate();

#if UNITY_EDITOR
            subtreeAsset = m_SubTreeAsset == null ? null : m_SubTreeAsset.name;
#endif
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            // TODO
            //if (m_RunningSubTree == null && subtreeAsset)
            //{
            //    m_RunningSubTree = BehaviorTree.Clone(subtreeAsset);
            //    m_RunningSubTree.actor = Actor;
            //    m_RunningSubTree.Initialize();
            //}
        }

        public override void OnReset()
        {
            base.OnReset();

            if (m_RunningSubTree != null)
            {
                m_RunningSubTree.ResetData();
            }
        }

        public override void OnEnter()
        {
            if(m_RunningSubTree == null)
            {
                m_RunningSubTree = BehaviorTree.CetOrCreateRuntimeTree(subtreeAsset, Actor);
            }

            m_RunningSubTree.BeginTraversal();
        }

        public override void OnExit()
        {
            if (m_RunningSubTree.IsRunning())
            {
                m_RunningSubTree.Interrupt();
            }
        }

        public override EStatus OnExecute(float deltaTime)
        {
            if (m_RunningSubTree != null)
            {
                m_RunningSubTree.Tick(deltaTime);
                return m_RunningSubTree.IsRunning()
                  ? EStatus.Running
                  : m_RunningSubTree.LastStatus();
            }

            // No tree was included. Just fail.
            return EStatus.Failure;
        }

        public override void Description(StringBuilder builder)
        {
            if (subtreeAsset != null)
            {
                builder.AppendFormat("Run {0}", subtreeAsset);
            }
            else
            {
                builder.Append("Tree not set");
            }
        }
    }
}