using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(DampedTransform))]
    [CanEditMultipleObjects]
    class DampedTransformEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent source = EditorGUIUtility.TrTextContent(
                "Source",
                "The GameObject that influences the Constrained Object's transform."
            );
            public static readonly GUIContent dampPosition = EditorGUIUtility.TrTextContent(
                "Damp Position",
                "The weight of positional damping to apply to the Constrained Object."
            );
            public static readonly GUIContent dampRotation = EditorGUIUtility.TrTextContent(
                "Damp Rotation",
                "The weight of rotational damping to apply to the Constrained Object."
            );
            public static readonly GUIContent maintainAim = EditorGUIUtility.TrTextContent(
                "Maintain Aim",
                "Specifies whether to maintain the initial rotation offset between the Constrained Object and the Source Object."
            );
        }

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

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject, CommonContent.constrainedObject);

            m_SourceObjectsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SourceObjectsToggle.value, Content.source);
            if (m_SourceObjectsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Source, Content.source);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, CommonContent.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DampPosition, Content.dampPosition);
                EditorGUILayout.PropertyField(m_DampRotation, Content.dampRotation);
                EditorGUILayout.PropertyField(m_MaintainAim, Content.maintainAim);
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
