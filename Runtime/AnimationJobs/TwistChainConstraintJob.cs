using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    [Unity.Burst.BurstCompile]
    public struct TwistChainConstraintJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle rootTarget;
        public ReadWriteTransformHandle tipTarget;

        public NativeArray<ReadWriteTransformHandle> chain;

        public NativeArray<float> steps;
        public NativeArray<float> weights;
        public NativeArray<Quaternion> rotations;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                // Retrieve root and tip rotation.
                Quaternion rootRotation = rootTarget.GetRotation(stream);
                Quaternion tipRotation = tipTarget.GetRotation(stream);

                // Interpolate rotation on chain.
                chain[0].SetRotation(stream, Quaternion.Lerp(chain[0].GetRotation(stream), rootRotation, w));
                for (int i = 1; i < chain.Length - 1; ++i)
                {
                    chain[i].SetRotation(stream, Quaternion.Lerp(chain[i].GetRotation(stream), rotations[i] * Quaternion.Lerp(rootRotation, tipRotation, weights[i]), w));
                }
                chain[chain.Length - 1].SetRotation(stream, Quaternion.Lerp(chain[chain.Length - 1].GetRotation(stream), tipRotation, w));

#if UNITY_EDITOR
                // Update position of tip handle for easier visualization.
                rootTarget.SetPosition(stream, chain[0].GetPosition(stream));
                tipTarget.SetPosition(stream, chain[chain.Length - 1].GetPosition(stream));
#endif
            }
            else
            {
                for (int i = 0; i < chain.Length; ++i)
                    AnimationRuntimeUtils.PassThrough(stream, chain[i]);
            }
        }
    }

    public interface ITwistChainConstraintData
    {
        Transform root { get; }
        Transform tip { get; }

        Transform rootTarget { get; }
        Transform tipTarget { get; }

        AnimationCurve curve { get; }
    }

    public class TwistChainConstraintJobBinder<T> : AnimationJobBinder<TwistChainConstraintJob, T>
        where T : struct, IAnimationJobData, ITwistChainConstraintData
    {
        public override TwistChainConstraintJob Create(Animator animator, ref T data, Component component)
        {
            // Retrieve chain in-between root and tip transforms.
            Transform[] chain = ConstraintsUtils.ExtractChain(data.root, data.tip);

            // Extract steps from chain.
            float[] steps = ConstraintsUtils.ExtractSteps(chain);

            // Build Job.
            var job = new TwistChainConstraintJob();
            job.chain = new NativeArray<ReadWriteTransformHandle>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.steps = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.weights = new NativeArray<float>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.rotations = new NativeArray<Quaternion>(chain.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            job.rootTarget = ReadWriteTransformHandle.Bind(animator, data.rootTarget);
            job.tipTarget = ReadWriteTransformHandle.Bind(animator, data.tipTarget);

            // Set values in NativeArray.
            for (int i = 0; i < chain.Length; ++i)
            {
                job.chain[i] = ReadWriteTransformHandle.Bind(animator, chain[i]);
                job.steps[i] = steps[i];
                job.weights[i] = Mathf.Clamp01(data.curve.Evaluate(steps[i]));
            }

            job.rotations[0] = Quaternion.identity;
            job.rotations[chain.Length - 1] = Quaternion.identity;
            for (int i = 1; i < chain.Length - 1; ++i)
            {
                job.rotations[i] = Quaternion.Inverse(Quaternion.Lerp(chain[0].rotation, chain[chain.Length - 1].rotation, job.weights[i])) * chain[i].rotation;
            }


            return job;
        }

        public override void Destroy(TwistChainConstraintJob job)
        {
            job.chain.Dispose();
            job.weights.Dispose();
            job.steps.Dispose();
            job.rotations.Dispose();
        }

#if UNITY_EDITOR
        public override void Update(TwistChainConstraintJob job, ref T data)
        {
            // Update weights based on curve.
            for (int i = 0; i < job.steps.Length; ++i)
            {
                job.weights[i] = Mathf.Clamp01(data.curve.Evaluate(job.steps[i]));
            }
        }
#endif
    }
}

