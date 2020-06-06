using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    internal struct CombinedSteeringForce : IComponentData
    {
        public float2 ForwardForce;
        public float2 LateralForce;
        public float2 OverridingDirectForce;
    }
}
