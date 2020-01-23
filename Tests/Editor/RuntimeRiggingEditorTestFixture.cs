//#define DEBUG_BAKE_TO_CONSTRAINT
//#define DEBUG_BAKE_TO_SKELETON

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.Playables;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Animations.Rigging;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.IO;
using System;

public class RuntimeRiggingEditorTestFixture
{
    readonly static float k_RotationEpsilon = 2.0f;
    readonly static float k_Epsilon = 0.05f;

    readonly static CurveFilterOptions k_DefaultCurveFilterOptions = new CurveFilterOptions()
    {
        unrollRotation = true,
        keyframeReduction = false,
        positionError = 0.5f,
        rotationError = 0.5f,
        scaleError = 0.5f,
        floatError = 0.5f
    };

    [Flags]
    public enum CompareFlags
    {
        None            = 0,
        Translation     = 1 << 0,
        Rotation        = 1 << 1,

        TR              = Translation | Rotation
    };

    [Flags]
    public enum AxesMask
    {
        None            = 0,
        X               = 1 << 0,
        Y               = 1 << 1,
        Z               = 1 << 2,

        XY              = X | Y,
        YZ              = Y | Z,
        XZ              = X | Z,
        XYZ             = X | Y | Z
    };

    public static bool TestXAxis(AxesMask mask) => (mask & AxesMask.X) != 0;
    public static bool TestYAxis(AxesMask mask) => (mask & AxesMask.Y) != 0;
    public static bool TestZAxis(AxesMask mask) => (mask & AxesMask.Z) != 0;

    private class EvaluationGraph : IDisposable
    {
        SyncSceneToStreamLayer m_SyncSceneToStreamLayer;
        List<IRigLayer> m_RigLayers;
        PlayableGraph m_Graph;
        AnimationPlayableOutput m_Output;

        public EvaluationGraph(Animator animator, AnimationClip clip)
        {
            m_Graph = PlayableGraph.Create("Evaluation-Graph");
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var clipPlayable = AnimationClipPlayable.Create(m_Graph, clip);

            m_Output = AnimationPlayableOutput.Create(m_Graph, "Evaluation-Graph-Output", animator);
            m_Output.SetSourcePlayable(clipPlayable);
        }

        public EvaluationGraph(RigBuilder rigBuilder, AnimationClip clip)
        {
            m_SyncSceneToStreamLayer = new SyncSceneToStreamLayer();

            var layers = rigBuilder.layers;
            m_RigLayers = new List<IRigLayer>(layers.Count);
            for (int i = 0; i < layers.Count; ++i)
            {
                if (!layers[i].active)
                    continue;

                m_RigLayers.Add(new RigLayer(layers[i].rig));
            }

            m_Graph = PlayableGraph.Create("Evaluation-Graph");
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var animator = rigBuilder.GetComponent<Animator>();

            var clipPlayable = AnimationClipPlayable.Create(m_Graph, clip);
            Playable inputPlayable = clipPlayable;

            var playableChains = RigBuilderUtils.BuildPlayables(animator, m_Graph, m_RigLayers, m_SyncSceneToStreamLayer);
            foreach (var chain in playableChains)
            {
                if (!chain.IsValid())
                    continue;

                chain.playables[0].AddInput(inputPlayable, 0, 1);
                inputPlayable = chain.playables[chain.playables.Length - 1];
            }


            m_Output = AnimationPlayableOutput.Create(m_Graph, "Evaluation-Graph-Output", animator);
            m_Output.SetSourcePlayable(inputPlayable);
        }

        public void Evaluate(float dt)
        {
            m_SyncSceneToStreamLayer?.Update(m_RigLayers);

            if (m_RigLayers != null)
            {
                foreach (var layer in m_RigLayers)
                {
                    if (layer.IsValid() && layer.active)
                        layer.Update();
                }
            }

            m_Graph.Evaluate(dt);
        }

