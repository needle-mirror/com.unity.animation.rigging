using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(TwistChainConstraint))]
    [CanEditMultipleObjects]
    class TwistChainConstraintEditor : Editor
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
            public static readonly GUIContent sourceObjects = new GUIContent(CommonContent.sourceObjects.text);
            public static readonly GUIContent rootTarget = EditorGUIUtility.TrTextContent(
                "Root Target",
                "The GameObject that specifies the desired target rotation for the chain's Root."
            );
            public static readonly GUIContent tipTarget = EditorGUIUtility.TrTextContent(
                "Tip Target",
                "The GameObject that specifies the desired target rotation for the chain's Tip."
            );
            public static readonly GUIContent curve = EditorGUIUtility.TrTextContent(
                "Curve",
                "A curve with a normalized domain and range, specifying how the twist rotation should be distributed down the length of the chain."
            );
        }

        SerializedProperty m_Weight;
        SerializedProperty m_Root;
        SerializedProperty m_Tip;
        SerializedProperty m_RootTarget;
        SerializedProperty m_TipTarget;
        SerializedProperty m_Curve;

        readonly FoldoutState m_SourceObjectsToggle = FoldoutState.ForSourceObjects<TwistChainConstraintEditor>();
        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<TwistChainConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_RootTarget = data.FindPropertyRelative("m_RootTarget");
            m_TipTarget = data.FindPropertyRelative("m_TipTarget");
            m_Root = data.FindPropertyRelative("m_Root");
            m_Tip = data.FindPropertyRelative("m_Tip");
            m_Curve = data.FindPropertyRelative("m_Curve");
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
                EditorGUILayout.PropertyField(m_RootTarget, Content.rootTarget);
                EditorGUILayout.PropertyField(m_TipTarget, Content.tipTarget);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, CommonContent.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Curve, Content.curve);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/TwistChainConstraint/Transfer motion to constraint", false, 611)]
        public static void TransferMotionToConstraint(MenuCommand command)
        {
            var constraint = command.context as TwistChainConstraint;
            BakeUtils.TransferMotionToConstraint(constraint);
        }

        [MenuItem("CONTEXT/TwistChainConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as TwistChainConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/TwistChainConstraint/Transfer motion to constraint", true)]
        [MenuItem("CONTEXT/TwistChainConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as TwistChainConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(TwistChainConstraint))]
    class TwistChainConstraintBakeParameters : BakeParameters<TwistChainConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => true;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, TwistChainConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, constraint.data.rootTarget, bindings);
            EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, constraint.data.tipTarget, bindings);

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, TwistChainConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            // Retrieve chain in-between root and tip transforms.
            Transform[] chain = ConstraintsUtils.ExtractChain(constraint.data.root, constraint.data.tip);

            for (int i = 0; i < chain.Length; ++i)
            {
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, chain[i], bindings);
            }

            return bindings;
        }
    }
}

