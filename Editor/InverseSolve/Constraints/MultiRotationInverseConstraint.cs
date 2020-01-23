using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(MultiRotationConstraint))]
    public class MultiRotationInverseConstraint : OverrideRigConstraint<
        MultiRotationConstraint,
        MultiRotationInverseConstraintJob,
        MultiRotationConstraintData,
        MultiRotationInverseConstraintJobBinder<MultiRotationConstraintData>
        >
    {
        public MultiRotationInverseConstraint(MultiRotationConstraint baseConstraint) : base(baseConstraint) {}
    }
}

