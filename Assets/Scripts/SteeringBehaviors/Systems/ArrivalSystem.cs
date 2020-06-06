using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    // from https://www.red3d.com/cwr/steer/gdc99/
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateAfter(typeof(ComponentAddSystem))]
    [UpdateBefore(typeof(SteeringForceCombinerSystem))]
    internal class ArrivalSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Utilities.GetDeltaTime();

            Entities
            .WithName("ArrivalUpdateJob")
            .ForEach((ref ArrivalSteeringForce arrivalSteeringForce, in SBPosition2D position, in SimpleVehicle simpleVehicle,
                in SBVelocity2D velocity, in Arrival arrival) =>
            {
                if (!arrival.Enabled)
                {
                    arrivalSteeringForce.Value = float2.zero;
                    return;
                }

                float2 target_offset = arrival.TargetPosition.Value - position.Value;

                // for testing
                // always overshoots target even without any adjustments
                //arrivalSteeringForce.Value = target_offset - velocity.Value;
                //return;

                float distanceToCenter = math.length(target_offset);
                
                // modification to stop near target, not at it
                // distance to a point on a sphere ONLY if difference is positive

                if (distanceToCenter < arrival.StopAtDistance)
                {
                    // means overshoot, back up
                    // can happen with repair ship repairing its own service post
                }

                float distanceToSphere = distanceToCenter - arrival.StopAtDistance;

                // 0 leads to division by 0 and NaN
                if (math.abs(distanceToSphere) < 0.01f)
                {
                    // even with this code still oscillates back and forth
                    arrivalSteeringForce.Value = - velocity.Value * deltaTime;
                    return;
                }

                target_offset = math.normalize(target_offset);
                target_offset *= distanceToSphere;
                //arrivalSteeringForce.Value = target_offset - velocity.Value;

                float slowingDistance = arrival.SlowingDistance;

                if (slowingDistance == 0.0f)
                {
                    arrivalSteeringForce.Value = - velocity.Value * deltaTime;
                    return;
                }

                float ramped_speed = simpleVehicle.MaxSpeed * (distanceToSphere / slowingDistance);
                float clipped_speed = math.min(ramped_speed, simpleVehicle.MaxSpeed);
                float2 desired_velocity = (clipped_speed / distanceToSphere) * target_offset;
                // !!! incorrect, must use only forward velocity, not lateral!
                arrivalSteeringForce.Value = desired_velocity - velocity.Value;
            }).ScheduleParallel();
        }
    }
}
