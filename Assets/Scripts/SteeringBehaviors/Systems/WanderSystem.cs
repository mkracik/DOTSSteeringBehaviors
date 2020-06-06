using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    // from https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-wander--gamedev-1624
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateAfter(typeof(ComponentAddSystem))]
    [UpdateBefore(typeof(SteeringForceCombinerSystem))]
    internal class WanderSystem : SystemBase
    {
        private NativeArray<Random> randomNumberGenerators;

        protected override void OnCreate()
        {
            base.OnCreate();

            // has to be created in OnCreate and not in OnStartRunning, otherwise there is error:
            // "A Native Collection has not been disposed, resulting in a memory leak."

            uint seed = (uint)System.Environment.TickCount;
            randomNumberGenerators = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
            for (int i = 0; i < randomNumberGenerators.Length; i++)
            {
                randomNumberGenerators[i] = new Random(seed);
            }
        }

        protected override void OnDestroy()
        {
            randomNumberGenerators.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            const float CIRCLE_DISTANCE = 500f;
            const float CIRCLE_RADIUS = 300f;
            const float ANGLE_CHANGE = 10f;

            float deltaTime = Utilities.GetDeltaTime();

            NativeArray<Random> localRandomNumberGenerators = randomNumberGenerators;
            
            Entities
            .WithAll<SimpleVehicle>()
            .WithNativeDisableParallelForRestriction(localRandomNumberGenerators)
            .WithName("WanderUpdateJob")
            .ForEach((int nativeThreadIndex, ref Wander wander, ref WanderSteeringForce wanderSteeringForce, in SBPosition2D position, in SBVelocity2D velocity) =>
            {
                if (!wander.Enabled)
                {
                    wanderSteeringForce.Value = float2.zero;
                    return;
                }

                wander.Timer += deltaTime;

                // change direction only each x sec
                if (wander.Timer <= 4.0f)
                {
                    // keep the same wander steering force
                    return;
                }

                wander.Timer = 0.0f;

                // Calculate the circle center
                float2 circleCenter = velocity.Value;
                circleCenter = math.normalize(circleCenter);
                circleCenter = circleCenter * CIRCLE_DISTANCE;

                // Calculate the displacement force
                float2 displacement = new float2(0, -1);
                displacement = displacement * CIRCLE_RADIUS;

               // Randomly change the vector direction
               // by making it change its current angle
               float len = math.length(displacement);
               displacement.x = math.cos(wander.WanderAngle) * len;
               displacement.y = math.sin(wander.WanderAngle) * len;

               // Change wanderAngle just a bit, so it
               // won't have the same value in the
               // next game frame.

               Random randomNumberGenerator = localRandomNumberGenerators[nativeThreadIndex];
               wander.WanderAngle += randomNumberGenerator.NextFloat() * ANGLE_CHANGE - (ANGLE_CHANGE * 0.5f);
               localRandomNumberGenerators[nativeThreadIndex] = randomNumberGenerator;

               // Finally calculate and return the wander force
               float2 wanderForce = circleCenter + displacement;
               wanderSteeringForce.Value = wanderForce;
            }).ScheduleParallel();
        }
    }
}
