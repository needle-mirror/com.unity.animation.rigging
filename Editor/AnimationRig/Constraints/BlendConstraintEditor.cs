using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(BlendConstraint))]
    [CanEditMultipleObjects]
    class BlendConstraintEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent sourceObjects = new GUIContent(CommonContent.sourceObjects.text);
            public static readonly GUIContent sourceA = EditorGUIUtility.TrTextContent(
                "Source A",
                "The first source GameObject that influences the position and rotation of the Constrained Object."
            );
            public static readonly GUIContent sourceB = EditorGUIUtility.TrTextContent(
                "Source B",
                "The second source GameObject that influences the position and rotation of the Constrained Object."
            );
            public static readonly GUIContent settings = CommonContent.settings;
            public static readonly GUIContent maintainOffset = CommonContent.maintainOffset;
            public static readonly GUIContent blendPosition = EditorGUIUtility.TrTextContent(
                "Blend A | B Position",
                "If enabled, the constrained GameObject's position blends between those of Source A and Source B by the specified amount."
            );
            public static readonly GUIContent blendRotation = EditorGUIUtility.TrTextContent(
                "Blend A | B Rotation",
                "If enabled, the constrained GameObject's rotation blends between those of Source A and Source B by the specified amount."
            );
        }

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_SourceA;
        SerializedProperty m_SourceB;
        SerializedProperty m_BlendPosition;
        SerializedProperty m_BlendRotation;
        SerializedProperty m_PositionWeight;
        SerializedProperty m_RotationWeight;
        SerializedProperty m_MaintainPositionOffsets;
        SerializedProperty m_MaintainRotationOffsets;

        readonly FoldoutState m_SourceObjectsToggle = FoldoutState.ForSourceObjects<BlendConstraintEditor>();
        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<BlendConstraintEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_SourceA = data.FindPropertyRelative("m_SourceA");
            m_SourceB = data.FindPropertyRelative("m_SourceB");
            m_BlendPosition = data.FindPropertyRelative("m_BlendPosition");
            m_BlendRotation = data.FindPropertyRelative("m_BlendRotation");
            m_PositionWeight = data.FindPropertyRelative("m_PositionWeight");
            m_RotationWeight = data.FindPropertyRelative("m_RotationWeight");
            m_MaintainPositionOffsets = data.FindPropertyRelative("m_MaintainPositionOffsets");
            m_MaintainRotationOffsets = data.FindPropertyRelative("m_MaintainRotationOffsets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);
            EditorGUILayout.PropertyField(m_ConstrainedObject, CommonContent.constrainedObject);

            m_SourceObjectsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SourceObjectsToggle.value, Content.sourceObjects);
            if (m_SourceObjectsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_SourceA, Content.sourceA);
                EditorGUILayout.PropertyField(m_SourceB, Content.sourceB);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, Content.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;

                MaintainOffsetHelper.DoDropdown(Content.maintainOffset, m_MaintainPositionOffsets, m_MaintainRotationOffsets);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_BlendPosition, Content.blendPosition);
                using (new EditorGUI.DisabledScope(!m_BlendPosition.boolValue))
                    EditorGUILayout.PropertyField(m_PositionWeight, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(m_BlendRotation, Content.blendRotation);
                using (new EditorGUI.DisabledScope(!m_BlendRotation.boolValue))
                    EditorGUILayout.PropertyField(m_RotationWeight, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/BlendConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as BlendConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/BlendConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as BlendConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(BlendConstraint))]
    class BlendConstraintBakeParameters : BakeParameters<BlendConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => false;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, BlendConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();
            var sourceA = constraint.data.sourceObjectA;
            var sourceB = constraint.data.sourceObjectB;

            if (constraint.data.blendPosition)
            {
                EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, sourceA, bindings);
                EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, sourceB, bindings);
            }

            if (constraint.data.blendRotation)
            {
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, sourceA, bindings);
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, sourceB, bindings);
            }

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, BlendConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();
            var constrained = constraint.data.constrainedObject;

            if (constraint.data.blendPosition)
                EditorCurveBindingUtils.CollectPositionBindings(rigBuilder.transform, constrained, bindings);

            if(constraint.data.blendRotation)
                EditorCurveBindingUtils.CollectRotationBindings(rigBuilder.transform, constrained, bindings);

            return bindings;
        }
    }
}
