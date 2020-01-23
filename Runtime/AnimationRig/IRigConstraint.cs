namespace UnityEngine.Animations.Rigging
{
    public interface IRigConstraint
    {
        bool IsValid();

        IAnimationJob CreateJob(Animator animator);
        void UpdateJob(IAnimationJob job);
        void DestroyJob(IAnimationJob job);

        IAnimationJobData data { get; }
        IAnimationJobBinder binder { get; }

        Component component { get; }

        float weight { get; set; }
    }
}

