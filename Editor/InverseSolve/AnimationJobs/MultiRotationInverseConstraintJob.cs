using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    public struct MultiRotationInverseConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadOnlyTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;
        public Vector3Property drivenOffset;

        public NativeArray<ReadWriteTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<Quaternion> sourceOffsets;

        public NativeArray<float> weightBuffer;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            var lRot = driven.GetLocalRotation(stream);
            var offset = drivenOffset.Get(stream);

            if (Vector3.Dot(offset, offset) > 0f)
                lRot *= Quaternion.Inverse(Quaternion.Euler(offset));

            var wRot = lRot;
            if (drivenParent.IsValid(stream))
            {
                wRot = drivenParent.GetRotation(stream) * wRot;
            }

            for (int i = 0; i < sourceTransforms.Length; ++i)
            {
                sourceWeights[i].SetFloat(stream, 1f);

                ReadWriteTransformHandle sourceTransform = sourceTransforms[i];

                sourceTransform.SetRotation(stream, wRot * sourceOffsets[i]);

                // Required to update handles with binding info.
                sourceTransforms[i] = sourceTransform;
            }
        }
    }

    public class MultiRotationInverseConstraintJobBinder<T> : AnimationJobBinder<MultiRotationInverseConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiRotationConstraintData
    {
        public override MultiRotationInverseConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiRotationInverseConstraintJob();

            job.driven = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
            job.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadWriteTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<Quaternion>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            job.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            Quaternion drivenRotInv = Quaternion.Inverse(data.constrainedObject.rotation);
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                job.sourceOffsets[i] = data.maintainOffset ?
                    (drivenRotInv * sourceObjects[i].transform.rotation) : Quaternion.identity;
            }

            return job;
        }

        public override void Destroy(MultiRotationInverseConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
