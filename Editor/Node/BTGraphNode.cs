using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Saro.SEditor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Saro.BT.Designer
{
    /*
     * TODO
     *
     * 1. 将BTRuntimeValue实时显示到运行时gui上
     *
     */
    public abstract partial class BTGraphNode : Node, IIterableNode<BTGraphNode>
    {
        public BTGraphView GraphView { get; internal set; }

        internal BTNode NodeBehavior { set; get; }

        private Label m_OrderLabel;
        private Image m_BreakpointIcon;

        public Port ParentPort { protected set; get; }

        private Label m_Description;
        private Image m_Icon;
        private Label m_TitleLabel;
        private IconBadge m_CommentBadge;

        /// <summary>
        /// UIE的 SetPosition 有延迟，不是立即生效的，这里做一个缓存
        /// <code>目前仅用于 <see cref="BTGraphFormatter"/>，其他地方慎重使用，不保证节点位置正确</code>
        /// </summary>
        internal Rect tempNodePosition;

        public BTGraphNode ParentNode
        {
            get
            {
                if (ParentPort != null && ParentPort.connected)
                {
                    return ParentPort.connections.First().output.node as BTGraphNode;
                }

                return null;
            }
        }

        protected BTGraphNode()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("Stylesheets/BTGraphNode"));
            AddToClassList("bold-text");

            AddTitle();

            AddParent();

            AddDescription();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new BTGraphDropdownMenuAction("Breakpoint", (a) =>
            {
                Debug.Assert(NodeBehavior != null);
                NodeBehavior.breakPoint = !NodeBehavior.breakPoint;
                RefreshBreakpoint();
            }));

            evt.menu.AppendSeparator();

            evt.menu.MenuItems().Add(new BTGraphDropdownMenuAction("Open Script", (a) =>
            {
                Debug.Assert(NodeBehavior != null);
                SEditorUtility.OpenScriptByType(NodeBehavior.GetType());
            }));

            evt.menu.AppendSeparator();

            evt.menu.MenuItems().Add(new BTGraphDropdownMenuAction("Decorate", (a) =>
            {
                GraphView.OnNodeDecorateRequest?.Invoke(this, evt);
            }, (DropdownMenuAction a) => !Application.isPlaying ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
        }

        protected virtual void AddTitle()
        {
            titleContainer.Clear();

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            titleContainer.Add(container);

            m_Icon = new Image();
            m_Icon.image = null;
            m_Icon.scaleMode = ScaleMode.ScaleToFit;
            m_Icon.style.marginRight = 5;
            container.Add(m_Icon);

            m_TitleLabel = new Label();
            m_TitleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            m_TitleLabel.style.fontSize = 14;
            container.Add(m_TitleLabel);

            m_OrderLabel = new Label() { style = { position = Position.Absolute, top = 0f, right = 0f } };
            m_OrderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            m_OrderLabel.style.fontSize = 12;
            titleContainer.Add(m_OrderLabel);

            m_BreakpointIcon = new Image() { style = { position = Position.Absolute, left = 0 } };
            m_BreakpointIcon.image = EditorGUIUtility.FindTexture("Invalid@2x");
            container.Add(m_BreakpointIcon);

            m_CommentBadge = IconBadge.CreateComment(string.Empty);
        }

        protected virtual void AddDescription()
        {
            m_Description = new Label();
            m_Description.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
            m_Description.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });
            mainContainer.Add(m_Description);
            m_Description.style.flexShrink = 1;
        }

        public virtual void PostInit(BTGraphView graphView)
        {
            GraphView = graphView;
        }

        public void SetBehavior(BTNode behavior)
        {
            NodeBehavior = behavior;
            NodeBehavior.StatusEditorNotify = MarkAsExecuted;

            NodeBehavior.Tree = GraphView.Tree;

            OnSetBehavior();

            RefreshBehavior();
        }

        internal void RefreshBehavior()
        {
            Debug.Assert(NodeBehavior != null, "NodeBehavior should not be null");

            NodeBehavior.OnValidate();

            var nodeAttribute = NodeBehavior.GetType().GetCustomAttribute<BTNodeAttribute>();
            if (nodeAttribute != null)
            {
                m_Icon.image = SEditorUtility.GetIcon(nodeAttribute.iconPath);
                titleContainer.tooltip = nodeAttribute.nodeDesc;
            }

            m_TitleLabel.text = NodeBehavior.Title;

            //RefreshPreOrder(NodeBehavior.preOrder);

            RefreshBreakpoint();

            RefreshCommentBadge();

            if (m_Description != null)
            {
                var sb = StringBuilderCache.Get(256);
                NodeBehavior.Description(sb);
                m_Description.text = StringBuilderCache.GetStringAndRelease(sb);
            }
        }

        private void RefreshCommentBadge()
        {
            if (m_CommentBadge != null)
            {
                m_CommentBadge.badgeText = NodeBehavior.comment;

                if (string.IsNullOrEmpty(NodeBehavior.comment))
                {
                    m_CommentBadge.Detach();
                    m_CommentBadge.RemoveFromHierarchy();
                }
                else
                {
                    Add(m_CommentBadge);
                    m_CommentBadge.AttachTo(titleContainer, SpriteAlignment.RightCenter);
                }
            }
        }

        internal void RefreshPreOrder(int preOrder)
        {
            m_OrderLabel.visible = preOrder != BTNode.k_InvalidPreOrder;
            m_OrderLabel.text = preOrder.ToString();
        }

        protected void RefreshBreakpoint()
        {
            m_BreakpointIcon.visible = NodeBehavior != null && NodeBehavior.breakPoint;
        }

        protected virtual void OnSetBehavior()
        {
            BBKeySelector.SetBBField(NodeBehavior, GraphView.Tree.blackboardData);
        }

        protected virtual void AddParent()
        {
            ParentPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Port));
            ParentPort.portName = "Parent";
            inputContainer.Add(ParentPort);
        }

        protected virtual Port CreateChildPort()
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(Port));
            port.portName = "Child";
            return port;
        }

        public override void OnSelected()
        {
            base.OnSelected();

            GraphView.OnNodeSelection?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();

            GraphView.OnNodeSelection?.Invoke(null);
        }

        public void Commit(Stack<BTGraphNode> stack)
        {
            OnCommit(stack);

            NodeBehavior.nodePosition = GetPosition();
            NodeBehavior.StatusEditorNotify = MarkAsExecuted;

            RefreshBehavior();
        }

        protected abstract void OnCommit(Stack<BTGraphNode> stack);

        public string Validate(Stack<BTGraphNode> stack)
        {
            if (NodeBehavior == null)
            {
                return $"{m_TitleLabel.text}'s NodeBehavior is null";
            }

            var result = OnValidate(stack);
            if (string.IsNullOrEmpty(result))
            {
                NodeBehavior.OnValidate();

                style.backgroundColor = new StyleColor(StyleKeyword.Null);
            }

            return result;
        }

        protected abstract string OnValidate(Stack<BTGraphNode> stack);

        private void MarkAsExecuted(BTNode.EStatusEditor status)
        {
            switch (status)
            {
                case BTNode.EStatusEditor.Failure:
                    SetBorderColor(Color.red);
                    break;
                case BTNode.EStatusEditor.Running:
                    SetBorderColor(Color.yellow);
                    break;
                case BTNode.EStatusEditor.Success:
                    SetBorderColor(Color.green);
                    break;
                case BTNode.EStatusEditor.None:
                    SetBorderColor(Color.clear);
                    break;
                case BTNode.EStatusEditor.Aborted:
                    SetBorderColor(new Color(1, 0, 0.76f));
                    break;
                case BTNode.EStatusEditor.Interruption:
                    SetBorderColor(Color.blue);
                    break;
            }
        }

        internal void SetBorderColor(Color color)
        {
            var border = this.Q<VisualElement>("node-border");
            var borderStyle = border.style;
            borderStyle.borderBottomColor
                = borderStyle.borderTopColor
                = borderStyle.borderRightColor
                = borderStyle.borderLeftColor
                = color;
        }

        public void ClearStyle()
        {
            style.backgroundColor = new StyleColor(StyleKeyword.Null);
            OnClearStyle();
        }

        protected abstract void OnClearStyle();

        public virtual void OnNodeInspectorGUI()
        {
            if (NodeBehavior != null)
            {
                SEditorUtility.ShowAutoEditorGUI(NodeBehavior);
            }
        }

        public abstract BTGraphNode GetChildAt(int childIndex);

        public void SetChildAt(BTGraphNode node, int childIndex) => throw new NotSupportedException("SetChildAt is not supported.");

        public abstract int ChildCount();

        public override string ToString()
        {
            return $"{NodeBehavior.GetType().Name}";
        }
    }
}