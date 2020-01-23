using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Animations.Rigging
{
    public static class EditorCurveBindingUtils
    {
        public static void CollectVector3Bindings<T>(Transform root, T component, string propertyName, List<EditorCurveBinding> bindings)
            where T : MonoBehaviour
        {
            if (root == null || component == null || propertyName == "" || bindings == null)
                throw new ArgumentNullException("Arguments cannot be null.");

            var path = AnimationUtility.CalculateTransformPath(component.transform, root);

            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(T), propertyName + ".x"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(T), propertyName + ".y"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(T), propertyName + ".z"));
        }

        public static void CollectTRSBindings(Transform root, Transform transform, List<EditorCurveBinding> bindings)
        {
            CollectPositionBindings(root, transform, bindings);
            CollectRotationBindings(root, transform, bindings);
            CollectScaleBindings(root, transform, bindings);
        }

        public static void CollectTRBindings(Transform root, Transform transform, List<EditorCurveBinding> bindings)
        {
            CollectPositionBindings(root, transform, bindings);
            CollectRotationBindings(root, transform, bindings);
        }

        public static void CollectPositionBindings(Transform root, Transform transform, List<EditorCurveBinding> bindings)
        {
            if (root == null || transform == null || bindings == null)
                throw new ArgumentNullException("Arguments cannot be null.");

            var path = AnimationUtility.CalculateTransformPath(transform, root);

            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z"));
        }

        public static void CollectRotationBindings(Transform root, Transform transform, List<EditorCurveBinding> bindings)
        {
            if (root == null || transform == null || bindings == null)
                throw new ArgumentNullException("Arguments cannot be null.");

            var path = AnimationUtility.CalculateTransformPath(transform, root);

            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"));
        }

        public static void CollectScaleBindings(Transform root, Transform transform, List<EditorCurveBinding> bindings)
        {
            if (root == null || transform == null || bindings == null)
                throw new ArgumentNullException("Arguments cannot be null.");

            var path = AnimationUtility.CalculateTransformPath(transform, root);

            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.x"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.y"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalScale.z"));
        }

        public static void CollectPropertyBindings(Transform root, MonoBehaviour component, string propertyName, List<EditorCurveBinding> bindings)
        {
            if (root == null || component == null || bindings == null)
                throw new ArgumentNullException("Arguments cannot be null.");

            var path = AnimationUtility.CalculateTransformPath(component.transform, root);

            bindings.Add(EditorCurveBinding.FloatCurve(path, component.GetType(), propertyName));
        }

        public static bool RemapRotationBinding(AnimationClip clip, EditorCurveBinding binding, ref EditorCurveBinding rotationBinding)
        {
            if (!binding.propertyName.StartsWith("localEulerAngles"))
                return false;

            string suffix = binding.propertyName.Split('.')[1];

            rotationBinding = binding;

            // Euler Angles
            rotationBinding.propertyName = "localEulerAnglesRaw." + suffix;
            if (AnimationUtility.GetEditorCurve(clip, rotationBinding) != null)
                return true;

            // Euler Angles (Quaternion) interpolation
            rotationBinding.propertyName = "localEulerAnglesBaked." + suffix;
            if (AnimationUtility.GetEditorCurve(clip, rotationBinding) != null)
                return true;

            // Quaternion interpolation
            rotationBinding.propertyName = "localEulerAngles." + suffix;
            if (AnimationUtility.GetEditorCurve(clip, rotationBinding) != null)
                return true;

            return false;
        }
    }
}
