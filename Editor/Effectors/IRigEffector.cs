
namespace UnityEngine.Animations.Rigging
{
    public interface IRigEffector
    {
        Transform transform { get; }
        bool visible { get; set; }

        void OnSceneGUI();
    }
}
