using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [Unity.Burst.BurstCompile]
    public struct MultiParentInverseConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadOnlyTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;

        public NativeArray<ReadWriteTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<AffineTransform> sourceOffsets;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            var drivenLocalTx = new AffineTransform(driven.GetLocalPosition(stream), driven.GetLocalRotation(stream));
            var parentTx = new AffineTransform();

            // Convert accumTx to local space
            if (drivenParent.IsValid(stream))
            {
                drivenParent.GetGlobalTR(stream, out Vector3 parentWPos, out Quaternion parentWRot);
                parentTx = new AffineTransform(parentWPos, parentWRot);
            }

            var drivenTx = parentTx * drivenLocalTx;

            for (int i = 0; i < sourceTransforms.Length; ++i)
            {
                sourceWeights[i].SetFloat(stream, 1f);

                var sourceTransform = sourceTransforms[i];
                sourceTransform.GetGlobalTR(stream, out var sourcePosition, out var sourceRotation);

                var result = drivenTx;
                result *= sourceOffsets[i];

                sourceTransform.SetGlobalTR(stream, result.translation, result.rotation);
                sourceTransforms[i] = sourceTransform;
            }
        }
    }

    public class MultiParentInverseConstraintJobBinder<T> : AnimationJobBinder<MultiParentInverseConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiParentConstraintData
    {
        public override MultiParentInverseConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiParentInverseConstraintJob();

            job.driven = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadWriteTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<AffineTransform>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            var drivenTx = new AffineTransform(data.constrainedObject.position, data.constrainedObject.rotation);
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                var sourceTransform = sourceObjects[i].transform;

                var srcTx = new AffineTransform(sourceTransform.position, sourceTransform.rotation);
                var srcOffset = AffineTransform.identity;
                var tmp = srcTx.InverseMul(drivenTx);

                if (data.maintainPositionOffset)
                    srcOffset.translation = tmp.translation;
                if (data.maintainRotationOffset)
                    srcOffset.rotation = tmp.rotation;

                srcOffset = srcOffset.Inverse();

                job.sourceOffsets[i] = srcOffset;
            }

            return job;
        }

        public override void Destroy(MultiParentInverseConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
        }
    }
}
