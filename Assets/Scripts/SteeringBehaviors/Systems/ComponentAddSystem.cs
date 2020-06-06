using Unity.Collections;
using Unity.Entities;

namespace SteeringBehaviors
{
    // from https://forum.unity.com/threads/whats-the-replacement-of-postupdatecommands-in-systembase.846997/
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    internal class ComponentAddSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            Entities
            .WithAll<SimpleVehicle>()
            .WithNone<CombinedSteeringForce>()
            .ForEach((Entity entity) =>
            {
                ecb.AddComponent(entity, new ArrivalSteeringForce());
                ecb.AddComponent(entity, new WanderSteeringForce());
                ecb.AddComponent(entity, new LeashSteeringForce());
                ecb.AddComponent(entity, new UnalignedCollisionAvoidanceSteeringForce());
                ecb.AddComponent(entity, new CombinedSteeringForce());
            }).Run();
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