        public void Dispose()
        {
            m_Output.SetTarget(null);
            m_Graph.Destroy();

            if (m_RigLayers != null)
            {
                for (int i = 0; i < m_RigLayers.Count; ++i)
                {
                    m_RigLayers[i].Reset();
                }
                m_RigLayers.Clear();
            }

            m_SyncSceneToStreamLayer?.Reset();
        }
    }

    public static AnimationClip CreateDefaultPose(GameObject rootGO)
    {
        var defaultPoseClip = new AnimationClip() { name = "DefaultPose" };

        var bindings = new List<EditorCurveBinding>();

        var gameObjects = new Queue<GameObject>();
        gameObjects.Enqueue(rootGO);

        while (gameObjects.Count > 0)
        {
            var gameObject = gameObjects.Dequeue();
            var path = AnimationUtility.CalculateTransformPath(gameObject.transform, rootGO.transform);

            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.x"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.y"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalPosition.z"));

            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.x"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.y"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.z"));
            bindings.Add(EditorCurveBinding.FloatCurve(path, typeof(Transform), "m_LocalRotation.w"));

            // Iterate over all child GOs
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform childTransform = gameObject.transform.GetChild(i);
                gameObjects.Enqueue(childTransform.gameObject);
            }
        }

        foreach(var binding in bindings)
        {
            float floatValue;
            AnimationUtility.GetFloatValue(rootGO, binding, out floatValue);

            var key = new Keyframe(0f, floatValue);
            var curve = new AnimationCurve(new Keyframe[] {key});
            defaultPoseClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
        }

        return defaultPoseClip;
    }

    public static void RestoreDefaultPose(Animator animator, AnimationClip defaultPoseClip)
    {
        using (var graph = new EvaluationGraph(animator, defaultPoseClip))
        {
            graph.Evaluate(0f);
        }
    }

    public static void TestTransferMotionToSkeleton<T>(T constraint, RigBuilder rigBuilder, AnimationClip clip, IList<Transform> targetTransforms, CompareFlags flags)
        where T : MonoBehaviour, IRigConstraint
    {
        Animator animator = rigBuilder.GetComponent<Animator>();

        int numberOfFrames = 60;
		float dt = 1f / numberOfFrames;

        var defaultPoseClip = CreateDefaultPose(rigBuilder.gameObject);

#if DEBUG_BAKE_TO_SKELETON
        AssetDatabase.CreateAsset(clip, "Assets/bakeToSkeletonBefore.anim");
        clip = UnityEngine.Object.Instantiate(clip) as AnimationClip;
#endif

        // Evaluate clip with constraint active and take snapshots.
        var translationsBeforeBaking = new Vector3[targetTransforms.Count, numberOfFrames + 1];
        var rotationsBeforeBaking = new Quaternion[targetTransforms.Count, numberOfFrames + 1];

        using (var graph = new EvaluationGraph(rigBuilder, clip))
        {
            graph.Evaluate(0f);
            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                translationsBeforeBaking[transformIndex, 0] = targetTransforms[transformIndex].position;
                rotationsBeforeBaking[transformIndex, 0] = targetTransforms[transformIndex].rotation;
            }

            for (int frame = 1; frame <= numberOfFrames; ++frame)
            {
                graph.Evaluate(dt);
                for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
                {
                    translationsBeforeBaking[transformIndex, frame] = targetTransforms[transformIndex].position;
                    rotationsBeforeBaking[transformIndex, frame] = targetTransforms[transformIndex].rotation;
                }
            }
        }

        RestoreDefaultPose(animator, defaultPoseClip);

        // Bake constraint to skeleton.
        var bakeParameters = BakeUtils.FindBakeParameters(constraint);
        Assert.That(bakeParameters, Is.Not.Null);

        var bindings = bakeParameters.GetConstrainedCurveBindings(rigBuilder, constraint);

        AnimationMode.StartAnimationMode();
        BakeUtils.BakeToSkeleton(constraint, clip, defaultPoseClip, bindings, k_DefaultCurveFilterOptions);
        AnimationMode.StopAnimationMode();

