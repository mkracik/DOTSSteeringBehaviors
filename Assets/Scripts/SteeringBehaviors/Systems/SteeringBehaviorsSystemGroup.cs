using Unity.Entities;

namespace SteeringBehaviors
{
    [DisableAutoCreation]
    [UnityEngine.ExecuteAlways]
    internal class BeginSteeringBehaviorsEntityCommandBufferSystem : EntityCommandBufferSystem {}

    [DisableAutoCreation]
    [UnityEngine.ExecuteAlways]
    internal class EndSteeringBehaviorsEntityCommandBufferSystem : EntityCommandBufferSystem {}

    public class SteeringBehaviorsSystemGroup : ComponentSystemGroup
    {
        private BeginSteeringBehaviorsEntityCommandBufferSystem m_BeginEntityCommandBufferSystem;
        private EndSteeringBehaviorsEntityCommandBufferSystem m_EndEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_BeginEntityCommandBufferSystem = World.GetOrCreateSystem<BeginSteeringBehaviorsEntityCommandBufferSystem>();
            m_EndEntityCommandBufferSystem = World.GetOrCreateSystem<EndSteeringBehaviorsEntityCommandBufferSystem>();
            AddSystemToUpdateList(m_BeginEntityCommandBufferSystem);
            AddSystemToUpdateList(m_EndEntityCommandBufferSystem);
        }
    }
}
