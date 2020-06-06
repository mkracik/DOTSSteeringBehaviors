using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateAfter(typeof(ComponentAddSystem))]
    [UpdateBefore(typeof(SteeringForceCombinerSystem))]
    internal class LeashSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
            .WithName("LeashUpdateJob")
            .ForEach((ref LeashSteeringForce leashSteeringForce, in SBPosition2D position, in Leash leash) =>
            {
                if (!leash.Enabled)
                {
                    leashSteeringForce.Value = float2.zero;
                    return;
                }

                // return to origin if too far
                float2 vectorToOrigin = leash.OriginPosition - position.Value;
                float distanceSquared = math.lengthsq(vectorToOrigin);
                if (distanceSquared > (leash.MaximumDistance * leash.MaximumDistance))
                {
                    leashSteeringForce.Value = vectorToOrigin;
                }
                else
                {
                    leashSteeringForce.Value = float2.zero;
                }
            }).ScheduleParallel();
        }
    }
}
