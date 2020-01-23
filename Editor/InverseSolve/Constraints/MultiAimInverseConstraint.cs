using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(MultiAimConstraint))]
    public class MultiAimInverseConstraint : OverrideRigConstraint<
        MultiAimConstraint,
        MultiAimInverseConstraintJob,
        MultiAimConstraintData,
        MultiAimInverseConstraintJobBinder<MultiAimConstraintData>
        >
    {
        public MultiAimInverseConstraint(MultiAimConstraint baseConstraint) : base(baseConstraint) {}
    }
}

