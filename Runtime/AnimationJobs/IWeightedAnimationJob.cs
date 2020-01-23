namespace UnityEngine.Animations.Rigging
{
    public interface IWeightedAnimationJob : IAnimationJob
    {
        FloatProperty jobWeight { get; set; }
    }
}
