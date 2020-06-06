using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    // from https://www.red3d.com/cwr/steer/gdc99/
    // uses forward Euler integration
    // Updates only internal Velocity and Position, not rotation.
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateAfter(typeof(SteeringForceCombinerSystem))]
    [UpdateBefore(typeof(EndSteeringBehaviorsEntityCommandBufferSystem))]
    internal class PhysicsUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Utilities.GetDeltaTime();

            Entities
            .WithName("PhysicsUpdateJob")
            .ForEach((ref SBPosition2D position, ref SBVelocity2D velocity,
                in CombinedSteeringForce combinedSteeringForce, in SimpleVehicle simpleVehicle) =>
            {
                if (!combinedSteeringForce.OverridingDirectForce.Equals(float2.zero))
                {
                    float2 steering_force = combinedSteeringForce.OverridingDirectForce;
                    float2 acceleration = steering_force;
                    float2 v = velocity.Value + (acceleration * deltaTime);
                    velocity.Value = Utilities.TruncateLength(v, simpleVehicle.MaxSpeed);
                    position.Value = position.Value + (velocity.Value * deltaTime);
                }
                else
                {
                    float2 forwardAcceleration = combinedSteeringForce.ForwardForce;
                    float2 lateralAcceleration = combinedSteeringForce.LateralForce;
                    float2 v = velocity.Value + (forwardAcceleration * deltaTime) + (lateralAcceleration * deltaTime);
                    velocity.Value = Utilities.TruncateLength(v, simpleVehicle.MaxSpeed);
                    position.Value = position.Value + (velocity.Value * deltaTime);
                }
            }).ScheduleParallel();
        }
    }
}
