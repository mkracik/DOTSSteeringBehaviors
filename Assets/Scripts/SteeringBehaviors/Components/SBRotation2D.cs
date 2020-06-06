using Unity.Entities;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct SBRotation2D : IComponentData
    {
        public float HeadingAngle; // in rad
    }
}
