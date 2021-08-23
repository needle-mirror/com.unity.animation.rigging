using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(OverrideTransform))]
    [CanEditMultipleObjects]
    class OverrideTransformEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent sourceObjects = new GUIContent(CommonContent.sourceObjects.text);
            public static readonly GUIContent overrideSource = EditorGUIUtility.TrTextContent(
                "Override Source",
                "The GameObject that influences the Constrained Object's transform. " +
                "If specified, Override Position and Override Rotation fields are ignored."
            );
            public static readonly GUIContent overridePosition = EditorGUIUtility.TrTextContent(
                "Override Position",
                "A specific position value to apply to the Constrained Object. " +
                "This value is ignored if an Override Source is specified."
            );
            public static readonly GUIContent overrideRotation = EditorGUIUtility.TrTextContent(
                "Override Rotation",
                "A specific rotation value to apply to the Constrained Object. " +
                "This value is ignored if an Override Source is specified."
            );
            public static readonly GUIContent space = EditorGUIUtility.TrTextContent(
                "Space",
                "Specifies how the Override Source's local transform values (or manual Override Position and Rotation) should be applied to Constrained Object."
            );
            public static readonly GUIContent positionWeight = EditorGUIUtility.TrTextContent(
                "Position Weight",
                "The weight of the position influence."
            );
            public static readonly GUIContent rotationWeight = EditorGUIUtility.TrTextContent(
                "Rotation Weight",
                "The weight of the rotation influence."
            );
        }

        SerializedProperty m_Weight;
        SerializedProperty m_ConstrainedObject;
        SerializedProperty m_OverrideSource;
        SerializedProperty m_OverridePosition;
        SerializedProperty m_OverrideRotation;
        SerializedProperty m_Space;
        SerializedProperty m_PositionWeight;
        SerializedProperty m_RotationWeight;

        readonly FoldoutState m_SourceObjectsToggle = FoldoutState.ForSourceObjects<OverrideTransformEditor>();
        readonly FoldoutState m_SettingsToggle = FoldoutState.ForSettings<OverrideTransformEditor>();

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_ConstrainedObject = data.FindPropertyRelative("m_ConstrainedObject");
            m_OverrideSource = data.FindPropertyRelative("m_OverrideSource");
            m_OverridePosition = data.FindPropertyRelative("m_OverridePosition");
            m_OverrideRotation = data.FindPropertyRelative("m_OverrideRotation");
            m_Space = data.FindPropertyRelative("m_Space");
            m_PositionWeight = data.FindPropertyRelative("m_PositionWeight");
            m_RotationWeight = data.FindPropertyRelative("m_RotationWeight");
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
                EditorGUILayout.PropertyField(m_OverrideSource, Content.overrideSource);
                using (new EditorGUI.DisabledScope(m_OverrideSource.objectReferenceValue != null))
                {
                    EditorGUILayout.PropertyField(m_OverridePosition, Content.overridePosition);
                    EditorGUILayout.PropertyField(m_OverrideRotation, Content.overrideRotation);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_SettingsToggle.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_SettingsToggle.value, CommonContent.settings);
            if (m_SettingsToggle.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Space, Content.space);
                EditorGUILayout.PropertyField(m_PositionWeight, Content.positionWeight);
                EditorGUILayout.PropertyField(m_RotationWeight, Content.rotationWeight);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/OverrideTransform/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as OverrideTransform;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/OverrideTransform/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as OverrideTransform;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(OverrideTransform))]
    class OverrideTransformBakeParameters : BakeParameters<OverrideTransform>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => false;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, OverrideTransform constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            if (constraint.data.sourceObject != null)
            {
                EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, constraint.data.sourceObject, bindings);
            }
            else
            {
                var data = (IOverrideTransformData)constraint.data;
                EditorCurveBindingUtils.CollectVector3Bindings(rigBuilder.transform, constraint, data.positionVector3Property, bindings);
                EditorCurveBindingUtils.CollectVector3Bindings(rigBuilder.transform, constraint, data.rotationVector3Property, bindings);
            }

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, OverrideTransform constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            EditorCurveBindingUtils.CollectTRSBindings(rigBuilder.transform, constraint.data.constrainedObject, bindings);

            return bindings;
        }
    }
}
