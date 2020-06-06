using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using SteeringBehaviors;

[UpdateAfter(typeof(SteeringBehaviorsSystemGroup))]
public class UnityTranslationUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
        .WithName("UnityTranslationUpdateJob")
        .ForEach((ref Translation translation, in SBPosition2D position) =>
        {
            translation.Value = new float3(position.Value.x, translation.Value.y, position.Value.y);
        }).ScheduleParallel();
    }
}
