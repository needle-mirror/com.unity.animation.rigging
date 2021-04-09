using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    /// <summary>
    /// MultiParent inverse constraint.
    /// </summary>
    /// <seealso cref="MultiParentConstraint"/>
    [InverseRigConstraint(typeof(MultiParentConstraint))]
    public class MultiParentInverseConstraint : OverrideRigConstraint<
        MultiParentConstraint,
        MultiParentInverseConstraintJob,
        MultiParentConstraintData,
        MultiParentInverseConstraintJobBinder<MultiParentConstraintData>
        >
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseConstraint">Base constraint to override.</param>
        public MultiParentInverseConstraint(MultiParentConstraint baseConstraint) : base(baseConstraint) { }
    }
}
