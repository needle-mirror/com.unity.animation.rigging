namespace UnityEngine.Animations.Rigging
{
    public class OverrideRigConstraint<TConstraint, TJob, TData, TBinder> : IRigConstraint
        where TConstraint : MonoBehaviour, IRigConstraint
        where TJob        : struct, IWeightedAnimationJob
        where TData       : struct, IAnimationJobData
        where TBinder     : AnimationJobBinder<TJob, TData>, new()
    {
        [SerializeField]
        protected TConstraint m_Constraint;

        static readonly TBinder s_Binder = new TBinder();

        public OverrideRigConstraint(TConstraint baseConstraint)
        {
            m_Constraint = baseConstraint;
        }

        public IAnimationJob CreateJob(Animator animator)
        {
            IAnimationJobBinder binder = (IAnimationJobBinder)s_Binder;
            TJob job = (TJob)binder.Create(animator, m_Constraint.data, m_Constraint);

            // Bind constraint job weight property
            job.jobWeight = FloatProperty.BindCustom(
                animator,
                PropertyUtils.ConstructCustomPropertyName(m_Constraint, ConstraintProperties.s_Weight)
                );

            return job;
        }

        public void DestroyJob(IAnimationJob job) => s_Binder.Destroy((TJob)job);

        public void UpdateJob(IAnimationJob job)
        {
            IAnimationJobBinder binder = (IAnimationJobBinder)s_Binder;
            binder.Update(job, m_Constraint.data);
        }

        public bool IsValid()
        {
            return m_Constraint.IsValid();
        }

        IAnimationJobBinder IRigConstraint.binder => s_Binder;

        IAnimationJobData IRigConstraint.data => m_Constraint.data;

        Component IRigConstraint.component => m_Constraint.component;

        public float weight { get => m_Constraint.weight; set => m_Constraint.weight = value; }
    }
}
