using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(MultiPositionConstraint))]
    public class MultiPositionInverseConstraint : OverrideRigConstraint<
        MultiPositionConstraint,
        MultiPositionInverseConstraintJob,
        MultiPositionConstraintData,
        MultiPositionInverseConstraintJobBinder<MultiPositionConstraintData>
        >
    {
        public MultiPositionInverseConstraint(MultiPositionConstraint baseConstraint) : base(baseConstraint) {}
    }
}

