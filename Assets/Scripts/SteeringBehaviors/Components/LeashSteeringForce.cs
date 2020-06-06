using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    internal struct LeashSteeringForce : IComponentData
    {
        public float2 Value;
    }
}
