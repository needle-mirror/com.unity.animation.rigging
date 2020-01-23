using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [InverseRigConstraint(typeof(TwistChainConstraint))]
    public class TwistChainInverseConstraint : OverrideRigConstraint<
        TwistChainConstraint,
        TwistChainInverseConstraintJob,
        TwistChainConstraintData,
        TwistChainInverseConstraintJobBinder<TwistChainConstraintData>
        >
    {
        public TwistChainInverseConstraint(TwistChainConstraint baseConstraint) : base(baseConstraint) {}
    }
}

