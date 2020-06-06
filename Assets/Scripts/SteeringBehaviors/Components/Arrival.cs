using Unity.Entities;

namespace SteeringBehaviors
{
    [GenerateAuthoringComponent]
    public struct Arrival : IComponentData
    {
        public bool Enabled;
        public SBPosition2D TargetPosition;
        public float StopAtDistance; // non-standard
        public float SlowingDistance;
    }
}
