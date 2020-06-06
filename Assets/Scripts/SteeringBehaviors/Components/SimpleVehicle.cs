using Unity.Entities;

namespace SteeringBehaviors
{
    // from OpenSteer SimpleVehicle.h, LocalSpace.h
    [GenerateAuthoringComponent]
    public struct SimpleVehicle : IComponentData
    {
        public float Mass;       // mass

        public float MaxForce;   // the maximum steering force this vehicle can apply
                                 // (steering force is clipped to this magnitude)

        public float MaxSpeed;   // the maximum speed this vehicle is allowed to move
                                 // (velocity is clipped to this magnitude)
    }
}
