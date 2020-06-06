using Unity.Entities;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct SBAvoidanceSource : IComponentData
    {
        public float VisibilityRange;
        public SBAvoidanceBitMask BitMask;
    }
}
