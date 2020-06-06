using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    internal struct ArrivalSteeringForce : IComponentData
    {
        public float2 Value;
    }
}
