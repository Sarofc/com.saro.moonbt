
using System;
using System.Globalization;
using System.Text;

namespace Saro.BT
{
    public enum EBlackBoardEntryComparison
    {
        Equal,
        NotEqual,
    }

    [BTNode("Compare_Blackboard_Entries_24x", "‘比较黑板键值’节点\n节点将比较两个黑板键的值，并根据结果（等于或不等）阻止或允许节点的执行。 ")]
    public class CompareBBEntries : BTDecorator
    {
        public EBlackBoardEntryComparison op;
        public BBKeySelector bbKeyA = new();
        public BBKeySelector bbKeyB = new();

        private Action m_OnBlackboardChanged;

        public CompareBBEntries()
        {
            m_OnBlackboardChanged = Evaluate;
        }

        protected override void OnDecoratorEnter()
        {
            if (abortType != EAbortType.None)
            {
                ObserverBegin();
            }

            if (EvaluateCondition())
            {
                RunChild();
            }
        }

        protected override void OnDecoratorExit()
        {
            if (abortType == EAbortType.None || abortType == EAbortType.Self)
            {
                ObserverEnd();
            }
        }

        protected override bool EvaluateCondition()
        {
            var bb = Blackboard;

            var valueA = bb.GetVariable(bbKeyA);
            var valueB = bb.GetVariable(bbKeyB);

            return valueA.Compare(valueB);
        }

        protected override void OnObserverBegin()
        {
            Blackboard.RegisterChangeEvent(bbKeyA, m_OnBlackboardChanged);
            Blackboard.RegisterChangeEvent(bbKeyB, m_OnBlackboardChanged);
        }

        protected override void OnObserverEnd()
        {
            Blackboard.UnregisterChangeEvent(bbKeyA, m_OnBlackboardChanged);
            Blackboard.UnregisterChangeEvent(bbKeyB, m_OnBlackboardChanged);
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            var compareString = $"{bbKeyA.keyName} {op} {bbKeyB.keyName}";

            int maxCharacters = 36;
            if (compareString.Length > maxCharacters)
            {
                builder.Append(compareString[..maxCharacters]);
                builder.Append("...");
            }
            else
            {
                builder.Append(compareString);
            }
        }
    }
}
