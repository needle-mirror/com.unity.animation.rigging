using System;

namespace UnityEngine.Animations.Rigging
{
    /// <summary>
    /// By default, <see cref="WeightedTransform.weight"/> appears as a numeric input field in the Inspector.
    /// Decorate <see cref="WeightedTransform"/> or <see cref="WeightedTransformArray"/> fields with this attribute to make it display using a slider with the specified range.
    /// See also <seealso cref="WeightedTransformArray.OnValidate"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class WeightRangeAttribute : PropertyAttribute
    {
        /// <summary>
        /// The smallest permissible value the weight may have.
        /// </summary>
        public readonly float min = 0f;
        /// <summary>
        /// The largest permissible value the weight may have.
        /// </summary>
        public readonly float max = 1f;

        /// <summary>
        /// Constructs a new <see cref="WeightRangeAttribute"/> instance with the specified range.
        /// A value of <see cref="Single.NaN"/> for either end of the range will permit any value to be entered.
        /// </summary>
        /// <param name="min">The smallest permissible value the weight may have.</param>
        /// <param name="max">The largest permissible value the weight may have.</param>
        public WeightRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
