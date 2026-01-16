using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    internal static class AnimationRiggingMenu
    {
        static bool FilterSourceAndDestinationFromSelection(out Transform source, out Transform destination)
        {
#if UNITY_6000_3_OR_NEWER
            var selected = Selection.entityIds;
#else
            var selected = Selection.instanceIDs;
#endif

            if (selected == null || selected.Length != 2)
            {
                source = destination = null;
                return false;
            }

#if UNITY_6000_3_OR_NEWER
            var srcGameObject = EditorUtility.EntityIdToObject(selected[1]) as GameObject;
            var dstGameObject = EditorUtility.EntityIdToObject(selected[0]) as GameObject;
#else
            var srcGameObject = EditorUtility.InstanceIDToObject(selected[1]) as GameObject;
            var dstGameObject = EditorUtility.InstanceIDToObject(selected[0]) as GameObject;
#endif

            if (srcGameObject == null || dstGameObject == null)
            {
                source = destination = null;
                return false;
            }

            source = srcGameObject.transform;
            destination = dstGameObject.transform;

            return true;
        }

        [MenuItem("Animation Rigging/Align Transform", false, 0)]
        static void PerformTransformAlign()
        {
            if (FilterSourceAndDestinationFromSelection(out Transform src, out Transform dst))
            {
                Undo.RecordObject(dst, "Align transform " + dst.name + " with " + src.name);
                dst.SetPositionAndRotation(src.position, src.rotation);
            }
        }

        [MenuItem("Animation Rigging/Align Rotation", false, 0)]
        static void PerformRotationAlign()
        {
            if (FilterSourceAndDestinationFromSelection(out Transform src, out Transform dst))
            {
                Undo.RecordObject(dst, "Align rotation of " + dst.name + " with " + src.name);
                dst.rotation = src.rotation;
            }
        }

        [MenuItem("Animation Rigging/Align Position", false, 0)]
        static void PerformPositionAlign()
        {
            if (FilterSourceAndDestinationFromSelection(out Transform src, out Transform dst))
            {
                Undo.RecordObject(dst, "Align position of " + dst.name + " with " + src.name);
                dst.position = src.position;
            }
        }

        [MenuItem("Animation Rigging/Restore Bind Pose", false, 11)]
        static void RestoreBindPose()
        {
            var selection = Selection.activeTransform;
            if (selection == null)
                return;

            AnimationRiggingEditorUtils.RestoreBindPose(selection);
        }

        [MenuItem("Animation Rigging/Rig Setup", false, 12)]
        static void RigSetup()
        {
            var selection = Selection.activeTransform;
            if (selection == null)
                return;

            AnimationRiggingEditorUtils.RigSetup(selection);
        }

        [MenuItem("Animation Rigging/Bone Renderer Setup", false, 13)]
        static void BoneRendererSetup()
        {
            var selection = Selection.activeTransform;
            if (selection == null)
                return;

            AnimationRiggingEditorUtils.BoneRendererSetup(selection);
        }
    }
}
