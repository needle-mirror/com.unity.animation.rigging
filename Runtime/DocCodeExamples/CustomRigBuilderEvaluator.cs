using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace DocCodeExamples
{
    /// <summary>
    /// Custom Evaluator that manually evaluates the RigBuilder in LateUpdate.
    /// </summary>
    #region custom-rig-builder-evaluator
    [RequireComponent(typeof(RigBuilder))]
    public class CustomRigBuilderEvaluator : MonoBehaviour
    {
        private RigBuilder m_RigBuilder;

        void OnEnable()
        {
            m_RigBuilder = GetComponent<RigBuilder>();

            // Disable the RigBuilder and set its PlayableGraph to manual update mode
            // to let the script evaluate it instead.
            m_RigBuilder.enabled = false;
            if (m_RigBuilder.Build())
            {
                m_RigBuilder.graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            }
        }

        void LateUpdate()
        {
            m_RigBuilder.Evaluate(Time.deltaTime);
        }
    }
    #endregion
}
