using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Threading.Tasks;

namespace Saro.BT.Designer
{
    public sealed class BTGraphEditor : EditorWindow
    {
        private const string k_EditorName = "BTGraphEditor";

        [MenuItem("Gameplay/" + k_EditorName)]
        public static BTGraphEditor Init()
        {
            var window = GetWindow<BTGraphEditor>();
            return window;
        }

        public static BehaviorTree CurretnTree { get; set; }

        private TemplateContainer m_Uxml;
        private BTGraphView m_BTGraphView;
        private IMGUIContainer m_NodeInspector;
        private IMGUIContainer m_GraphBlackboard;

        private CreateBTNodeProvider m_SearchProvider;
        private ChangeBTCompositeProvider m_ChangeBTCompositeNodeProvider;
        private ChangeBTTaskProvider m_ChangeBTTaskNodeProvider;
        private ChangeBTServiceProvider m_ChangeBTServiceNodeProvider;
        private ChangeBTDecoratorProvider m_ChangeBTDecoratorNodeProvider;
        private DecorateBTNodeProvider m_DecorateNodeProvider;

        public void CreateGUI()
        {
            try
            {
                __Create();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnEnable()
        {
            titleContent.text = $"{k_EditorName}";
            titleContent.image = BTEditorUtils.GetIcon("Run_Behaviour_24x");

            Selection.selectionChanged += SelectionChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= SelectionChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                m_NodeInspector.MarkDirtyRepaint();
            }
        }

        private void OnDestroy()
        {
            CurretnTree = null;
        }

        private void __Create()
        {
            m_ChangeBTCompositeNodeProvider = new(this);
            m_ChangeBTTaskNodeProvider = new(this);
            m_ChangeBTServiceNodeProvider = new(this);
            m_ChangeBTDecoratorNodeProvider = new(this);
            m_DecorateNodeProvider = new(this);

            VisualElement root = rootVisualElement;

            var visualTree = Resources.Load<VisualTreeAsset>("Stylesheets/BTGraphEditor");
            m_Uxml = visualTree.Instantiate();
            root.Add(m_Uxml);
            m_Uxml.StretchToParentSize();

            {
                m_BTGraphView = new BTGraphView();

                m_SearchProvider = new(m_BTGraphView, this);

                m_BTGraphView.nodeCreationRequest += context =>
                {
                    m_SearchProvider.ConnectedPort = null; // incase
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchProvider);
                };

                m_BTGraphView.OnNodeDecorateRequest += (node, evt) =>
                {
                    m_DecorateNodeProvider.Show(node, evt);
                };

                m_BTGraphView.OnNodeChangeRequest += (node, evt) =>
                {
                    switch (node)
                    {
                        case BTDecoratorNode:
                            m_ChangeBTDecoratorNodeProvider.Show(node, evt);
                            break;
                        case BTCompositeNode:
                            m_ChangeBTCompositeNodeProvider.Show(node, evt);
                            break;
                        case BTTaskNode:
                            m_ChangeBTTaskNodeProvider.Show(node, evt);
                            break;
                        case BTServiceNode:
                            m_ChangeBTServiceNodeProvider.Show(node, evt);
                            break;
                    }
                };

                m_BTGraphView.EdgeConnectorListener = new EdgeConnectorListener(m_BTGraphView, m_SearchProvider, this);

                m_Uxml.Q<VisualElement>("BTGraphView").Add(m_BTGraphView);

                m_BTGraphView.StretchToParentSize();
            }

            {

                var btnExport = m_Uxml.Q<Button>("BtnExport");
                btnExport.clicked += () =>
                {
                    if (CurretnTree != null)
                    {
                        // TODO save json
                        var json = BehaviorTree.ToJson(CurretnTree);
                        var path = AssetDatabase.GetAssetPath(CurretnTree) + ".json";
                        File.WriteAllText(path, json);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                };

                var btnSave = m_Uxml.Q<Button>("BtnSave");
                btnSave.clicked += () =>
                {
                    var result = m_BTGraphView.Save();
                    if (string.IsNullOrEmpty(result))
                    {
                        this.ShowNotification(new GUIContent("Successfully Save."), 1f);
                    }
                    else
                    {
                        this.ShowNotification(new GUIContent($"Error: {result}"), 1f);
                    }
                };

                var btnFormat = m_Uxml.Q<Button>("BtnFormat");
                btnFormat.clicked += () =>
                {
                    m_BTGraphView.Format();

                    this.ShowNotification(new GUIContent("Successfully Format."), 1f);
                };

                var toggleInspector = m_Uxml.Q<ToolbarToggle>("ToggleInspector");
                toggleInspector.RegisterValueChangedCallback((@event) =>
                {
                    m_Uxml.Q<VisualElement>("Docker").visible = @event.newValue;
                });

                toggleInspector.value = true;

                //var btGUIDs = AssetDatabase.FindAssets($"t:{nameof(BehaviorTree)}");
                //var bts = new List<BehaviorTree>();
                //foreach (var guid in btGUIDs)
                //{
                //    var bt = AssetDatabase.LoadAssetAtPath<BehaviorTree>(AssetDatabase.GUIDToAssetPath(guid));
                //    bts.Add(bt);
                //}

                //var searchBt = uxml.Q<ToolbarPopupSearchField>("SearchBT");

                //foreach (var tree in bts)
                //{
                //    searchBt.menu.AppendAction(tree.name, (a) =>
                //    {
                //        SetBehaviorTree(tree);
                //    });
                //}
            }

            {
                var inspector = ScriptableObject.CreateInstance<BTGraphInspector>();
                m_BTGraphView.OnNodeSelection += inspector.OnNodeSelection;
                m_NodeInspector = m_Uxml.Q<IMGUIContainer>("IMGUI_NodeInsepctor");
                m_NodeInspector.onGUIHandler = () =>
                {
                    inspector.OnInspectorGUI();
                };

                var graphBlackboard = ScriptableObject.CreateInstance<BTGraphBlackboard>();
                graphBlackboard.Initialize(m_BTGraphView);
                m_GraphBlackboard = m_Uxml.Q<IMGUIContainer>("IMGUI_Blackboard");
                m_GraphBlackboard.onGUIHandler = () =>
                {
                    graphBlackboard.OnInspectorGUI();
                };

                var splitView = new TwoPaneSplitView(0, 400f, TwoPaneSplitViewOrientation.Vertical);
                var docker = m_Uxml.Q<VisualElement>("Docker");
                splitView.Add(m_Uxml.Q<VisualElement>("BTNodeInspector"));
                splitView.Add(m_Uxml.Q<VisualElement>("BTBlackboard"));

                docker.Add(splitView);
            }
        }

        //public class BTGraphInspectorElement : GraphElement { }

        private async void SelectionChanged()
        {
            // TODO playmode 切换时，不能自动选择到 上一个资源
            // OnPlayModeStateChanged / Selection.selectionChanged 需要延迟一帧
            while (m_BTGraphView == null)
            {
                await Task.Yield();
            }

            BehaviorTree tree = null;

            if (Selection.activeObject is BehaviorTree _tree)
            {
                tree = _tree;
            }
            else if (Selection.activeObject is GameObject _go)
            {
                if (_go.TryGetComponent<TreeComponent>(out var treeComponent))
                {
                    tree = treeComponent.RuntimeTree != null ? treeComponent.RuntimeTree : treeComponent.TreeAsset;
                }
            }

            SetBehaviorTree(tree);
        }

        public void SetBehaviorTree(BehaviorTree tree)
        {
            CurretnTree = tree;
            m_BTGraphView.OnBehaviorTreeChanged(tree);

            if (CurretnTree == null)
            {
                rootVisualElement.visible = false;
                ShowNotification(new GUIContent("Please select a TreeAsset."));
            }
            else
            {
                rootVisualElement.visible = true;
                RemoveNotification();
            }

            m_Uxml.Q<Label>("TreeAsset").text = $"[{(tree == null ? "None" : tree.name)}]";
        }


        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            switch (playModeStateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    SelectionChanged();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    SelectionChanged();
                    break;
            }
        }

        /// <summary>
        /// Opens up the Bonsai window from asset selection.
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [OnOpenAsset(0)]
        static bool OpenCanvasAsset(int instanceID, int line)
        {
            var tree = EditorUtility.InstanceIDToObject(instanceID) as BehaviorTree;

            if (tree)
            {
                Init().SetBehaviorTree(tree);
                return true;
            }

            return false;
        }
    }
}