using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct Leash : IComponentData
    {
        public bool Enabled;
        public float2 OriginPosition;
        public float MaximumDistance;
    }
}
