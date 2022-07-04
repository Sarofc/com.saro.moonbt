
using System;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    [BTNode("Blackboard_24x")]
    public class BBCondition : BTDecorator
    {
        public BBKeySelector bbKey = new();

        [KeyOperation]
        public byte keyOperation;

#if UNITY_EDITOR
        [ShowIf(nameof(GetOpType), typeof(BBKeyType_Float))]
#endif
        public float compareFloat;
#if UNITY_EDITOR
        [ShowIf(nameof(GetOpType), typeof(BBKeyType_Int))]
#endif
        public int compareInt;
#if UNITY_EDITOR
        [ShowIf(nameof(GetOpType), typeof(BBKeyType_String))]
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
                BBKeyType_Bool bbkey => bbkey.TestOperation(Blackboard.GetValue<bool>(keyIndex), keyOperation),
                BBKeyType_Float bbkey => bbkey.TestOperation(Blackboard.GetValue<float>(keyIndex), keyOperation, compareFloat),
                BBKeyType_Int bbkey => bbkey.TestOperation(Blackboard.GetValue<int>(keyIndex), keyOperation, compareInt),
                BBKeyType_String bbkey => bbkey.TestOperation(Blackboard.GetValue<string>(keyIndex), keyOperation, compareText),
                BBKeyType_Object bbkey => bbkey.TestOperation(Blackboard.GetValue<object>(keyIndex), keyOperation),
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
                case BBKeyType_Bool _bool:
                case BBKeyType_Object _object:
                    {
                        var keyOp = (EBasicKeyOperation)keyOperation;
                        builder.Append(" is ");
                        builder.Append(keyOp);
                    }
                    break;
                case BBKeyType_Float _flaot:
                    {
                        var keyOp = (EArithmeticKeyOperation)keyOperation;
                        builder.Append(" ");
                        builder.Append(keyOp);
                        builder.Append(" ");
                        builder.Append(compareFloat);
                    }
                    break;
                case BBKeyType_Int _int:
                    {
                        var keyOp = (EArithmeticKeyOperation)keyOperation;
                        builder.Append(" ");
                        builder.Append(keyOp);
                        builder.Append(" ");
                        builder.Append(compareInt);
                    }
                    break;
                case BBKeyType_String _string:
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
                    BBKeyType_Bool bbkey => typeof(BBKeyType_Bool),
                    BBKeyType_Float bbkey => typeof(BBKeyType_Float),
                    BBKeyType_Int bbkey => typeof(BBKeyType_Int),
                    BBKeyType_String bbkey => typeof(BBKeyType_String),
                    BBKeyType_Object bbkey => typeof(BBKeyType_Object),
                    _ => null,
                };
            }
        }
#endif
    }
}