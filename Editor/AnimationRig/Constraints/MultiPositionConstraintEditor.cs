using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(MultiPositionConstraint))]
    [CanEditMultipleObjects]
    class MultiPositionConstraintEditor : Editor
    {
        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_ConstrainedAxes;
        SerializedProperty m_SourceObjects;
        SerializedProperty m_MaintainOffset;
        SerializedProperty m_Offset;

        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<MultiPositionConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_ConstrainedAxes = data.FindPropertyRelative("m_ConstrainedAxes");
            m_SourceObjects = data.FindPropertyRelative("m_SourceObjects");
            m_MaintainOffset = data.FindPropertyRelative("m_MaintainOffset");
            m_Offset = data.FindPropertyRelative("m_Offset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject, CommonContent.constrainedObject);
            EditorGUILayout.PropertyField(m_ConstrainedAxes, CommonContent.constrainedAxesPosition);
            EditorGUILayout.PropertyField(m_SourceObjects, CommonContent.sourceObjectsWeightedPosition);

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, CommonContent.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_MaintainOffset, CommonContent.maintainPositionOffset);
                EditorGUILayout.PropertyField(m_Offset, CommonContent.offsetPosition);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/MultiPositionConstraint/Transfer motion to constraint", false, 611)]
        public static void TransferMotionToConstraint(MenuCommand command)
        {
            var constraint = command.context as MultiPositionConstraint;

            var axesMask = new Vector3(
                System.Convert.ToSingle(constraint.data.constrainedXAxis),
                System.Convert.ToSingle(constraint.data.constrainedYAxis),
                System.Convert.ToSingle(constraint.data.constrainedZAxis));

            if (Vector3.Dot(axesMask, axesMask) < 3f)
            {
                Debug.LogWarning("Multi-Position constraint with one or more Constrained Axes toggled off may lose precision when transferring its motion to constraint.");
            }

            BakeUtils.TransferMotionToConstraint(constraint);
        }

        [MenuItem("CONTEXT/MultiPositionConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as MultiPositionConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/MultiPositionConstraint/Transfer motion to constraint", true)]
        [MenuItem("CONTEXT/MultiPositionConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as MultiPositionConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(MultiPositionConstraint))]
    class MultiPositionConstraintBakeParameters : BakeParameters<MultiPositionConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => true;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, MultiPositionConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            for (int i = 0; i < constraint.data.sourceObjects.Count; ++i)
            {
                var sourceObject = constraint.data.sourceObjects[i];

                EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, sourceObject.transform, bindings);
                EditorCurveBindingUtils.CollectPropertyBindings(rigBuilder.transform, constraint, ((IMultiPositionConstraintData)constraint.data).sourceObjectsProperty + ".m_Item" + i + ".weight", bindings);
            }

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, MultiPositionConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, constraint.data.constrainedObject, bindings);

            return bindings;
        }
    }
}
