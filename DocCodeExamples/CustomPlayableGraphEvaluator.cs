using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace DocCodeExamples
{
    /// <summary>
    /// Custom evaluator that manually evaluates the PlayableGraph in LateUpdate.
    /// </summary>
    #region custom-playable-graph-evaluator
    [RequireComponent(typeof(RigBuilder))]
    public class CustomPlayableGraphEvaluator : MonoBehaviour
    {
        private RigBuilder m_RigBuilder;
        private PlayableGraph m_PlayableGraph;

        void OnEnable()
        {
            m_RigBuilder = GetComponent<RigBuilder>();
            m_PlayableGraph = PlayableGraph.Create();
            m_PlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            m_RigBuilder.Build(m_PlayableGraph);
        }

        void OnDisable()
        {
            if (m_PlayableGraph.IsValid())
                m_PlayableGraph.Destroy();
        }

        void LateUpdate()
        {
            m_RigBuilder.SyncLayers();
            m_PlayableGraph.Evaluate(Time.deltaTime);
        }
    }
    #endregion
}
