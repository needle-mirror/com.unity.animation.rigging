using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(MultiParentConstraint))]
    [CanEditMultipleObjects]
    class MultiParentConstraintEditor : Editor
    {
        static readonly GUIContent k_SourceObjectsLabel = new GUIContent("Source Objects");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");
        static readonly GUIContent k_MaintainOffsetLabel = new GUIContent("Maintain Offset");

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_ConstrainedPositionAxes;
        SerializedProperty m_ConstrainedRotationAxes;
        SerializedProperty m_SourceObjects;
        SerializedProperty m_MaintainPositionOffset;
        SerializedProperty m_MaintainRotationOffset;

        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<MultiParentConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_ConstrainedPositionAxes = data.FindPropertyRelative("m_ConstrainedPositionAxes");
            m_ConstrainedRotationAxes = data.FindPropertyRelative("m_ConstrainedRotationAxes");
            m_SourceObjects = data.FindPropertyRelative("m_SourceObjects");
            m_MaintainPositionOffset = data.FindPropertyRelative("m_MaintainPositionOffset");
            m_MaintainRotationOffset = data.FindPropertyRelative("m_MaintainRotationOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject);
            EditorGUILayout.PropertyField(m_SourceObjects, k_SourceObjectsLabel);

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, k_SettingsLabel);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                MaintainOffsetHelper.DoDropdown(k_MaintainOffsetLabel, m_MaintainPositionOffset, m_MaintainRotationOffset);
                EditorGUILayout.PropertyField(m_ConstrainedPositionAxes);
                EditorGUILayout.PropertyField(m_ConstrainedRotationAxes);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/MultiParentConstraint/Transfer motion to constraint", false, 611)]
        public static void TransferMotionToConstraint(MenuCommand command)
        {
            var constraint = command.context as MultiParentConstraint;
            BakeUtils.TransferMotionToConstraint(constraint);
        }

        [MenuItem("CONTEXT/MultiParentConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as MultiParentConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/MultiParentConstraint/Transfer motion to constraint", true)]
        [MenuItem("CONTEXT/MultiParentConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as MultiParentConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(MultiParentConstraint))]
    class MultiParentConstraintParameters : BakeParameters<MultiParentConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => true;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, MultiParentConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            for (int i = 0; i < constraint.data.sourceObjects.Count; ++i)
            {
                var sourceObject = constraint.data.sourceObjects[i];

                EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, sourceObject.transform, bindings);
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, sourceObject.transform, bindings);
                EditorCurveBindingUtils.CollectPropertyBindings(rigBuilder.transform, constraint, ((IMultiParentConstraintData)constraint.data).sourceObjectsProperty + ".m_Item" + i + ".weight", bindings);
            }

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, MultiParentConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();
            EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, constraint.data.constrainedObject, bindings);
            EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, constraint.data.constrainedObject, bindings);
            return bindings;
        }
    }
}
