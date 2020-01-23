using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Animations.Rigging;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using RigTestData = RuntimeRiggingTestFixture.RigTestData;

public class TwistChainConstraintTests {
    const float k_Epsilon = 1e-5f;

    public struct ConstraintTestData
    {
        public RigTestData rigData;
        public TwistChainConstraint constraint;
    }

    public static ConstraintTestData SetupConstraintRig()
    {
        var data = new ConstraintTestData();

        data.rigData = RuntimeRiggingTestFixture.SetupRigHierarchy();

        var twistChainGO = new GameObject("twistChain");
        var twistChain = twistChainGO.AddComponent<TwistChainConstraint>();
        twistChain.Reset();

        twistChainGO.transform.parent = data.rigData.rigGO.transform;

        twistChain.data.root = data.rigData.hipsGO.transform.Find("Chest/LeftArm");
        Assert.IsNotNull(twistChain.data.root, "Could not find root transform");

        twistChain.data.tip = twistChain.data.root.transform.Find("LeftForeArm/LeftHand");
        Assert.IsNotNull(twistChain.data.tip, "Could not find tip transform");

        var rootTargetGO = new GameObject ("rootTarget");
        rootTargetGO.transform.parent = twistChainGO.transform;

        var tipTargetGO = new GameObject ("tipTarget");
        tipTargetGO.transform.parent = twistChainGO.transform;

        twistChain.data.rootTarget = rootTargetGO.transform;
        twistChain.data.tipTarget = tipTargetGO.transform;

        data.rigData.rootGO.GetComponent<RigBuilder>().Build();

        rootTargetGO.transform.position = twistChain.data.root.position;
        rootTargetGO.transform.rotation = twistChain.data.root.rotation;

        tipTargetGO.transform.position = twistChain.data.tip.position;
        tipTargetGO.transform.rotation = twistChain.data.tip.rotation;

        data.constraint = twistChain;

        return data;
    }

    [UnityTest]
    public IEnumerator TwistChainConstraint_FollowsTargets()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        var tipTarget = constraint.data.tipTarget;
        var rootTarget = constraint.data.rootTarget;

        var tip = constraint.data.tip;
        var root = constraint.data.root;

        var quaternionComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_Epsilon);

        for (int i = 0; i < 5; ++i)
        {
            tipTarget.rotation = tipTarget.rotation *  Quaternion.Euler(0, 0, 10);
            yield return RuntimeRiggingTestFixture.YieldTwoFrames();

            Assert.That(tip.rotation, Is.EqualTo(tipTarget.rotation).Using(quaternionComparer));
            Assert.That(root.rotation, Is.EqualTo(rootTarget.rotation).Using(quaternionComparer));
        }

        for (int i = 0; i < 5; ++i)
        {
            rootTarget.rotation = rootTarget.rotation *  Quaternion.Euler(0, 0, -10);
            yield return RuntimeRiggingTestFixture.YieldTwoFrames();

            Assert.That(root.rotation, Is.EqualTo(rootTarget.rotation).Using(quaternionComparer));
            Assert.That(tip.rotation, Is.EqualTo(tipTarget.rotation).Using(quaternionComparer));
        }
    }

    [UnityTest]
    public IEnumerator TwistChainConstraint_ApplyWeight()
    {
        var data = SetupConstraintRig();
        var constraint = data.constraint;

        Transform[] chain = ConstraintsUtils.ExtractChain(constraint.data.root, constraint.data.tip);

        // Chain with no constraint.
        Vector3[] bindPoseChain = chain.Select(transform => transform.position).ToArray();

        var tipTarget = constraint.data.tipTarget;
        var rootTarget = constraint.data.rootTarget;

        rootTarget.rotation = Quaternion.Euler(0, 0, 0);
        tipTarget.rotation = Quaternion.Euler(0, 0, 50);

        yield return null;

        // Chain with TwistChain constraint.
        Vector3[] weightedChain = chain.Select(transform => transform.position).ToArray();

        // In-between chains.
        var inBetweenChains = new List<Vector3[]>();
        for (int i = 0; i <= 5; ++i)
        {
            float w = i / 5.0f;

            data.constraint.weight = w;
            yield return null;

            inBetweenChains.Add(chain.Select(transform => transform.position).ToArray());
        }

        var floatComparer = new RuntimeRiggingTestFixture.FloatEqualityComparer(k_Epsilon);

        for (int i = 0; i <= 5; ++i)
        {
            Vector3[] prevChain = (i > 0) ? inBetweenChains[i - 1] : bindPoseChain;
            Vector3[] currentChain = inBetweenChains[i];
            Vector3[] nextChain = (i < 5) ? inBetweenChains[i + 1] : weightedChain;

            for (int j = 0; j < bindPoseChain.Length - 1; ++j)
            {
                Vector2 dir1 = prevChain[j + 1] - prevChain[j];
                Vector2 dir2 = currentChain[j + 1] - currentChain[j];
                Vector2 dir3 = nextChain[j + 1] - nextChain[j];

                float maxAngle = Vector2.Angle(dir1, dir3);
                float angle = Vector2.Angle(dir1, dir2);

                Assert.That(angle, Is.GreaterThanOrEqualTo(0f).Using(floatComparer));
                Assert.That(angle, Is.LessThanOrEqualTo(maxAngle).Using(floatComparer));
            }
        }
    }
}
