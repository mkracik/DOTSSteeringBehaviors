using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    internal struct WanderSteeringForce : IComponentData
    {
        public float2 Value;
    }
}
