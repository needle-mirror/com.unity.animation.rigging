using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [Unity.Burst.BurstCompile]
    public struct MultiPositionInverseConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadOnlyTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;
        public Vector3Property drivenOffset;

        public NativeArray<ReadWriteTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<Vector3> sourceOffsets;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            var drivenPos = driven.GetLocalPosition(stream);
            var offset = drivenOffset.Get(stream);

            var lPos = drivenPos - offset;

            var parentTx = new AffineTransform();
            if (drivenParent.IsValid(stream))
            {
                drivenParent.GetGlobalTR(stream, out Vector3 parentWPos, out Quaternion parentWRot);
                parentTx = new AffineTransform(parentWPos, parentWRot);
            }

            var wPos = parentTx.Transform(lPos);
            for (int i = 0; i < sourceTransforms.Length; ++i)
            {
                sourceWeights[i].SetFloat(stream, 1f);

                ReadWriteTransformHandle sourceTransform = sourceTransforms[i];

                sourceTransform.SetPosition(stream, wPos - sourceOffsets[i]);

                // Required to update handles with binding info.
                sourceTransforms[i] = sourceTransform;
            }
        }
    }

    public class MultiPositionInverseConstraintJobBinder<T> : AnimationJobBinder<MultiPositionInverseConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiPositionConstraintData
    {
        public override MultiPositionInverseConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiPositionInverseConstraintJob();

            job.driven = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
            job.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadWriteTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<Vector3>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            Vector3 drivenPos = data.constrainedObject.position;
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                job.sourceOffsets[i] = data.maintainOffset ? (drivenPos - sourceObjects[i].transform.position) : Vector3.zero;
            }

            return job;
        }

        public override void Destroy(MultiPositionInverseConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
        }
    }
}
