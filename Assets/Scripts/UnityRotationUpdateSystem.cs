using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using SteeringBehaviors;

[UpdateAfter(typeof(SteeringBehaviorsSystemGroup))]
public class UnityRotationUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
        .WithName("SetUnityRotationJob")
        .ForEach((ref Rotation rotation, in SBRotation2D rotation2D) =>
        {
            rotation.Value = quaternion.AxisAngle(math.up(), rotation2D.HeadingAngle);
        }).ScheduleParallel();
    }
}
