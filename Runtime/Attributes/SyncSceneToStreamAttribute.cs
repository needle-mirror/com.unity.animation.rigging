using System;

namespace UnityEngine.Animations.Rigging
{
    /// <summary>
    /// The [SyncSceneToStream] attribute can be used to ensure constraints properties are read from the scene
    /// and written back in the AnimationStream if they were not previously animated.
    /// Supported value types are: Float, Int, Bool, Vector2, Vector3, Vector4, Quaternion, Vector3Int, Vector3Bool,
    /// Transform, Transform[], WeightedTransform and WeightedTransformArray.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SyncSceneToStreamAttribute : Attribute { }

    internal enum PropertyType : byte { Bool, Int, Float };

    internal struct PropertyDescriptor
    {
        public int size;
        public PropertyType type;
    }

    internal struct Property
    {
        public string name;
        public PropertyDescriptor descriptor;
    }

    internal struct RigProperties
    {
        public static string s_Weight = "m_Weight";
        public Component component;
    }

    internal struct ConstraintProperties
    {
        public static string s_Weight = "m_Weight";
        public Component component;
        public Property[] properties;
    }

    /// <summary>
    /// Utility functions for constraints (deprecated).
    /// </summary>
    [Obsolete("PropertyUtils is deprecated. Use ConstraintsUtils instead. (UnityUpgradable) -> ConstraintsUtils")]
    public static class PropertyUtils
    {
        /// <summary>
        /// Prepends RigConstraint data property to specified property name.
        /// </summary>
        /// <param name="property">Property name.</param>
        /// <returns>Return a complete property name.</returns>
        [Obsolete("ConstructConstraintDataPropertyName is deprecated. Use ConstraintsUtils.ConstructConstraintDataPropertyName instead.")]
        public static string ConstructConstraintDataPropertyName(string property)
        {
            return ConstraintsUtils.ConstructConstraintDataPropertyName(property);
        }

        /// <summary>
        /// Builds a unique property name for a custom property.
        /// </summary>
        /// <param name="component">Associated component.</param>
        /// <param name="property">Property name.</param>
        /// <returns>Returns a custom property name.</returns>
        [Obsolete("ConstructCustomPropertyName is deprecated. Use ConstraintsUtils.ConstructCustomPropertyName instead.")]
        public static string ConstructCustomPropertyName(Component component, string property)
        {
            return ConstraintsUtils.ConstructCustomPropertyName(component, property);
        }
    }
}
