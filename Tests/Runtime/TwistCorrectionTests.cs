using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

class TwistCorrectionTests
{
    const float k_Epsilon = .005f;

    struct ConstraintTestData
    {
        public RigTestData rigData;
        public TwistCorrection constraint;

        public Quaternion restLocalRotation;
    }

    private ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var twistCorrectionGO = new GameObject("twistCorrection");
        var twistCorrection = twistCorrectionGO.AddComponent<TwistCorrection>();
        twistCorrection.Reset();

        twistCorrectionGO.transform.parent = data.rigData.rigGO.transform;

        var leftArm = data.rigData.hipsGO.transform.Find("Chest/LeftArm");
        var leftForeArm = leftArm.Find("LeftForeArm");
        var leftHand = leftForeArm.Find("LeftHand");

        // Force zero rotation to simplify testing
        leftHand.rotation = Quaternion.identity;
        leftForeArm.rotation = Quaternion.identity;
        leftArm.rotation = Quaternion.identity;

        twistCorrection.data.sourceObject = leftHand;
        twistCorrection.data.twistAxis = TwistCorrectionData.Axis.X;
        data.restLocalRotation = leftHand.localRotation;

        var twistNodes = new WeightedTransformArray();
        var twistNode0GO = new GameObject("twistNode0");
        var twistNode1GO = new GameObject("twistNode1");
        twistNode0GO.transform.parent = leftForeArm;
        twistNode1GO.transform.parent = leftForeArm;
        twistNode0GO.transform.SetPositionAndRotation(Vector3.Lerp(leftForeArm.position, leftHand.position, 0.25f), leftHand.rotation);
        twistNode1GO.transform.SetPositionAndRotation(Vector3.Lerp(leftForeArm.position, leftHand.position, 0.75f), leftHand.rotation);
        twistNodes.Add(new WeightedTransform(twistNode0GO.transform, 0f));
        twistNodes.Add(new WeightedTransform(twistNode1GO.transform, 0f));
        twistCorrection.data.twistNodes = twistNodes;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = twistCorrection;

        return data;
    }

    [UnityTest]
    public IEnumerator TwistCorrection_FollowsSourceObject()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var sourceObject = constraint.data.sourceObject;
        var twistNodes = constraint.data.twistNodes;

        var rotationComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);
        var floatComparer = new RuntimeRiggingTestFixture.FloatEqualityComparer(k_Epsilon);

        // Apply rotation to source object
        sourceObject.localRotation = sourceObject.localRotation * Quaternion.AngleAxis(90, Vector3.left);

        // twistNode0.w = 0.0f, twistNode1.w = 0.0f [does not influence twist nodes]
        Assert.AreEqual(twistNodes[0].weight, 0.0f);
        Assert.AreEqual(twistNodes[1].weight, 0.0f);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(sourceObject.localRotation, Is.Not.EqualTo(data.restLocalRotation).Using(rotationComparer));
        Assert.That(twistNodes[0].transform.localRotation, Is.EqualTo(Quaternion.identity).Using(rotationComparer));
        Assert.That(twistNodes[1].transform.localRotation, Is.EqualTo(Quaternion.identity).Using(rotationComparer));

        // twistNode0.w = 1f, twistNode1.w = 1f [twist nodes should be equal to source]
        twistNodes.SetWeight(0, 1f);
        twistNodes.SetWeight(1, 1f);
        constraint.data.twistNodes = twistNodes;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        // Verify twist on X axis
        Assert.That(twistNodes[0].transform.localRotation.x, Is.EqualTo(sourceObject.localRotation.x).Using(floatComparer));
        Assert.That(twistNodes[0].transform.localRotation.w, Is.EqualTo(sourceObject.localRotation.w).Using(floatComparer));
        Assert.That(twistNodes[1].transform.localRotation.x, Is.EqualTo(sourceObject.localRotation.x).Using(floatComparer));
        Assert.That(twistNodes[1].transform.localRotation.w, Is.EqualTo(sourceObject.localRotation.w).Using(floatComparer));

        // twistNode0.w = -1f, twistNode1.w = -1f [twist nodes should be inverse to source]
        twistNodes.SetWeight(0, -1f);
        twistNodes.SetWeight(1, -1f);
        constraint.data.twistNodes = twistNodes;
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        var invTwist = Quaternion.Inverse(sourceObject.localRotation);
        // Verify twist on X axis
        Assert.That(twistNodes[0].transform.localRotation.x, Is.EqualTo(invTwist.x).Using(floatComparer));
        Assert.That(twistNodes[0].transform.localRotation.w, Is.EqualTo(invTwist.w).Using(floatComparer));
        Assert.That(twistNodes[1].transform.localRotation.x, Is.EqualTo(invTwist.x).Using(floatComparer));
        Assert.That(twistNodes[1].transform.localRotation.w, Is.EqualTo(invTwist.w).Using(floatComparer));
    }

    [UnityTest]
    public IEnumerator TwistCorrection_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var sourceObject = constraint.data.sourceObject;
        var twistNodes = constraint.data.twistNodes;

        var floatComparer = new RuntimeRiggingTestFixture.FloatEqualityComparer(k_Epsilon);

        // Apply rotation to source object
        sourceObject.localRotation = sourceObject.localRotation * Quaternion.AngleAxis(90, Vector3.left);
        twistNodes.SetWeight(0, 1f);
        constraint.data.twistNodes = twistNodes;

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            var weightedRot = Quaternion.Lerp(data.restLocalRotation, sourceObject.localRotation, w);
            Assert.That(twistNodes[0].transform.localRotation.x, Is.EqualTo(weightedRot.x).Using(floatComparer));
            Assert.That(twistNodes[0].transform.localRotation.w, Is.EqualTo(weightedRot.w).Using(floatComparer));
        }
    }
}
