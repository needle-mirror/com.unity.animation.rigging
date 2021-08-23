using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    static class CommonContent
    {
        public static readonly GUIContent constrainedAxesPosition = EditorGUIUtility.TrTextContent(
            "Constrained Axes",
            "Specifies the axes to which the constraint can apply translation."
        );

        public static readonly GUIContent constrainedAxesRotation = EditorGUIUtility.TrTextContent(
            "Constrained Axes",
            "Specifies the axes to which the constraint can apply rotation."
        );

        public static readonly GUIContent constrainedObject = EditorGUIUtility.TrTextContent(
            "Constrained Object",
            "The GameObject affected by the Source Objects."
        );

        public static readonly GUIContent maintainOffset = EditorGUIUtility.TrTextContent(
            "Maintain Offset",
            "Specifies whether to maintain the initial offset between the Constrained Object and the Source Objects"
        );

        public static readonly GUIContent maintainPositionOffset = EditorGUIUtility.TrTextContent(
            "Maintain Offset",
            "Specifies whether to maintain the initial position offset between the Constrained Object and the Source Objects."
        );

        public static readonly GUIContent maintainRotationOffset = EditorGUIUtility.TrTextContent(
            "Maintain Offset",
            "Specifies whether to maintain the initial rotation offset between the Constrained Object and the Source Objects."
        );

        public static readonly GUIContent maintainIKTargetOffset = EditorGUIUtility.TrTextContent(
            "Maintain Target Offset",
            "Specifies whether to maintain the initial offset between the Tip and the Target."
        );

        public static readonly GUIContent offsetPosition = EditorGUIUtility.TrTextContent(
            "Offset",
            "Specifies an additional local space translation offset to apply to the Constrained Object, after it has been translated toward its target."
        );

        public static readonly GUIContent offsetRotation = EditorGUIUtility.TrTextContent(
            "Offset",
            "Specifies an additional local space rotation offset to apply to the Constrained Object, after it has been rotated toward its target."
        );

        public static readonly GUIContent settings = EditorGUIUtility.TrTextContent(
            "Settings"
        );

        public static readonly GUIContent sourceObjects = EditorGUIUtility.TrTextContent(
            "Source Objects",
            "The list of GameObjects that influence the Constrained Object's position and orientation, and the amount of weight they contribute to the final pose. " +
            "The constraint applies linearly interpolated, weighted translation and rotation toward each target. " +
            "The order of Source Objects does not affect the result."
        );

        public static readonly GUIContent sourceObjectsWeightedPosition = EditorGUIUtility.TrTextContent(
            "Source Objects",
            "The list of GameObjects that influence the Constrained Object's position, and the amount of weight they contribute to the final pose. " +
            "The constraint calculates translation toward each target to produce a weighted sum. " +
            "The order of Source Objects does not affect the result."
        );

        public static readonly GUIContent sourceObjectsWeightedRotation = EditorGUIUtility.TrTextContent(
            "Source Objects",
            "The list of GameObjects that influence the Constrained Object's orientation, and the amount of weight they contribute to the final pose. " +
            "The constraint calculates rotation toward each target to produce a weighted sum. " +
            "The order of Source Objects does not affect the result."
        );

        public static readonly GUIContent weight = EditorGUIUtility.TrTextContent(
            "Weight",
            "The overall weight of the constraint. " +
            "If set to 0, the constraint has no influence on the Constrained Object. " +
            "When set to 1, it applies full influence with the current settings. " +
            "Intermediate values are interpolated linearly."
        );
    }
}
