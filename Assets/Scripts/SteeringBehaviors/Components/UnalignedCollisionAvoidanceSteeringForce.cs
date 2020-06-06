using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    internal struct UnalignedCollisionAvoidanceSteeringForce : IComponentData
    {
        public float2 Value;
    }
}
