
using System;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    public enum EBlackBoardEntryComparison
    {
        Equal,
        NotEqual,
    }

    [BTNode("Compare_Blackboard_Entries_24x")]
    public class CompareBBEntries : BTDecorator
    {
        public EBlackBoardEntryComparison op;

        public BBKeySelector bbKeyA = new();

        public BBKeySelector bbKeyB = new();

        public override bool Condition()
        {
            var bb = Blackboard;

            var valueA = bb.GetVariable(bbKeyA);
            var valueB = bb.GetVariable(bbKeyB);

            return valueA.Compare(valueB);
        }

        private Action m_OnBlackboardChanged;

        public override void OnInitialize()
        {
            base.OnInitialize();

            m_OnBlackboardChanged = () =>
            {
                Evaluate();
            };
        }

        public override void OnReset()
        {
            base.OnReset();

            m_OnBlackboardChanged = null;
        }

        public override void OnObserverBegin()
        {
            Blackboard.RegisterChangeEvent(bbKeyA, m_OnBlackboardChanged);
            Blackboard.RegisterChangeEvent(bbKeyB, m_OnBlackboardChanged);
        }

        public override void OnObserverEnd()
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
