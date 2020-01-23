namespace UnityEngine.Animations.Rigging
{
    public interface IRigLayer
    {
        Rig rig { get; }

        IRigConstraint[] constraints { get; }
        IAnimationJob[] jobs { get; }

        bool active { get; }
        string name { get; }

        bool Initialize(Animator animator);
        void Update();
        void Reset();

        bool IsValid();
    }
}
