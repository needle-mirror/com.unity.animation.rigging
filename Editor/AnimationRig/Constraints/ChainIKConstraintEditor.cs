using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(ChainIKConstraint))]
    [CanEditMultipleObjects]
    class ChainIKConstraintEditor : Editor
    {
        static readonly GUIContent k_SourceObjectLabel = new GUIContent("Source Object");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");
        static readonly GUIContent k_MaintainTargetOffsetLabel = new GUIContent("Maintain Target Offset");

        SerializedProperty m_Weight;
        SerializedProperty m_Root;
        SerializedProperty m_Tip;
        SerializedProperty m_Target;
        SerializedProperty m_ChainRotationWeight;
        SerializedProperty m_TipRotationWeight;
        SerializedProperty m_MaxIterations;
        SerializedProperty m_Tolerance;
        SerializedProperty m_MaintainTargetPositionOffset;
        SerializedProperty m_MaintainTargetRotationOffset;

        readonly FoldoutState m_SourceObjectsToggle = FoldoutState.ForSourceObjects<ChainIKConstraintEditor>();
        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<ChainIKConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_Root = data.FindPropertyRelative("m_Root");
            m_Tip = data.FindPropertyRelative("m_Tip");
            m_Target = data.FindPropertyRelative("m_Target");
            m_ChainRotationWeight = data.FindPropertyRelative("m_ChainRotationWeight");
            m_TipRotationWeight = data.FindPropertyRelative("m_TipRotationWeight");
            m_MaxIterations = data.FindPropertyRelative("m_MaxIterations");
            m_Tolerance = data.FindPropertyRelative("m_Tolerance");
            m_MaintainTargetPositionOffset = data.FindPropertyRelative("m_MaintainTargetPositionOffset");
            m_MaintainTargetRotationOffset = data.FindPropertyRelative("m_MaintainTargetRotationOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_Root);
            EditorGUILayout.PropertyField(m_Tip);

            m_SourceObjectsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SourceObjectsToggle.value, k_SourceObjectLabel);
            if (m_SourceObjectsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Target);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, k_SettingsLabel);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                MaintainOffsetHelper.DoDropdown(k_MaintainTargetOffsetLabel, m_MaintainTargetPositionOffset, m_MaintainTargetRotationOffset);
                EditorGUILayout.PropertyField(m_ChainRotationWeight);
                EditorGUILayout.PropertyField(m_TipRotationWeight);
                EditorGUILayout.PropertyField(m_MaxIterations);
                EditorGUILayout.PropertyField(m_Tolerance);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/ChainIKConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as ChainIKConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/ChainIKConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as ChainIKConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(ChainIKConstraint))]
    class ChainIKConstraintBakeParameters : BakeParameters<ChainIKConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => false;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, ChainIKConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, constraint.data.target, bindings);

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, ChainIKConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            var root = constraint.data.root;
            var tip = constraint.data.tip;

            var tmp = tip;
            while (tmp != root)
            {
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, tmp, bindings);
                tmp = tmp.parent;
            }
            EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, root, bindings);

            return bindings;
        }
    }
}
