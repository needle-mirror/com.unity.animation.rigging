using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class MultiReferentialConstraintTests
{
    const float k_Epsilon = 1e-5f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public MultiReferentialConstraint constraint;

        public AffineTransform restPose;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var multiRefGO = new GameObject("multiReferential");
        var multiRef = multiRefGO.AddComponent<MultiReferentialConstraint>();
        multiRef.Reset();

        multiRefGO.transform.parent = data.rigData.rigGO.transform;

        List<Transform> sources = new List<Transform>(3);
        var src0GO = new GameObject("source0");
        var src1GO = new GameObject("source1");
        src0GO.transform.parent = multiRefGO.transform;
        src1GO.transform.parent = multiRefGO.transform;
        sources.Add(data.rigData.hipsGO.transform);
        sources.Add(src0GO.transform);
        sources.Add(src1GO.transform);
        multiRef.data.sourceObjects = sources;
        multiRef.data.driver = 0;

        var pos = data.rigData.hipsGO.transform.position;
        var rot = data.rigData.hipsGO.transform.rotation;
        src0GO.transform.SetPositionAndRotation(pos, rot);
        src1GO.transform.SetPositionAndRotation(pos, rot);
        data.restPose = new AffineTransform(pos, rot);

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        data.constraint = multiRef;

        return data;
    }

    [UnityTest]
    public IEnumerator MultiReferentialConstraint_FollowSourceObjects()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);
        var rotationComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        var sources = constraint.data.sourceObjects;

        constraint.data.driver = 0;
        var driver = sources[0];
        driver.position += Vector3.forward;
        driver.rotation *= Quaternion.AngleAxis(90, Vector3.up);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(driver.position, Is.EqualTo(sources[1].position).Using(positionComparer));
        Assert.That(driver.rotation, Is.EqualTo(sources[1].rotation).Using(rotationComparer));
        Assert.That(driver.position, Is.EqualTo(sources[2].position).Using(positionComparer));
        Assert.That(driver.rotation, Is.EqualTo(sources[2].rotation).Using(rotationComparer));

        constraint.data.driver = 1;
        driver = sources[1];
        driver.position += Vector3.back;
        driver.rotation *= Quaternion.AngleAxis(-90, Vector3.up);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(driver.position, Is.EqualTo(sources[0].position).Using(positionComparer));
        Assert.That(driver.rotation, Is.EqualTo(sources[0].rotation).Using(rotationComparer));
        Assert.That(driver.position, Is.EqualTo(sources[2].position).Using(positionComparer));
        Assert.That(driver.rotation, Is.EqualTo(sources[2].rotation).Using(rotationComparer));

        constraint.data.driver = 2;
        driver = sources[2];
        driver.position += Vector3.up;
        driver.rotation *= Quaternion.AngleAxis(90, Vector3.left);
        yield return RuntimeRiggingTestFixture.YieldTwoFrames();

        Assert.That(driver.position, Is.EqualTo(sources[0].position).Using(positionComparer));
        Assert.That(driver.rotation, Is.EqualTo(sources[0].rotation).Using(rotationComparer));
        Assert.That(driver.position, Is.EqualTo(sources[1].position).Using(positionComparer));
        Assert.That(driver.rotation, Is.EqualTo(sources[1].rotation).Using(rotationComparer));
    }

    [UnityTest]
    public IEnumerator MultiReferentialConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var sources = constraint.data.sourceObjects;

        constraint.data.driver = 1;
        constraint.weight = 0f;
        yield return null;

        sources[1].position += Vector3.right;
        sources[1].rotation *= Quaternion.AngleAxis(-90, Vector3.up);

        var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);
        var rotationComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            constraint.weight = w;
            yield return null;

            var weightedPos = Vector3.Lerp(data.restPose.translation, sources[1].position, w);

            Assert.That(
                sources[0].position,
                Is.EqualTo(weightedPos).Using(positionComparer),
                String.Format("Expected Source0 to be at {0} for a weight of {1}, but was {2}", weightedPos, w, sources[0].position)
                );
            Assert.That(
                sources[2].position,
                Is.EqualTo(weightedPos).Using(positionComparer),
                String.Format("Expected Source2 to be at {0} for a weight of {1}, but was {2}", weightedPos, w, sources[2].position)
                );

            var weightedRot = Quaternion.Lerp(data.restPose.rotation, sources[1].rotation, w);
            Assert.That(
                sources[0].rotation,
                Is.EqualTo(weightedRot).Using(rotationComparer),
                String.Format("Expected rotations to be equal for a weight of {0}", w)
                );
            Assert.That(
                sources[2].rotation,
                Is.EqualTo(weightedRot).Using(rotationComparer),
                String.Format("Expected rotations to be equal for a weight of {0}", w)
                );

            // Since we have no animation in the stream the new rest pose
            // should be the last evaluated one.
            data.restPose.translation = sources[0].position;
            data.restPose.rotation = sources[0].rotation;
        }
    }
}
