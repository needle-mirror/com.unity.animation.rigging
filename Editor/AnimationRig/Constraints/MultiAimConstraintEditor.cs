using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(MultiAimConstraint))]
    [CanEditMultipleObjects]
    class MultiAimConstraintEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent[] axisLabels = new []{ "X", "-X", "Y", "-Y", "Z", "-Z" }
                .Select(c => new GUIContent(c))
                .ToArray();
            public static readonly GUIContent aimAxis = EditorGUIUtility.TrTextContent(
                "Aim Axis",
                "Specifies the local aim axis of the Constrained Object to use in order to orient its forward direction to the Source Objects."
            );
            public static readonly GUIContent upAxis = EditorGUIUtility.TrTextContent(
                "Up Axis",
                "Specifies the local up axis of the Constrained Object to use in order to orient its upward direction (i.e., roll orientation)."
            );
            public static readonly GUIContent worldUpType = EditorGUIUtility.TrTextContent(
                "World Up Type",
                "Specifies which mode to use to stabilize the upward direction (i.e., roll orientation) of the Constrained Object."
            );
            public static readonly GUIContent worldUpAxis = EditorGUIUtility.TrTextContent(
                "World Up Axis",
                "A vector in some reference frame that is used to stabilize the upward direction of the Constrained Object. " +
                "This value is used when World Up Type is either Vector or Object Rotation Up."
            );
            public static readonly GUIContent worldUpObject = EditorGUIUtility.TrTextContent(
                "World Up Object",
                "A GameObject used as a reference frame for World Up Axis. " +
                "This value is used when World Up Type is either Object Up or Object Rotation Up."
            );
            public static readonly GUIContent sourceObjects = CommonContent.sourceObjectsWeightedRotation;
            public static readonly GUIContent settings = CommonContent.settings;
            public static readonly GUIContent maintainOffset = CommonContent.maintainRotationOffset;
            public static readonly GUIContent minLimit = EditorGUIUtility.TrTextContent(
                "Min Limit",
                "Clamps the minimum rotation that may be applied about any of the constrained axes of rotation."
            );
            public static readonly GUIContent maxLimit = EditorGUIUtility.TrTextContent(
                "Max Limit",
                "Clamps the maximum rotation that may be applied about any of the constrained axes of rotation."
            );
        }

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_AimAxis;
        SerializedProperty m_UpAxis;
        SerializedProperty m_WorldUpType;
        SerializedProperty m_WorldUpAxis;
        SerializedProperty m_WorldUpObject;
        SerializedProperty m_SourceObjects;
        SerializedProperty m_MaintainOffset;
        SerializedProperty m_Offset;
        SerializedProperty m_ConstrainedAxes;
        SerializedProperty m_MinLimit;
        SerializedProperty m_MaxLimit;

        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<MultiAimConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_AimAxis = data.FindPropertyRelative("m_AimAxis");
            m_UpAxis = data.FindPropertyRelative("m_UpAxis");
            m_WorldUpType = data.FindPropertyRelative("m_WorldUpType");
            m_WorldUpAxis = data.FindPropertyRelative("m_WorldUpAxis");
            m_WorldUpObject = data.FindPropertyRelative("m_WorldUpObject");
            m_SourceObjects = data.FindPropertyRelative("m_SourceObjects");
            m_MaintainOffset = data.FindPropertyRelative("m_MaintainOffset");
            m_Offset = data.FindPropertyRelative("m_Offset");
            m_ConstrainedAxes = data.FindPropertyRelative("m_ConstrainedAxes");
            m_MinLimit = data.FindPropertyRelative("m_MinLimit");
            m_MaxLimit = data.FindPropertyRelative("m_MaxLimit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);

            EditorGUILayout.PropertyField(m_ConstrainedObject, CommonContent.constrainedObject);

            ++EditorGUI.indentLevel;
            DoAxisField(m_AimAxis, Content.aimAxis);
            DoAxisField(m_UpAxis, Content.upAxis);
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(m_WorldUpType, Content.worldUpType);

            var worldUpType = (MultiAimConstraintData.WorldUpType)m_WorldUpType.intValue;

            ++EditorGUI.indentLevel;
            using (new EditorGUI.DisabledGroupScope(worldUpType != MultiAimConstraintData.WorldUpType.ObjectRotationUp && worldUpType != MultiAimConstraintData.WorldUpType.Vector))
            {
                DoAxisField(m_WorldUpAxis, Content.worldUpAxis);
            }
            using (new EditorGUI.DisabledGroupScope(worldUpType != MultiAimConstraintData.WorldUpType.ObjectUp && worldUpType != MultiAimConstraintData.WorldUpType.ObjectRotationUp))
            {
                EditorGUILayout.PropertyField(m_WorldUpObject, Content.worldUpObject);
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(m_SourceObjects, Content.sourceObjects);

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, Content.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_MaintainOffset, Content.maintainOffset);
                EditorGUILayout.PropertyField(m_Offset, CommonContent.offsetRotation);
                EditorGUILayout.PropertyField(m_ConstrainedAxes, CommonContent.constrainedAxesRotation);
                EditorGUILayout.PropertyField(m_MinLimit, Content.minLimit);
                EditorGUILayout.PropertyField(m_MaxLimit, Content.maxLimit);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        static void DoAxisField(SerializedProperty property, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Popup(rect, label, property.intValue, Content.axisLabels);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newValue;
            EditorGUI.EndProperty();
        }

        [MenuItem("CONTEXT/MultiAimConstraint/Transfer motion to constraint", false, 611)]
        public static void TransferMotionToConstraint(MenuCommand command)
        {
            var constraint = command.context as MultiAimConstraint;
            BakeUtils.TransferMotionToConstraint(constraint);
        }

        [MenuItem("CONTEXT/MultiAimConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as MultiAimConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/MultiAimConstraint/Transfer motion to constraint", true)]
        [MenuItem("CONTEXT/MultiAimConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as MultiAimConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(MultiAimConstraint))]
    class MultiAimConstraintBakeParameters : BakeParameters<MultiAimConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => true;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, MultiAimConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            for (int i = 0; i < constraint.data.sourceObjects.Count; ++i)
            {
                var sourceObject = constraint.data.sourceObjects[i];

                EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, sourceObject.transform, bindings);
                EditorCurveBindingUtils.CollectPropertyBindings(rigBuilder.transform, constraint, ((IMultiAimConstraintData)constraint.data).sourceObjectsProperty + ".m_Item" + i + ".weight", bindings);
            }

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, MultiAimConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, constraint.data.constrainedObject, bindings);

            return bindings;
        }
    }
}
