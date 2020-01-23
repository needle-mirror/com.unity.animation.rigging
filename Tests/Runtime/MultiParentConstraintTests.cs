using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class MultiParentConstraintTests
{
    const float k_Epsilon = 1e-5f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public MultiParentConstraint constraint;

        public AffineTransform constrainedObjectRestTx;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var multiParentGO = new GameObject("multiParent");
        var multiParent = multiParentGO.AddComponent<MultiParentConstraint>();
        multiParent.Reset();

        multiParentGO.transform.parent = data.rigData.rigGO.transform;

        multiParent.data.constrainedObject = data.rigData.hipsGO.transform;
        data.constrainedObjectRestTx = new AffineTransform(
            multiParent.data.constrainedObject.position,
            multiParent.data.constrainedObject.rotation
            );

        var sources = new WeightedTransformArray();
        var src0GO = new GameObject("source0");
        var src1GO = new GameObject("source1");
        src0GO.transform.parent = multiParentGO.transform;
        src1GO.transform.parent = multiParentGO.transform;
        sources.Add(new WeightedTransform(src0GO.transform, 0f));
        sources.Add(new WeightedTransform(src1GO.transform, 0f));
        multiParent.data.sourceObjects = sources;

        var pos = data.rigData.hipsGO.transform.position;
        var rot = data.rigData.hipsGO.transform.rotation;
        src0GO.transform.SetPositionAndRotation(pos, rot);
        src1GO.transform.SetPositionAndRotation(pos, rot);

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = multiParent;

        return data;
    }

    [UnityTest]
    public IEnumerator MultiParentConstraint_FollowSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);
        var rotationComprarer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        // src0.w = 0, src1.w = 0
        Assert.Zero(sources[0].weight);
        Assert.Zero(sources[1].weight);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(constrainedObject.position, Is.EqualTo(data.constrainedObjectRestTx.translation).Using(positionComparer));
        Assert.That(constrainedObject.rotation, Is.EqualTo(data.constrainedObjectRestTx.rotation).Using(rotationComprarer));

        // Add displacement to source objects
        sources[0].transform.position += Vector3.right;
        sources[0].transform.rotation *= Quaternion.AngleAxis(-90, Vector3.up);
        sources[1].transform.position += Vector3.left;
        sources[1].transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);

        // src0.w = 1, src1.w = 0
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(constrainedObject.position, Is.EqualTo(sources[0].transform.position).Using(positionComparer));
        Assert.That(constrainedObject.rotation, Is.EqualTo(sources[0].transform.rotation).Using(rotationComprarer));

        // src0.w = 0, src1.w = 1
        sources.SetWeight(0, 0f);
        sources.SetWeight(1, 1f);
        constraint.data.sourceObjects = sources;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(constrainedObject.position, Is.EqualTo(sources[1].transform.position).Using(positionComparer));
        Assert.That(constrainedObject.rotation, Is.EqualTo(sources[1].transform.rotation).Using(rotationComprarer));
    }

    [UnityTest]
    public IEnumerator MultiParentConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var constrainedObject = constraint.data.constrainedObject;
        var sources = constraint.data.sourceObjects;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);
        var rotationComprarer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        sources[0].transform.position += Vector3.right;
        sources[0].transform.rotation *= Quaternion.AngleAxis(-90, Vector3.up);
        sources.SetWeight(0, 1f);

        constraint.data.sourceObjects = sources;

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            var weightedPos = Vector3.Lerp(data.constrainedObjectRestTx.translation, sources[0].transform.position, w);
            Assert.That(
                constrainedObject.position,
                Is.EqualTo(weightedPos).Using(positionComparer),
                String.Format("Expected constrainedObject to be at {0} for a weight of {1}, but was {2}", weightedPos, w, constrainedObject.position)
                );

            var weightedRot = Quaternion.Lerp(data.constrainedObjectRestTx.rotation, sources[0].transform.rotation, w);
             Assert.That(
                constrainedObject.rotation,
                Is.EqualTo(weightedRot).Using(rotationComprarer),
                String.Format("Expected constrainedObject to be at {0} for a weight of {1}, but was {2}", weightedRot, w, constrainedObject.rotation)
                );
        }
    }
}
