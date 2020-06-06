using Unity.Entities;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct Wander : IComponentData
    {
        public float WanderAngle;
        public float Timer;
        public bool Enabled;
    }
}
