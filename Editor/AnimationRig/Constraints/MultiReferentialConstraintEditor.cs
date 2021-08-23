using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomEditor(typeof(MultiReferentialConstraint))]
    [CanEditMultipleObjects]
    class MultiReferentialConstraintEditor : Editor
    {
        static class Content
        {
            public static readonly GUIContent driving = EditorGUIUtility.TrTextContent(
                "Driving",
                "An object from the list of Referenced Objects, whose motion drives that of all other Referenced Objects."
            );
            public static readonly GUIContent referenceObjects = EditorGUIUtility.TrTextContent(
                "Reference Objects",
                "A list of GameObjects to be driven by the specified Driving object."
            );
        }

        SerializedProperty m_Weight;
        SerializedProperty m_Driver;
        SerializedProperty m_SourceObjects;
        SerializedProperty m_SourceObjectsSize;

        GUIContent[] m_DrivingLabels = Array.Empty<GUIContent>();
        int m_PreviousSourceSize;

        void OnEnable()
        {
            m_Weight = serializedObject.FindProperty("m_Weight");

            var data = serializedObject.FindProperty("m_Data");
            m_Driver = data.FindPropertyRelative("m_Driver");
            m_SourceObjects = data.FindPropertyRelative("m_SourceObjects");
            m_SourceObjectsSize = m_SourceObjects.FindPropertyRelative("Array.size");
            m_PreviousSourceSize = m_SourceObjectsSize.intValue;

            UpdateDrivingLabels();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Weight, CommonContent.weight);

            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, Content.driving, m_Driver);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Popup(rect, Content.driving, m_Driver.intValue, m_DrivingLabels);
            if (EditorGUI.EndChangeCheck())
                m_Driver.intValue = newValue;
            EditorGUI.EndProperty();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SourceObjects, Content.referenceObjects);
            // also check if size has changed, because drag/drop on default control and Reset do not trigger change
            if (EditorGUI.EndChangeCheck() || m_PreviousSourceSize != m_SourceObjectsSize.intValue)
            {
                UpdateDrivingLabels();
                m_PreviousSourceSize = m_SourceObjectsSize.intValue;
            }

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateDrivingLabels()
        {
            Array.Resize(ref m_DrivingLabels, m_SourceObjects.arraySize);
            for (int i = 0; i < m_DrivingLabels.Length; ++i)
            {
                var element = m_SourceObjects.GetArrayElementAtIndex(i);
                var name = element.objectReferenceValue == null ? "None" : element.objectReferenceValue.name;
                m_DrivingLabels[i] = new GUIContent($"{i} : {name}");
            }
        }

        [MenuItem("CONTEXT/MultiReferentialConstraint/Transfer motion to constraint", false, 611)]
        public static void TransferMotionToConstraint(MenuCommand command)
        {
            var constraint = command.context as MultiReferentialConstraint;
            BakeUtils.TransferMotionToConstraint(constraint);
        }

        [MenuItem("CONTEXT/MultiReferentialConstraint/Transfer motion to skeleton", false, 612)]
        public static void TransferMotionToSkeleton(MenuCommand command)
        {
            var constraint = command.context as MultiReferentialConstraint;
            BakeUtils.TransferMotionToSkeleton(constraint);
        }

        [MenuItem("CONTEXT/MultiReferentialConstraint/Transfer motion to constraint", true)]
        [MenuItem("CONTEXT/MultiReferentialConstraint/Transfer motion to skeleton", true)]
        public static bool TransferMotionValidate(MenuCommand command)
        {
            var constraint = command.context as MultiReferentialConstraint;
            return BakeUtils.TransferMotionValidate(constraint);
        }
    }

    [BakeParameters(typeof(MultiReferentialConstraint))]
    class MultiReferentialConstraintBakeParameters : BakeParameters<MultiReferentialConstraint>
    {
        public override bool canBakeToSkeleton => true;
        public override bool canBakeToConstraint => true;

        public override IEnumerable<EditorCurveBinding> GetSourceCurveBindings(RigBuilder rigBuilder, MultiReferentialConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            var sources = constraint.data.sourceObjects;
            for (int i = 1; i < sources.Count; ++i)
                EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, sources[i], bindings);

            return bindings;
        }

        public override IEnumerable<EditorCurveBinding> GetConstrainedCurveBindings(RigBuilder rigBuilder, MultiReferentialConstraint constraint)
        {
            var bindings = new List<EditorCurveBinding>();

            var transform = constraint.data.sourceObjects[0];
            EditorCurveBindingUtils.CollectTRBindings(rigBuilder.transform, transform, bindings);

            return bindings;
        }
    }
}
