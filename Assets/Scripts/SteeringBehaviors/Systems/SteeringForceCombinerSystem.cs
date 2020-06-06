using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateBefore(typeof(EndSteeringBehaviorsEntityCommandBufferSystem))]
    internal class SteeringForceCombinerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
            .WithAll<SimpleVehicle>()
            .WithName("SteeringForceCombinerJob")
            .ForEach((
                ref CombinedSteeringForce combinedSteeringForce,
                in ArrivalSteeringForce arrivalSteeringForce,
                in WanderSteeringForce wanderSteeringForce,
                in LeashSteeringForce leashSteeringForce,
                in UnalignedCollisionAvoidanceSteeringForce unalignedCollisionAvoidanceSteeringForce) =>
            {
                CombinedSteeringForce result = new CombinedSteeringForce(); // set all to 0

                if (!leashSteeringForce.Value.Equals(float2.zero))
                {
                    result.OverridingDirectForce = leashSteeringForce.Value;
                }

                if (!unalignedCollisionAvoidanceSteeringForce.Value.Equals(float2.zero))
                {
                    // Avoidance lateral has priority over Wander lateral
                    // Avoidance lateral (normalized) must be combined with Arrival forward or back (not normalized, clamped to max speed)
                    // without combination lateral arrival was overriding braking force with zero causing overshoot of target
                    result.LateralForce = unalignedCollisionAvoidanceSteeringForce.Value;
                    if (!arrivalSteeringForce.Value.Equals(float2.zero))
                    {
                        result.ForwardForce = arrivalSteeringForce.Value;
                    }
                }
                else if (!arrivalSteeringForce.Value.Equals(float2.zero))
                {
                    result.ForwardForce = arrivalSteeringForce.Value;
                }
                else if (!wanderSteeringForce.Value.Equals(float2.zero))
                {
                    // only Wander lateral without Arrival
                    // Wander is mutually exclusive with FollowPath and Arrival
                    result.LateralForce = wanderSteeringForce.Value;
                }

                combinedSteeringForce = result;
            }).ScheduleParallel();
        }
    }
}
