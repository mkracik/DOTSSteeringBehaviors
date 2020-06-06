using Unity.Entities;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct SBAvoidanceTarget : IComponentData
    {
        public float Radius;     // size of bounding sphere, for obstacle avoidance, etc.
        public SBAvoidanceBitMask BitMask;
    }
}
