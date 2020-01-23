using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace UnityEditor.Animations.Rigging
{
    [Unity.Burst.BurstCompile]
    public struct MultiAimInverseConstraintJob : IWeightedAnimationJob
    {
        const float k_Epsilon = 1e-5f;

        public ReadOnlyTransformHandle driven;
        public ReadOnlyTransformHandle drivenParent;
        public Vector3Property drivenOffset;

        public NativeArray<ReadWriteTransformHandle> sourceTransforms;
        public NativeArray<PropertyStreamHandle> sourceWeights;
        public NativeArray<Quaternion> sourceOffsets;

        public NativeArray<float> weightBuffer;

        public Vector3 aimAxis;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(AnimationStream stream) { }

        public void ProcessAnimation(AnimationStream stream)
        {
            jobWeight.Set(stream, 1f);

            var lRot = driven.GetLocalRotation(stream);
            var offset = drivenOffset.Get(stream);

            if (Vector3.Dot(offset, offset) > 0f)
                lRot *= Quaternion.Inverse(Quaternion.Euler(offset));

            var localToWorld = Quaternion.identity;
            var wPos = driven.GetPosition(stream);
            if (drivenParent.IsValid(stream))
                localToWorld = drivenParent.GetRotation(stream);

            for (int i = 0; i < sourceTransforms.Length; ++i)
            {
                sourceWeights[i].SetFloat(stream, 1f);

                var sourceTransform = sourceTransforms[i];

                sourceTransform.SetPosition(stream, wPos + localToWorld * sourceOffsets[i] * lRot * aimAxis);

                // Required to update handles with binding info.
                sourceTransforms[i] = sourceTransform;
            }
        }
    }
    public class MultiAimInverseConstraintJobBinder<T> : AnimationJobBinder<MultiAimInverseConstraintJob, T>
        where T : struct, IAnimationJobData, IMultiAimConstraintData
    {
        public override MultiAimInverseConstraintJob Create(Animator animator, ref T data, Component component)
        {
            var job = new MultiAimInverseConstraintJob();

            job.driven = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject);
            job.drivenParent = ReadOnlyTransformHandle.Bind(animator, data.constrainedObject.parent);
            job.drivenOffset = Vector3Property.Bind(animator, component, data.offsetVector3Property);
            job.aimAxis = data.aimAxis;

            WeightedTransformArray sourceObjects = data.sourceObjects;

            WeightedTransformArrayBinder.BindReadWriteTransforms(animator, component, sourceObjects, out job.sourceTransforms);
            WeightedTransformArrayBinder.BindWeights(animator, component, sourceObjects, data.sourceObjectsProperty, out job.sourceWeights);

            job.sourceOffsets = new NativeArray<Quaternion>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < sourceObjects.Count; ++i)
            {
                if (data.maintainOffset)
                {
                    var aimDirection = data.constrainedObject.rotation * data.aimAxis;
                    var dataToSource = sourceObjects[i].transform.position - data.constrainedObject.position;
                    var rot = QuaternionExt.FromToRotation(dataToSource, aimDirection);
                    job.sourceOffsets[i] = Quaternion.Inverse(rot);
                }
                else
                {
                    job.sourceOffsets[i] = Quaternion.identity;
                }
            }

            job.weightBuffer = new NativeArray<float>(sourceObjects.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            return job;
        }

        public override void Destroy(MultiAimInverseConstraintJob job)
        {
            job.sourceTransforms.Dispose();
            job.sourceWeights.Dispose();
            job.sourceOffsets.Dispose();
            job.weightBuffer.Dispose();
        }
    }
}
