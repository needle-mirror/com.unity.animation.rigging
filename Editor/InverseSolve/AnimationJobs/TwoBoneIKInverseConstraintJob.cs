using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [Unity.Burst.BurstCompile]
    public struct TwoBoneIKInverseConstraintJob : IWeightedAnimationJob
    {
        const float k_SqrEpsilon = 1e-8f;

        public ReadOnlyTransformHandle root;
        public ReadOnlyTransformHandle mid;
        public ReadOnlyTransformHandle tip;

        public ReadWriteTransformHandle hint;
        public ReadWriteTransformHandle target;

        public AffineTransform targetOffset;
        public Vector2 linkLengths;

        public FloatProperty targetPositionWeight;
        public FloatProperty targetRotationWeight;
        public FloatProperty hintWeight;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            tip.GetGlobalTR(stream, out var tipPosition, out var tipRotation);
            target.GetGlobalTR(stream, out var targetPosition, out var targetRotation);

            var positionWeight = targetPositionWeight.Get(stream);
            targetPosition = (positionWeight > 0f) ? tipPosition + targetOffset.translation : targetPosition;

            var rotationWeight = targetRotationWeight.Get(stream);
            targetRotation = (rotationWeight > 0f) ? tipRotation * targetOffset.rotation : targetRotation;

            target.SetGlobalTR(stream, targetPosition, targetRotation);

            if (hint.IsValid(stream))
            {
                var rootPosition = root.GetPosition(stream);
                var midPosition = mid.GetPosition(stream);

                var ac = tipPosition - rootPosition;
                var ab = midPosition - rootPosition;
                var bc = tipPosition - midPosition;

                var acSqrMag = Vector3.Dot(ac, ac);
                var projectionPoint = rootPosition;
                if (acSqrMag > k_SqrEpsilon)
                    projectionPoint += Vector3.Dot(ab / acSqrMag, ac) * ac;
                var poleVectorDirection = midPosition - projectionPoint;

                var weight = hintWeight.Get(stream);
                var hintPosition = hint.GetPosition(stream);
                var scale = linkLengths[0] + linkLengths[1];
                hintPosition = (weight > 0f) ? projectionPoint + (poleVectorDirection.normalized * scale) : hintPosition;
                hint.SetPosition(stream, hintPosition);
            }
        }
    }

    public class TwoBoneIKInverseConstraintJobBinder<T> : AnimationJobBinder<TwoBoneIKInverseConstraintJob, T>
        where T : struct, IAnimationJobData, ITwoBoneIKConstraintData
    {
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

            job.linkLengths[0] = Vector3.Distance(data.root.position, data.mid.position);
            job.linkLengths[1] = Vector3.Distance(data.mid.position, data.tip.position);

            return job;
        }

        public override void Destroy(TwoBoneIKInverseConstraintJob job)
        {
        }
    }
}
