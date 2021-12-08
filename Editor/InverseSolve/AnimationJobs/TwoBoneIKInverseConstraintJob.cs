using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    /// <summary>
    /// The TwoBoneIK inverse constraint job.
    /// </summary>
    [Unity.Burst.BurstCompile]
    public struct TwoBoneIKInverseConstraintJob : IWeightedAnimationJob
    {
        const float k_SqrEpsilon = 1e-8f;

        /// <summary>The transform handle for the root transform.</summary>
        public ReadOnlyTransformHandle root;
        /// <summary>The transform handle for the mid transform.</summary>
        public ReadOnlyTransformHandle mid;
        /// <summary>The transform handle for the tip transform.</summary>
        public ReadOnlyTransformHandle tip;

        /// <summary>The transform handle for the hint transform.</summary>
        public ReadWriteTransformHandle hint;
        /// <summary>The transform handle for the target transform.</summary>
        public ReadWriteTransformHandle target;

        /// <summary>The offset applied to the target transform if maintainTargetPositionOffset or maintainTargetRotationOffset is enabled.</summary>
        public AffineTransform targetOffset;

        /// <summary>The weight for which target position has an effect on IK calculations. This is a value in between 0 and 1.</summary>
        public FloatProperty targetPositionWeight;
        /// <summary>The weight for which target rotation has an effect on IK calculations. This is a value in between 0 and 1.</summary>
        public FloatProperty targetRotationWeight;
        /// <summary>The weight for which hint transform has an effect on IK calculations. This is a value in between 0 and 1.</summary>
        public FloatProperty hintWeight;

        /// <inheritdoc />
        public FloatProperty jobWeight { get; set; }

        /// <summary>
        /// Defines what to do when processing the root motion.
        /// </summary>
        /// <param name="stream">The animation stream to work on.</param>
        public void ProcessRootMotion(AnimationStream stream) { }

        /// <summary>
        /// Defines what to do when processing the animation.
        /// </summary>
        /// <param name="stream">The animation stream to work on.</param>
        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            AnimationRuntimeUtils.InverseSolveTwoBoneIK(stream, root, mid, tip, target, hint,
                targetPositionWeight.Get(stream),
                targetRotationWeight.Get(stream),
                hintWeight.Get(stream), targetOffset);
        }
    }

    /// <summary>
    /// The TwoBoneIK inverse constraint job binder.
    /// </summary>
    /// <typeparam name="T">The constraint data type</typeparam>
    public class TwoBoneIKInverseConstraintJobBinder<T> : AnimationJobBinder<TwoBoneIKInverseConstraintJob, T>
        where T : struct, IAnimationJobData, ITwoBoneIKConstraintData
    {
        /// <inheritdoc />
        public override TwoBoneIKInverseConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new TwoBoneIKInverseConstraintJob();

            job.root = ReadOnlyTransformHandle.Bind(animator, data.root);
            job.mid = ReadOnlyTransformHandle.Bind(animator, data.mid);
            job.tip = ReadOnlyTransformHandle.Bind(animator, data.tip);
            job.target = ReadWriteTransformHandle.Bind(animator, data.target);

            if (data.hint != null)
                job.hint = ReadWriteTransformHandle.Bind(animator, data.hint);

            job.targetOffset = AffineTransform.identity;
            if (data.maintainTargetPositionOffset)
                job.targetOffset.translation = -(data.tip.position - data.target.position);
            if (data.maintainTargetRotationOffset)
                job.targetOffset.rotation = Quaternion.Inverse(data.tip.rotation) * data.target.rotation;

            job.targetPositionWeight = FloatProperty.Bind(animator, component, data.targetPositionWeightFloatProperty);
            job.targetRotationWeight = FloatProperty.Bind(animator, component, data.targetRotationWeightFloatProperty);
            job.hintWeight = FloatProperty.Bind(animator, component, data.hintWeightFloatProperty);

            return job;
        }

        /// <inheritdoc />
        public override void Destroy(TwoBoneIKInverseConstraintJob job)
        {
        }
    }
}
