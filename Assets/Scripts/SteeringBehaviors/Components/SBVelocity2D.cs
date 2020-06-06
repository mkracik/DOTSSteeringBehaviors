using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct SBVelocity2D : IComponentData
    {
        public float2 Value;
    }
}