#if DEBUG_BAKE_TO_SKELETON
        AssetDatabase.CreateAsset(clip, "Assets/bakeToSkeletonAfter.anim");
#endif

        RestoreDefaultPose(animator, defaultPoseClip);

        // Evaluate again without constraint and take snapshots.
        var translationsAfterBaking = new Vector3[targetTransforms.Count, numberOfFrames + 1];
        var rotationsAfterBaking = new Quaternion[targetTransforms.Count, numberOfFrames + 1];

        using (var graph = new EvaluationGraph(rigBuilder.GetComponent<Animator>(), clip))
        {
            graph.Evaluate(0f);
            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                translationsAfterBaking[transformIndex, 0] = targetTransforms[transformIndex].position;
                rotationsAfterBaking[transformIndex, 0] = targetTransforms[transformIndex].rotation;
            }

            for (int frame = 1; frame <= numberOfFrames; ++frame)
            {
                graph.Evaluate(dt);
                for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
                {
                    translationsAfterBaking[transformIndex, frame] = targetTransforms[transformIndex].position;
                    rotationsAfterBaking[transformIndex, frame] = targetTransforms[transformIndex].rotation;
                }
            }
        }

        RestoreDefaultPose(animator, defaultPoseClip);

        if ((flags & CompareFlags.Rotation) != 0)
        {
            // Compare rotations
            var quaternionComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_RotationEpsilon);

            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                for (int frame = 0; frame <= numberOfFrames; ++frame)
                {
                    Assert.That(rotationsAfterBaking[transformIndex, frame], Is.EqualTo(rotationsBeforeBaking[transformIndex, frame]).Using(quaternionComparer),
                            String.Format("Transform '{0}' rotation is set to {1} at frame {2}, but was expected to be {3}",
                                targetTransforms[transformIndex].name,rotationsAfterBaking[transformIndex, frame].eulerAngles, frame, rotationsBeforeBaking[transformIndex, frame].eulerAngles));
                }
            }
        }

        if ((flags & CompareFlags.Translation) != 0)
        {
            // Compare translations
            var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                for (int frame = 0; frame <= numberOfFrames; ++frame)
                {
                    Assert.That(translationsAfterBaking[transformIndex, frame], Is.EqualTo(translationsBeforeBaking[transformIndex, frame]).Using(positionComparer),
                            String.Format("Transform '{0}' position is set to {1} at frame {2}, but was expected to be {3}",
                                targetTransforms[transformIndex].name,translationsAfterBaking[transformIndex, frame], frame, translationsBeforeBaking[transformIndex, frame]));

                }
            }
        }
    }

    public static void TestTransferMotionToConstraint<T>(T constraint, RigBuilder rigBuilder, AnimationClip clip, IList<Transform> targetTransforms, CompareFlags flags)
        where T : MonoBehaviour, IRigConstraint
    {
        Animator animator = rigBuilder.GetComponent<Animator>();

        int numberOfFrames = 60;
        float dt = 1f / numberOfFrames;

        var defaultPoseClip = CreateDefaultPose(rigBuilder.gameObject);

#if DEBUG_BAKE_TO_CONSTRAINT
        AssetDatabase.CreateAsset(clip, "Assets/bakeToConstraintBefore.anim");
        clip = UnityEngine.Object.Instantiate(clip) as AnimationClip;
#endif

        // Evaluate clip without constraint and take snapshots.
        var translationsBeforeBaking = new Vector3[targetTransforms.Count, numberOfFrames + 1];
        var rotationsBeforeBaking = new Quaternion[targetTransforms.Count, numberOfFrames + 1];

        using (var graph = new EvaluationGraph(rigBuilder.GetComponent<Animator>(), clip))
        {
            graph.Evaluate(0f);
            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                translationsBeforeBaking[transformIndex, 0] = targetTransforms[transformIndex].position;
                rotationsBeforeBaking[transformIndex, 0] = targetTransforms[transformIndex].rotation;
            }

            for (int frame = 1; frame <= numberOfFrames; ++frame)
            {
                graph.Evaluate(dt);
                for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
                {
                    translationsBeforeBaking[transformIndex, frame] = targetTransforms[transformIndex].position;
                    rotationsBeforeBaking[transformIndex, frame] = targetTransforms[transformIndex].rotation;
                }
            }
        }

        RestoreDefaultPose(animator, defaultPoseClip);

        // Bake and inverse solve to constraint.
        var bakeParameters = BakeUtils.FindBakeParameters(constraint);
        Assert.That(bakeParameters, Is.Not.Null);

        var bindings = bakeParameters.GetSourceCurveBindings(rigBuilder, constraint);

        AnimationMode.StartAnimationMode();
        BakeUtils.BakeToConstraint(constraint, clip, defaultPoseClip, bindings, k_DefaultCurveFilterOptions);
        AnimationMode.StopAnimationMode();

