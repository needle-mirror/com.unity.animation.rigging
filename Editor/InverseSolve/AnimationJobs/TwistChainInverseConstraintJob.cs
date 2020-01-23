using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [Unity.Burst.BurstCompile]
    public struct TwistChainInverseConstraintJob : IWeightedAnimationJob
    {
        public ReadOnlyTransformHandle root;
        public ReadOnlyTransformHandle tip;

        public ReadWriteTransformHandle rootTarget;
        public ReadWriteTransformHandle tipTarget;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            rootTarget.SetPosition(stream, root.GetPosition(stream));
            rootTarget.SetRotation(stream, root.GetRotation(stream));

            tipTarget.SetPosition(stream, tip.GetPosition(stream));
            tipTarget.SetRotation(stream, tip.GetRotation(stream));
        }
    }

    public class TwistChainInverseConstraintJobBinder<T> : AnimationJobBinder<TwistChainInverseConstraintJob, T>
        where T : struct, IAnimationJobData, ITwistChainConstraintData
    {
        public override TwistChainInverseConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new TwistChainInverseConstraintJob();

            job.root = ReadOnlyTransformHandle.Bind(animator, data.root);
            job.tip = ReadOnlyTransformHandle.Bind(animator, data.tip);

            job.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
            job.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);

            return job;
        }

        public override void Destroy(TwistChainInverseConstraintJob job)
        {
        }
    }
}
