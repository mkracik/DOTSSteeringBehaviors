using Unity.Entities;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateAfter(typeof(PhysicsUpdateSystem))]
    [UpdateBefore(typeof(EndSteeringBehaviorsEntityCommandBufferSystem))]
    internal class RotationUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
            .WithName("RotationUpdateJob")
            .ForEach((ref SBRotation2D rotation, in SBVelocity2D velocity) =>
            {
                if (math.lengthsq(velocity.Value) <= 0.0f)
                {
                    return;
                }

                float2 new_forward = velocity.Value;
                rotation.HeadingAngle = math.atan2(new_forward.x, new_forward.y);
            }).ScheduleParallel();
        }
    }
}
