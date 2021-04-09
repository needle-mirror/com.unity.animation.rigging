using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(DampedTransform))]
    [CanEditMultipleObjects]
    class DampedTransformEditor : Editor
    {
        static readonly GUIContent k_SourceObjectLabel = new GUIContent("Source Object");
        static readonly GUIContent k_SettingsLabel = new GUIContent("Settings");

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_Source;
        SerializedProperty m_DampPosition;
        SerializedProperty m_DampRotation;
        SerializedProperty m_MaintainAim;

        readonly FoldoutState m_SourceObjectsToggle = FoldoutState.ForSourceObjects<DampedTransformEditor>();
        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<DampedTransformEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_Source = data.FindPropertyRelative("m_Source");
            m_DampPosition = data.FindPropertyRelative("m_DampPosition");
            m_DampRotation = data.FindPropertyRelative("m_DampRotation");
            m_MaintainAim = data.FindPropertyRelative("m_MaintainAim");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject);

            m_SourceObjectsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SourceObjectsToggle.value, k_SourceObjectLabel);
            if (m_SourceObjectsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Source);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, k_SettingsLabel);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DampPosition);
                EditorGUILayout.PropertyField(m_DampRotation);
                EditorGUILayout.PropertyField(m_MaintainAim);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/DampedTransform/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as DampedTransform;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/DampedTransform/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as DampedTransform;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(DampedTransform))]
    class DampedTransformBakeParameters : BakeParameters<DampedTransform>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => false;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, DampedTransform constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, constraint.data.sourceObject, bindings);

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, DampedTransform constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, constraint.data.constrainedObject, bindings);

            return bindings;
        }
    }
}
