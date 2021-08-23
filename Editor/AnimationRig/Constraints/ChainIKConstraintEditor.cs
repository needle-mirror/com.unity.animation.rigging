using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(ChainIKConstraint))]
    [CanEditMultipleObjects]
    class ChainIKConstraintEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent root = EditorGUIUtility.TrTextContent(
                "Root",
                "The root GameObject of the chain hierarchy."
            );
            public static readonly GUIContent tip = EditorGUIUtility.TrTextContent(
                "Tip",
                "The final GameObject of the chain hierarchy. It must be a descendant of the Root GameObject."
            );
            public static readonly GUIContent target = EditorGUIUtility.TrTextContent(
                "Target",
                "The GameObject that specifies the desired target transform for the chain's Tip."
            );
            public static readonly GUIContent sourceObjects = new GUIContent(CommonContent.sourceObjects.text);
            public static readonly GUIContent chainRotationWeight = EditorGUIUtility.TrTextContent(
                "Chain Rotation Weight",
                "The weight of rotations applied throughout the chain."
            );
            public static readonly GUIContent tipRotationWeight = EditorGUIUtility.TrTextContent(
                "Tip Rotation Weight",
                "The weight of the rotation applied to the Tip."
            );
            public static readonly GUIContent maxIterations = EditorGUIUtility.TrTextContent(
                "Max Iterations",
                "The maximum number of solver iterations to perform to try to make the Tip reach the Target within the specified Tolerance threshold."
            );
            public static readonly GUIContent tolerance = EditorGUIUtility.TrTextContent(
                "Tolerance",
                "Distance tolerance between the Target and Tip GameObjects. " +
                "The solver will finish its computation if the distance is less than this value at any point, even if Max Iterations has not been reached."
            );
        }

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

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);
            EditorGUILayout.PropertyField(m_Root, Content.root);
            EditorGUILayout.PropertyField(m_Tip, Content.tip);

            m_SourceObjectsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SourceObjectsToggle.value, Content.sourceObjects);
            if (m_SourceObjectsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Target, Content.target);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, CommonContent.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                MaintainOffsetHelper.DoDropdown(CommonContent.maintainIKTargetOffset, m_MaintainTargetPositionOffset, m_MaintainTargetRotationOffset);
                EditorGUILayout.PropertyField(m_ChainRotationWeight, Content.chainRotationWeight);
                EditorGUILayout.PropertyField(m_TipRotationWeight, Content.tipRotationWeight);
                EditorGUILayout.PropertyField(m_MaxIterations, Content.maxIterations);
                EditorGUILayout.PropertyField(m_Tolerance, Content.tolerance);
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
