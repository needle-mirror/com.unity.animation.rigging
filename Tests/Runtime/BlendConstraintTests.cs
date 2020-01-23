using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class BlendConstraintTests
{
    const float k_Epsilon = 1e-5f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public BlendConstraint constraint;
        public AffineTransform restPose;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var blendConstraintGO = new GameObject("blendConstraint");
        var blendConstraint = blendConstraintGO.AddComponent<BlendConstraint>();
        blendConstraint.Reset();

        blendConstraintGO.transform.parent = data.rigData.rigGO.transform;

        var leftForeArm = data.rigData.hipsGO.transform.Find("Chest/LeftArm/LeftForeArm");
        var leftHand = leftForeArm.Find("LeftHand");

        blendConstraint.data.sourceObjectA = leftForeArm;
        blendConstraint.data.sourceObjectB = leftHand;

        var constrainedObject = new GameObject("constrainedBlendObj");
        constrainedObject.transform.parent = blendConstraintGO.transform;
        blendConstraint.data.constrainedObject = constrainedObject.transform;
        data.restPose = new AffineTransform(constrainedObject.transform.position, constrainedObject.transform.rotation);

        blendConstraint.data.positionWeight = blendConstraint.data.rotationWeight = 0.5f;
        blendConstraint.data.blendPosition = blendConstraint.data.blendRotation = true;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();
        data.constraint = blendConstraint;

        return data;
    }


    [UnityTest]
    public IEnumerator BlendConstraint_FollowsSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;
        var srcObjA = constraint.data.sourceObjectA;
        var srcObjB = constraint.data.sourceObjectB;
        var constrainedObj = constraint.data.constrainedObject;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);
        var rotationComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        // Apply rotation on sourceB
        srcObjB.rotation *= Quaternion.AngleAxis(90, Vector3.right);
        yield return null;

        // SourceA has full influence
        constraint.data.positionWeight = 0f;
        constraint.data.rotationWeight = 0f;
        yield return null;

        Assert.That(constrainedObj.position, Is.EqualTo(srcObjA.position).Using(positionComparer));
        Assert.That(constrainedObj.rotation, Is.EqualTo(srcObjA.rotation).Using(rotationComparer));      

        // SourceB has full influence
        constraint.data.positionWeight = 1f;
        constraint.data.rotationWeight = 1f;
        yield return null;

        Assert.That(constrainedObj.position, Is.EqualTo(srcObjB.position).Using(positionComparer));
        Assert.That(constrainedObj.rotation, Is.EqualTo(srcObjB.rotation).Using(rotationComparer));

        // Translation/Rotation blending between sources is disabled
        constraint.data.blendPosition = false;
        constraint.data.blendRotation = false;
        yield return null;

        Assert.That(constrainedObj.position, Is.EqualTo(data.restPose.translation).Using(positionComparer));
        Assert.That(constrainedObj.rotation, Is.EqualTo(data.restPose.rotation).Using(rotationComparer));
    }

    [UnityTest]
    public IEnumerator BlendConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;
        var srcObjB = constraint.data.sourceObjectB;
        var constrainedObj = constraint.data.constrainedObject;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);
        var rotationComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        // SourceB has full influence
        constraint.data.positionWeight = 1f;
        constraint.data.rotationWeight = 1f;
        srcObjB.rotation *= Quaternion.AngleAxis(90, Vector3.right);
        yield return null;

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            var weightedPos = Vector3.Lerp(data.restPose.translation, srcObjB.position, w);
            var weightedRot = Quaternion.Lerp(data.restPose.rotation, srcObjB.rotation, w);
            Assert.That(constrainedObj.position, Is.EqualTo(weightedPos).Using(positionComparer));
            Assert.That(constrainedObj.rotation, Is.EqualTo(weightedRot).Using(rotationComparer));
        }
    }
}