#if DEBUG_BAKE_TO_CONSTRAINT
        AssetDatabase.CreateAsset(clip, "Assets/bakeToConstraintAfter.anim");
#endif

        RestoreDefaultPose(animator, defaultPoseClip);

        // Evaluate again with constraint active and take snapshots.
        var translationsAfterBaking = new Vector3[targetTransforms.Count, numberOfFrames + 1];
        var rotationsAfterBaking = new Quaternion[targetTransforms.Count, numberOfFrames + 1];

        using (var graph = new EvaluationGraph(rigBuilder, clip))
        {
            graph.Evaluate(0f);
            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                translationsAfterBaking[transformIndex, 0] = targetTransforms[transformIndex].position;
                rotationsAfterBaking[transformIndex, 0] = targetTransforms[transformIndex].rotation;
            }

            for (int frame = 1; frame <= numberOfFrames; ++frame)
            {
                graph.Evaluate(dt);
                for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
                {
                    translationsAfterBaking[transformIndex, frame] = targetTransforms[transformIndex].position;
                    rotationsAfterBaking[transformIndex, frame] = targetTransforms[transformIndex].rotation;
                }
            }
        }

        RestoreDefaultPose(animator, defaultPoseClip);

        if ((flags & CompareFlags.Rotation) != 0)
        {
            // Compare rotations
            var quaternionComparer = new RuntimeRiggingTestFixture.QuaternionEqualityComparer(k_RotationEpsilon);

            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                for (int frame = 0; frame <= numberOfFrames; ++frame)
                {
                    Assert.That(rotationsAfterBaking[transformIndex, frame], Is.EqualTo(rotationsBeforeBaking[transformIndex, frame]).Using(quaternionComparer),
                            String.Format("Transform '{0}' rotation is set to {1} at frame {2}, but was expected to be {3}",
                                targetTransforms[transformIndex].name,rotationsAfterBaking[transformIndex, frame].eulerAngles, frame, rotationsBeforeBaking[transformIndex, frame].eulerAngles));
                }
            }
        }

        if ((flags & CompareFlags.Translation) != 0)
        {
            // Compare translations
            var positionComparer = new RuntimeRiggingTestFixture.Vector3EqualityComparer(k_Epsilon);

            for (int transformIndex = 0; transformIndex < targetTransforms.Count; ++transformIndex)
            {
                for (int frame = 0; frame <= numberOfFrames; ++frame)
                {
                    Assert.That(translationsAfterBaking[transformIndex, frame], Is.EqualTo(translationsBeforeBaking[transformIndex, frame]).Using(positionComparer),
                            String.Format("Transform '{0}' position is set to {1} at frame {2}, but was expected to be {3}",
                                targetTransforms[transformIndex].name,translationsAfterBaking[transformIndex, frame], frame, translationsBeforeBaking[transformIndex, frame]));

                }
            }
        }
    }

}

