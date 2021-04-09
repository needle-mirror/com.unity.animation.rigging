using System.Reflection;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [CustomPropertyDrawer(typeof(WeightedTransform))]
    class WeightedTransformDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        (WeightRangeAttribute attr, RangeAttribute legacyAttr)? m_RangeAttributes;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var (attr, legacyAttr) = m_RangeAttributes ??= (
                fieldInfo.GetCustomAttribute<WeightRangeAttribute>(),
                fieldInfo.GetCustomAttribute<RangeAttribute>()
            );

            float min = attr?.min ?? legacyAttr?.min ?? float.NaN;
            float max = attr?.max ?? legacyAttr?.max ?? float.NaN;

            DoGUI(rect, property, min, max);
        }

        static class Styles
        {
            public static float transformFieldWidthScale = 0.65f;
            public static readonly int horizontalMargin = (
                EditorStyles.objectField.margin.right + GUI.skin.horizontalSlider.margin.left
            ) / 2;
        }

        internal static void DoGUI(Rect rect, SerializedProperty property, float min, float max)
        {
            EditorGUI.BeginProperty(rect, GUIContent.none, property);

            var w = rect.width * Styles.transformFieldWidthScale;
            var weightRect = new Rect(rect.x + w, rect.y, rect.width - w, EditorGUIUtility.singleLineHeight);
            rect.width = w;

            var transformRect = new Rect(rect.x, rect.y, rect.width - Styles.horizontalMargin, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(transformRect, property.FindPropertyRelative("transform"), GUIContent.none);

            var indentLvl = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            if (float.IsNaN(max) || float.IsNaN(min))
                EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"), GUIContent.none);
            else
                EditorGUI.Slider(weightRect, property.FindPropertyRelative("weight"), min, max, GUIContent.none);
            EditorGUI.indentLevel = indentLvl;

            EditorGUI.EndProperty();
        }
    }
}
