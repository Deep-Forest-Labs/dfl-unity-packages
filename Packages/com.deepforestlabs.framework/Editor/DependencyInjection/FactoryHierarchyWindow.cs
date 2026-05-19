#nullable enable
using System;
using System.Collections.Generic;
using DeepForestLabs.Data;
using DeepForestLabs.Factories;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeepForestLabs.EditorTools
{
    public sealed class FactoryHierarchyWindow : EditorWindow
    {
        private const string MenuPath = "Deep Forest Labs/Tools/Factory Hierarchy";

        [SerializeField] private TreeViewState _treeState = default!;
        [SerializeField] private string _searchString = "";

        private FactoryTreeView? _treeView;
        private SearchField? _searchField;
        private List<FactoryNode>? _roots;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            var win = GetWindow<FactoryHierarchyWindow>();
            win.titleContent = new GUIContent("Factory Hierarchy",
                EditorGUIUtility.IconContent("d_ScriptableObject Icon").image);
            win.minSize = new Vector2(380f, 300f);
        }

        private void OnEnable()
        {
            _treeState ??= new TreeViewState();
            Rebuild();
        }

        private void OnFocus()
        {
            Rebuild();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_treeView == null)
            {
                EditorGUILayout.HelpBox("No factory hierarchy found. Ensure a MainArgs asset exists in a Resources folder.", MessageType.Info);
                return;
            }

            Rect remaining = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _treeView.OnGUI(remaining);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                Rebuild();

            if (GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.Width(72f)))
                _treeView?.ExpandAll();

            if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                _treeView?.CollapseAll();

            GUILayout.FlexibleSpace();

            _searchField ??= new SearchField();
            string newSearch = _searchField.OnToolbarGUI(_searchString, GUILayout.Width(200f));
            if (newSearch != _searchString)
            {
                _searchString = newSearch;
                if (_treeView != null)
                    _treeView.searchString = _searchString;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void Rebuild()
        {
            _roots = FactoryTreeBuilder.Build();
            _treeView = new FactoryTreeView(_treeState, _roots);
            _treeView.searchString = _searchString;
            _treeView.ExpandAll();
        }
    }

    internal sealed class FactoryNode
    {
        public string DisplayName;
        public string FieldLabel;
        public Object? Asset;
        public FactoryNodeKind Kind;
        public List<FactoryNode> Children = new();

        public FactoryNode(string displayName, string fieldLabel, Object? asset, FactoryNodeKind kind)
        {
            DisplayName = displayName;
            FieldLabel = fieldLabel;
            Asset = asset;
            Kind = kind;
        }
    }

    internal enum FactoryNodeKind
    {
        ContainerBuilderFactory,
        ContainerFactory,
        MvcFactory,
    }

    internal static class FactoryTreeBuilder
    {
        private static readonly Type ContainerBuilderFactoryType = typeof(ContainerBuilderFactory);
        private static readonly Type ContainerFactoryType = typeof(ContainerFactory);

        public static List<FactoryNode> Build()
        {
            var roots = new List<FactoryNode>();
            var visited = new HashSet<int>();

            var mainArgs = FindMainArgs();
            if (mainArgs != null)
            {
                var node = CreateNode(mainArgs, "Root", visited);
                if (node != null)
                    roots.Add(node);
            }

            if (roots.Count == 0)
            {
                BuildFromAllFactories(roots, visited);
            }

            return roots;
        }

        private static MainArgs? FindMainArgs()
        {
            string[] guids = AssetDatabase.FindAssets("t:MainArgs");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<MainArgs>(path);
                if (asset != null)
                    return asset;
            }
            return null;
        }

        private static void BuildFromAllFactories(List<FactoryNode> roots, HashSet<int> visited)
        {
            var allFactories = new List<ValidatedData>();
            var referenced = new HashSet<int>();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ValidatedData>(path);
                if (asset != null && IsFactory(asset))
                    allFactories.Add(asset);
            }

            foreach (var factory in allFactories)
            {
                CollectReferencedFactoryIds(factory, referenced);
            }

            foreach (var factory in allFactories)
            {
                if (!referenced.Contains(factory.GetInstanceID()))
                {
                    var node = CreateNode(factory, "Root", visited);
                    if (node != null)
                        roots.Add(node);
                }
            }
        }

        private static void CollectReferencedFactoryIds(Object factory, HashSet<int> referenced)
        {
            var so = new SerializedObject(factory);
            var iterator = so.GetIterator();
            if (!iterator.NextVisible(true)) return;

            do
            {
                CollectFromProperty(iterator, referenced);
            } while (iterator.NextVisible(false));
        }

        private static void CollectFromProperty(SerializedProperty prop, HashSet<int> referenced)
        {
            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                var obj = prop.objectReferenceValue;
                if (obj != null && IsFactory(obj))
                    referenced.Add(obj.GetInstanceID());
            }
            else if (prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
            {
                for (int i = 0; i < prop.arraySize; i++)
                {
                    var elem = prop.GetArrayElementAtIndex(i);
                    if (elem.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var obj = elem.objectReferenceValue;
                        if (obj != null && IsFactory(obj))
                            referenced.Add(obj.GetInstanceID());
                    }
                }
            }
        }

        private static FactoryNode? CreateNode(Object asset, string fieldLabel, HashSet<int> visited)
        {
            int id = asset.GetInstanceID();
            if (!visited.Add(id))
                return null;

            var kind = ClassifyFactory(asset);
            string displayName = asset.GetType().Name;
            var node = new FactoryNode(displayName, fieldLabel, asset, kind);

            var so = new SerializedObject(asset);
            var iterator = so.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script") continue;
                    AddChildrenFromProperty(iterator, node, visited);
                } while (iterator.NextVisible(false));
            }

            return node;
        }

        private static void AddChildrenFromProperty(SerializedProperty prop, FactoryNode parent, HashSet<int> visited)
        {
            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                var obj = prop.objectReferenceValue;
                if (obj != null && IsFactory(obj))
                {
                    var child = CreateNode(obj, prop.displayName, visited);
                    if (child != null)
                        parent.Children.Add(child);
                }
            }
            else if (prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
            {
                for (int i = 0; i < prop.arraySize; i++)
                {
                    var elem = prop.GetArrayElementAtIndex(i);
                    if (elem.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var obj = elem.objectReferenceValue;
                        if (obj != null && IsFactory(obj))
                        {
                            string label = $"{prop.displayName}[{i}]";
                            var child = CreateNode(obj, label, visited);
                            if (child != null)
                                parent.Children.Add(child);
                        }
                    }
                }
            }
            else if (IsAssetRefProperty(prop))
            {
                TryResolveAssetRef(prop, parent, visited);
            }
        }

        private static bool IsAssetRefProperty(SerializedProperty prop)
        {
            if (prop.propertyType != SerializedPropertyType.Generic) return false;
            return prop.type.Contains("AssetRef");
        }

        private static void TryResolveAssetRef(SerializedProperty prop, FactoryNode parent, HashSet<int> visited)
        {
            var guidProp = prop.FindPropertyRelative("_guid");
            var modeProp = prop.FindPropertyRelative("_mode");
            var pathProp = prop.FindPropertyRelative("_resourcesPath");

            Object? resolved = null;

            if (modeProp != null && modeProp.intValue == (int)AssetRefMode.Addressables
                && guidProp != null && !string.IsNullOrEmpty(guidProp.stringValue))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guidProp.stringValue);
                if (!string.IsNullOrEmpty(assetPath))
                    resolved = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            }
            else if (modeProp != null && modeProp.intValue == (int)AssetRefMode.Resources
                     && pathProp != null && !string.IsNullOrEmpty(pathProp.stringValue))
            {
                resolved = Resources.Load<ScriptableObject>(pathProp.stringValue);
            }

            if (resolved != null && IsFactory(resolved))
            {
                var child = CreateNode(resolved, prop.displayName, visited);
                if (child != null)
                    parent.Children.Add(child);
            }
        }

        private static bool IsFactory(Object obj)
        {
            Type t = obj.GetType();
            return ContainerBuilderFactoryType.IsAssignableFrom(t)
                   || ContainerFactoryType.IsAssignableFrom(t);
        }

        private static FactoryNodeKind ClassifyFactory(Object obj)
        {
            Type t = obj.GetType();
            if (ContainerBuilderFactoryType.IsAssignableFrom(t))
                return FactoryNodeKind.ContainerBuilderFactory;

            if (IsMvcFactory(t))
                return FactoryNodeKind.MvcFactory;

            return FactoryNodeKind.ContainerFactory;
        }

        private static bool IsMvcFactory(Type t)
        {
            Type? current = t;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    Type def = current.GetGenericTypeDefinition();
                    if (def.Name.StartsWith("Factory`"))
                        return true;
                }
                current = current.BaseType;
            }
            return false;
        }
    }

    internal sealed class FactoryTreeView : TreeView
    {
        private readonly List<FactoryNode> _roots;
        private readonly Dictionary<int, FactoryNode> _nodeMap = new();

        private static GUIStyle? _fieldLabelStyle;
        private static GUIStyle FieldLabelStyle =>
            _fieldLabelStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) },
                alignment = TextAnchor.MiddleLeft,
            };

        public FactoryTreeView(TreeViewState state, List<FactoryNode> roots)
            : base(state)
        {
            _roots = roots;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1, "Root");
            _nodeMap.Clear();

            if (_roots.Count == 0)
            {
                root.AddChild(new TreeViewItem(0, 0, "(No factories found)"));
                return root;
            }

            int nextId = 0;
            foreach (var factoryRoot in _roots)
            {
                AddNode(root, factoryRoot, 0, ref nextId);
            }

            if (!root.hasChildren)
                root.AddChild(new TreeViewItem(0, 0, "(Empty)"));

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        private void AddNode(TreeViewItem parent, FactoryNode node, int depth, ref int nextId)
        {
            int id = nextId++;
            _nodeMap[id] = node;

            string label = node.DisplayName;
            var item = new TreeViewItem(id, depth, label);
            parent.AddChild(item);

            foreach (var child in node.Children)
            {
                AddNode(item, child, depth + 1, ref nextId);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            if (!_nodeMap.TryGetValue(args.item.id, out var node))
                return;

            float indent = GetContentIndent(args.item);
            float labelWidth = EditorStyles.label.CalcSize(new GUIContent(args.item.displayName)).x;
            float x = args.rowRect.x + indent + labelWidth + 20f;
            float availableWidth = args.rowRect.xMax - x - 4f;

            if (availableWidth > 30f && !string.IsNullOrEmpty(node.FieldLabel) && node.FieldLabel != "Root")
            {
                string kindTag = node.Kind switch
                {
                    FactoryNodeKind.ContainerBuilderFactory => "[Scope]",
                    FactoryNodeKind.ContainerFactory => "[Composite]",
                    FactoryNodeKind.MvcFactory => "[MVC]",
                    _ => "",
                };

                string suffix = string.IsNullOrEmpty(kindTag)
                    ? node.FieldLabel
                    : $"{kindTag}  {node.FieldLabel}";

                var rect = new Rect(x, args.rowRect.y, availableWidth, args.rowRect.height);
                GUI.Label(rect, suffix, FieldLabelStyle);
            }
            else if (availableWidth > 30f)
            {
                string kindTag = node.Kind switch
                {
                    FactoryNodeKind.ContainerBuilderFactory => "[Scope]",
                    FactoryNodeKind.ContainerFactory => "[Composite]",
                    FactoryNodeKind.MvcFactory => "[MVC]",
                    _ => "",
                };

                if (!string.IsNullOrEmpty(kindTag))
                {
                    var rect = new Rect(x, args.rowRect.y, availableWidth, args.rowRect.height);
                    GUI.Label(rect, kindTag, FieldLabelStyle);
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 0) return;

            if (_nodeMap.TryGetValue(selectedIds[0], out var node) && node.Asset != null)
            {
                Selection.activeObject = node.Asset;
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (_nodeMap.TryGetValue(id, out var node) && node.Asset != null)
            {
                EditorGUIUtility.PingObject(node.Asset);
                Selection.activeObject = node.Asset;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (_nodeMap.TryGetValue(item.id, out var node))
            {
                return node.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                       || node.FieldLabel.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            return base.DoesItemMatchSearch(item, search);
        }
    }
}
#nullable disable
