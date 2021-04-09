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
        static readonly GUIContent k_SourceObjectsLabel = new GUIContent("Source Objects");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");
        static readonly GUIContent k_AimAxisLabel = new GUIContent("Aim Axis");
        static readonly GUIContent k_UpAxisLabel = new GUIContent("Up Axis");
        static readonly GUIContent k_WorldUpAxisLabel = new GUIContent("World Up Axis");
        static readonly GUIContent k_WorldUpType = new GUIContent("World Up Type");
        static readonly GUIContent k_WorldUpObject = new GUIContent("World Up Object");
        static readonly GUIContent[] k_AxisLabels = new []{ "X", "-X", "Y", "-Y", "Z", "-Z" }.Select(s => new GUIContent(s)).ToArray();
        static readonly GUIContent k_MaintainOffsetLabel = new GUIContent("Maintain Rotation Offset");

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

            EditorGUILayout.PropertyField(m_Weight);

            EditorGUILayout.PropertyField(m_ConstrainedObject);

            ++EditorGUI.indentLevel;
            DoAxisField(m_AimAxis, k_AimAxisLabel);
            DoAxisField(m_UpAxis, k_UpAxisLabel);
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(m_WorldUpType, k_WorldUpType);

            var worldUpType = (MultiAimConstraintData.WorldUpType)m_WorldUpType.intValue;

            ++EditorGUI.indentLevel;
            using (new EditorGUI.DisabledGroupScope(worldUpType != MultiAimConstraintData.WorldUpType.ObjectRotationUp && worldUpType != MultiAimConstraintData.WorldUpType.Vector))
            {
                DoAxisField(m_WorldUpAxis, k_WorldUpAxisLabel);
            }
            using (new EditorGUI.DisabledGroupScope(worldUpType != MultiAimConstraintData.WorldUpType.ObjectUp && worldUpType != MultiAimConstraintData.WorldUpType.ObjectRotationUp))
            {
                EditorGUILayout.PropertyField(m_WorldUpObject, k_WorldUpObject);
            }
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(m_SourceObjects, k_SourceObjectsLabel);

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, k_SettingsLabel);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_MaintainOffset, k_MaintainOffsetLabel);
                EditorGUILayout.PropertyField(m_Offset);
                EditorGUILayout.PropertyField(m_ConstrainedAxes);
                EditorGUILayout.PropertyField(m_MinLimit);
                EditorGUILayout.PropertyField(m_MaxLimit);
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
            var newValue = EditorGUI.Popup(rect, label, property.intValue, k_AxisLabels);
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
