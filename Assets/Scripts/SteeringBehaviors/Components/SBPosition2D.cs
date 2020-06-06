using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct SBPosition2D : IComponentData
    {
        public float2 Value;
    }
}
