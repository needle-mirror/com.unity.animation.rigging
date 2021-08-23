using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(TwistCorrection))]
    [CanEditMultipleObjects]
    class TwistCorrectionEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent source = EditorGUIUtility.TrTextContent(
                "Source",
                "The GameObject that influences the Twist Nodes to rotate around a specific Twist Axis."
            );
            public static readonly GUIContent twistAxis = EditorGUIUtility.TrTextContent(
                "Twist Axis",
                "Specifies the axis on the Source object from which the rotation is extracted and then redistributed to the Twist Nodes."
            );
            public static readonly GUIContent twistNodes = EditorGUIUtility.TrTextContent(
                "Twist Nodes",
                "The list of GameObjects that will be influenced by the Source GameObject, and the cumulative percentage of the Source's twist rotation they should inherit. " +
                "They are generally expected to all be leaf nodes in the hierarchy (i.e., they have a common parent and no children), and to have their twist axes oriented the same as the Source object in their initial pose."
            );
        }

        SerializedProperty m_Weight;
        SerializedProperty m_Source;
        SerializedProperty m_TwistAxis;
        SerializedProperty m_TwistNodes;
        SerializedProperty m_TwistNodesLength;

        readonly FoldoutState m_SourceObjectsToggle = FoldoutState.ForSourceObjects<TwistChainConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");
            var data = serializedObject.FindProperty("m_Data");
            m_Source = data.FindPropertyRelative("m_Source");
            m_TwistAxis = data.FindPropertyRelative("m_TwistAxis");
            m_TwistNodes = data.FindPropertyRelative("m_TwistNodes");
            m_TwistNodesLength = m_TwistNodes.FindPropertyRelative("m_Length");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);

            // by default, the first WeightedTransform element a user adds has a weight of 1
            // for this constraint, the first twist node usually should not have a value of 1
            // TODO: make drag/drop auto-distribute weights
            EditorGUI.BeginChangeCheck();
            var oldLength = m_TwistNodesLength.intValue;
            EditorGUILayout.PropertyField(m_TwistNodes, Content.twistNodes);
            if (EditorGUI.EndChangeCheck() && oldLength == 0 && m_TwistNodesLength.intValue != oldLength)
                m_TwistNodes.FindPropertyRelative("m_Item0.weight").floatValue = 0f;

            EditorGUILayout.PropertyField(m_TwistAxis, Content.twistAxis);

            m_SourceObjectsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SourceObjectsToggle.value, Content.source);
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_Source);
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/TwistCorrection/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as TwistCorrection;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/TwistCorrection/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as TwistCorrection;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(TwistCorrection))]
    class TwistCorrectionBakeParameters : BakeParameters<TwistCorrection>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => false;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, TwistCorrection constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, constraint.data.sourceObject, bindings);

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, TwistCorrection constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            foreach (var node in constraint.data.twistNodes)
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, node.transform, bindings);

            return bindings;
        }
    }
}
