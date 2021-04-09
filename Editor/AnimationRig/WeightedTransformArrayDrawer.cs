using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Animations.Rigging
{
    [CustomPropertyDrawer(typeof(WeightedTransformArray))]
    sealed class WeightedTransformArrayDrawer : PropertyDrawer
    {
        const string k_LengthPath = "m_Length";
        const string k_ItemTransformPath = nameof(WeightedTransform.transform);
        const string k_ItemWeightPath = nameof(WeightedTransform.weight);
        static readonly string[] k_ItemPropertyPaths =
            Enumerable.Range(0, WeightedTransformArray.k_MaxLength).Select(i => $"m_Item{i}").ToArray();

        // one reorderable list per unique property path
        readonly Dictionary<string, (ReorderableList listControl, SerializedProperty lengthProperty)> m_GUIState =
            new Dictionary<string, (ReorderableList listControl, SerializedProperty lengthProperty)>();

        // function to get WeightedTransform from item property
        static WeightedTransform GetValueSingle(SerializedProperty item) => new WeightedTransform
        {
            transform = item.FindPropertyRelative(k_ItemTransformPath).objectReferenceValue as Transform,
            weight = item.FindPropertyRelative(k_ItemWeightPath).floatValue
        };

        // function to modify a list of items per target
        static void ModifyItemsSingle(SerializedProperty parentProperty, Action<List<WeightedTransform>> modifyList)
        {
            foreach (var target in parentProperty.serializedObject.targetObjects)
            {
                using (var so = new SerializedObject(target))
                {
                    var sp = so.FindProperty(parentProperty.propertyPath);
                    var length = sp.FindPropertyRelative(k_LengthPath);

                    // create a live list of items
                    var items =
                        Enumerable.Range(0, length.intValue)
                        .Select(i => GetValueSingle(sp.FindPropertyRelative(k_ItemPropertyPaths[i])))
                        .ToList();

                    // modify the list
                    modifyList(items);

                    // write the results back to the serialized data stream
                    for (var i = 0; i < items.Count; ++i)
                    {
                        var item = sp.FindPropertyRelative(k_ItemPropertyPaths[i]);
                        item.FindPropertyRelative(k_ItemTransformPath).objectReferenceValue = items[i].transform;
                        item.FindPropertyRelative(k_ItemWeightPath).floatValue = items[i].weight;
                    }
                    // clear other items
                    for (var i = items.Count; i < WeightedTransformArray.k_MaxLength; ++i)
                    {
                        var item = sp.FindPropertyRelative(k_ItemPropertyPaths[i]);
                        item.FindPropertyRelative(k_ItemTransformPath).objectReferenceValue = default;
                        item.FindPropertyRelative(k_ItemWeightPath).floatValue = default;
                    }

                    // synchronize length property
                    length.intValue = items.Count;

                    // write back results
                    so.ApplyModifiedProperties();
                }
            }
            // update parent property's serialized data stream to get new (shared) values for all targets
            parentProperty.serializedObject.Update();
        }

        (ReorderableList listControl, SerializedProperty lengthProperty) GetGUIState(SerializedProperty property, GUIContent label)
        {
            var lengthProperty = property.FindPropertyRelative(k_LengthPath);

            // create a new reorderable list if one does not exist
            if (!m_GUIState.TryGetValue(property.propertyPath, out var guiState))
            {
                // bind the control to a proxy list
                var proxy = Enumerable.Range(0, lengthProperty.intValue)
                    .ToList();
                var reorderableList = new ReorderableList(proxy, typeof(int));

                reorderableList.headerHeight = Styles.minHeaderHeight;

                // default array control only allocates single line height, but that leaves no spacing between object fields
                reorderableList.elementHeight =
                    EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var attr = fieldInfo.GetCustomAttribute<WeightRangeAttribute>();
                var legacyAttr = fieldInfo.GetCustomAttribute<RangeAttribute>();
                var min = attr?.min ?? legacyAttr?.min ?? float.NaN;
                var max = attr?.max ?? legacyAttr?.max ?? float.NaN;
                var spacing = EditorGUIUtility.standardVerticalSpacing / 2f;
                reorderableList.drawElementCallback += (rect, index, active, focused) =>
                {
                    rect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight, y = rect.y + spacing };
                    WeightedTransformDrawer.DoGUI(rect, property.FindPropertyRelative(k_ItemPropertyPaths[index]), min, max);
                };

                reorderableList.onCanAddCallback += list =>
                    !Application.isPlaying
                    && !AnimationMode.InAnimationMode()
                    && lengthProperty.intValue < WeightedTransformArray.k_MaxLength;

                reorderableList.onCanRemoveCallback += list =>
                    !Application.isPlaying
                    && !AnimationMode.InAnimationMode()
                    && lengthProperty.intValue > 0;

                reorderableList.onAddCallback += list =>
                {
                    ModifyItemsSingle(property, items =>
                    {
                        int insertIndex = Math.Max(0, reorderableList.index >= 0 ? reorderableList.index : items.Count - 1);
                        if (items.Count < WeightedTransformArray.k_MaxLength)
                            items.Insert(insertIndex, insertIndex < items.Count ? items[insertIndex] : WeightedTransform.Default(1f));
                    });
                    proxy.Add(proxy.Count);
                };

                reorderableList.onRemoveCallback += list =>
                {
                    ModifyItemsSingle(property, items =>
                    {
                        int removeIndex = Math.Max(0, reorderableList.index >= 0 ? reorderableList.index : items.Count - 1);
                        if (removeIndex >= 0)
                            items.RemoveAt(removeIndex);
                    });
                    proxy.RemoveAt(proxy.Count - 1);
                };

                reorderableList.onReorderCallbackWithDetails += (list, srcIndex, dstIndex) =>
                    ModifyItemsSingle(property, items =>
                    {
                        var moved = items[srcIndex];
                        items.RemoveAt(srcIndex);
                        items.Insert(dstIndex, moved);
                    });

                guiState = m_GUIState[property.propertyPath] = (reorderableList, lengthProperty.Copy());
            }

            // synchronize proxy list to serialized length
            var proxyList = guiState.listControl.list;
            while (proxyList.Count < lengthProperty.intValue)
                proxyList.Add(proxyList.Count);
            while (proxyList.Count > lengthProperty.intValue)
                proxyList.RemoveAt(proxyList.Count - 1);

            return guiState;
        }

        static class Styles
        {
            // cf. ReorderableList.Defaults.minHeaderHeight;
            public static float minHeaderHeight = 2f;
            // cf. ReorderableListWrapper.cs
            public const float headerPadding = 3f;
            public const float arraySizeWidth = 50f; // 48 in ReorderableListWrapper, but EditorGUI.Slider() field is 50
            public const float defaultFoldoutHeaderHeight = 18f;
            public static readonly GUIContent sizeLabel = EditorGUIUtility.TrTextContent("", "Length");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            Styles.defaultFoldoutHeaderHeight
            + (property.isExpanded ? Styles.headerPadding + GetGUIState(property, label).listControl.GetHeight() : 0f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guiState = GetGUIState(property, label);

            Rect headerRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
            Rect sizeRect = new Rect(headerRect) { xMin = headerRect.xMax - Styles.arraySizeWidth };

            EventType prevType = Event.current.type;
            if (Event.current.type == EventType.MouseUp && sizeRect.Contains(Event.current.mousePosition))
            {
                Event.current.type = EventType.Used;
            }

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, property.isExpanded, label);
            EditorGUI.EndFoldoutHeaderGroup();

            if (Event.current.type == EventType.Used && sizeRect.Contains(Event.current.mousePosition)) Event.current.type = prevType;

            EditorGUI.BeginChangeCheck();
            EditorGUI.DelayedIntField(sizeRect, guiState.lengthProperty, Styles.sizeLabel);
            if (EditorGUI.EndChangeCheck())
                guiState.lengthProperty.intValue = Mathf.Clamp(guiState.lengthProperty.intValue, 0, WeightedTransformArray.k_MaxLength);
            EditorGUI.LabelField(sizeRect, Styles.sizeLabel);

            if (headerRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
                {
                    OnDropObjects(property, DragAndDrop.objectReferences, guiState.listControl);
                    DragAndDrop.AcceptDrag();
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.DragExited)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.None;
                Event.current.Use();
            }

            if (property.isExpanded)
                guiState.listControl.DoList(new Rect(position) { yMin = headerRect.yMax + Styles.headerPadding });
        }

        static void OnDropObjects(SerializedProperty property, UnityObject[] objectReferences, ReorderableList listControl)
        {
            foreach (var o in objectReferences)
            {
                var go = o as GameObject;
                var c = o as Component;

                if (go == null && c == null)
                    continue;

                if (listControl.list.Count >= WeightedTransformArray.k_MaxLength)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    continue;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                var t = c == null ? go.transform : c.transform;
                if (Event.current.type == EventType.DragPerform)
                {
                    ModifyItemsSingle(property, items =>
                    {
                        var weight = items.Count == 0 ? 1f : items[items.Count - 1].weight;
                        items.Add(new WeightedTransform(t, weight));
                    });
                    listControl.list.Add(listControl.list.Count);
                }
            }
        }
    }
}
