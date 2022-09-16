using System;
using System.Text;

namespace Saro.BT
{
    [BTNode("Blackboard_24x", "‘黑板条件’节点\n节点将判断黑板键的值与给定值之间的关系，根据结果（可以是大于、小于、等于，等等）阻止或允许节点的执行。")]
    public class BBCondition : BTDecorator
    {
        public BBKeySelector bbKey = new();

        [KeyOperation]
        public byte keyOperation;

#if UNITY_EDITOR
        [ShowIf(nameof(GetOpType), typeof(BBKey_Float))]
#endif
        public float compareFloat;
#if UNITY_EDITOR
        [ShowIf(nameof(GetOpType), typeof(BBKey_Int))]
#endif
        public int compareInt;
#if UNITY_EDITOR
        [ShowIf(nameof(GetOpType), typeof(BBKey_String))]
#endif
        public string compareText;

        private Action m_OnBlackboardChanged;

        public override void OnInitialize()
        {
            base.OnInitialize();

            m_OnBlackboardChanged = Evaluate;
        }

        public override void OnReset()
        {
            base.OnReset();

            m_OnBlackboardChanged = null;
        }

        public override bool Condition()
        {
            // 这俩可以提前 初始化
            if (Blackboard == null) return false;

            var entry = Blackboard.GetKeyEntryByName(bbKey);
            var keyIndex = Blackboard.GetKeyIndexByName(bbKey);

            var keyType = entry.keyType;

            return keyType switch
            {
                BBKey_Bool bbkey => bbkey.TestOperation(Blackboard.GetValue<bool>(keyIndex), keyOperation),
                BBKey_Float bbkey => bbkey.TestOperation(Blackboard.GetValue<float>(keyIndex), keyOperation, compareFloat),
                BBKey_Int bbkey => bbkey.TestOperation(Blackboard.GetValue<int>(keyIndex), keyOperation, compareInt),
                BBKey_String bbkey => bbkey.TestOperation(Blackboard.GetValue<string>(keyIndex), keyOperation, compareText),
                BBKey_Object bbkey => bbkey.TestOperation(Blackboard.GetValue<object>(keyIndex), keyOperation),
                _ => false,
            };
        }

        public override void OnObserverBegin()
        {
            if (Blackboard == null) return;

            Blackboard.RegisterChangeEvent(bbKey, m_OnBlackboardChanged);
        }

        public override void OnObserverEnd()
        {
            if (Blackboard == null) return;

            Blackboard.UnregisterChangeEvent(bbKey, m_OnBlackboardChanged);
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

#if UNITY_EDITOR

            var keyType = bbKey.GetBlackboardKeyType();
            if (keyType == null)
            {
                builder.Append($"entry {bbKey} is not found");
                return;
            }

            var keyIndex = bbKey.data.GetKeyIndexByName(bbKey);

            builder.Append("'")
                .Append(bbKey)
                .Append("'")
                .Append("(")
                .Append(bbKey.GetBlackboardKeyType()?.GetValueType().Name)
                .Append(")");

            builder.AppendLine();

            switch (keyType)
            {
                case BBKey_Bool _bool:
                case BBKey_Object _object:
                    {
                        var keyOp = (EBasicKeyOperation)keyOperation;
                        builder.Append(" is ");
                        builder.Append(keyOp);
                    }
                    break;
                case BBKey_Float _flaot:
                    {
                        var keyOp = (EArithmeticKeyOperation)keyOperation;
                        builder.Append(" ");
                        builder.Append(keyOp);
                        builder.Append(" ");
                        builder.Append(compareFloat);
                    }
                    break;
                case BBKey_Int _int:
                    {
                        var keyOp = (EArithmeticKeyOperation)keyOperation;
                        builder.Append(" ");
                        builder.Append(keyOp);
                        builder.Append(" ");
                        builder.Append(compareInt);
                    }
                    break;
                case BBKey_String _string:
                    {
                        var keyOp = (ETextKeyOperation)keyOperation;
                        builder.Append(" ");
                        builder.Append(keyOp);
                        builder.Append(" ");
                        builder.Append(compareText);
                    }
                    break;
            }
#endif
        }

#if UNITY_EDITOR
        private Type GetOpType
        {
            get
            {
                var keyType = bbKey.GetBlackboardKeyType();
                if (keyType == null)
                {
                    return null;
                }

                return keyType switch
                {
                    BBKey_Bool bbkey => typeof(BBKey_Bool),
                    BBKey_Float bbkey => typeof(BBKey_Float),
                    BBKey_Int bbkey => typeof(BBKey_Int),
                    BBKey_String bbkey => typeof(BBKey_String),
                    BBKey_Object bbkey => typeof(BBKey_Object),
                    _ => null,
                };
            }
        }
#endif
    }
}