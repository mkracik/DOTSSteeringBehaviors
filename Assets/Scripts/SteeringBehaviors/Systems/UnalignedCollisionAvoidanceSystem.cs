using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SteeringBehaviors
{
    // from OpenSteer SteerLibrary.h steerToAvoidNeighbors()
    [UpdateInGroup(typeof(SteeringBehaviorsSystemGroup))]
    [UpdateAfter(typeof(BeginSteeringBehaviorsEntityCommandBufferSystem))]
    [UpdateAfter(typeof(ComponentAddSystem))]
    [UpdateBefore(typeof(SteeringForceCombinerSystem))]
    [UpdateBefore(typeof(EndSteeringBehaviorsEntityCommandBufferSystem))]
    internal class UnalignedCollisionAvoidanceSystem : SystemBase
    {
        private EntityQuery avoidanceTargetQuery;
        
        // in loop order and won't alias
        private struct InnerLoopContainer
        {
            public SBAvoidanceTarget sbAvoidanceTarget;
            public SBPosition2D position;
            
            public SBRotation2D rotation;
            public float2 forwardNormalized;
            
            public SBVelocity2D velocity;
            public float speed;
            public Entity entity;
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            avoidanceTargetQuery = EntityManager.CreateEntityQuery(typeof(SBAvoidanceTarget), typeof(SBPosition2D), typeof(SBRotation2D), typeof(SBVelocity2D));
        }

        const float minTimeToCollision = 10; //20; // should be seconds but it's calculated incorrectly; 10 won't avoid asteroids; 15 repair ships oscillate around main station
        const float minimumSpeed = 0.01f;

        // ------------------------------------------------------------------------
        // Unaligned collision avoidance behavior: avoid colliding with other
        // nearby vehicles moving in unconstrained directions.  Determine which
        // (if any) other other vehicle we would collide with first, then steers
        // to avoid the site of that potential collision.  Returns a steering
        // force vector, which is zero length if there is no impending collision.

        protected override void OnUpdate()
        {
            // moved here from inner loop lookups
            NativeArray<InnerLoopContainer> allInnerLoopContainers;
            NativeArray<Entity> allAvoidanceTargetEntities = avoidanceTargetQuery.ToEntityArray(Allocator.TempJob);
            allInnerLoopContainers = new NativeArray<InnerLoopContainer>(allAvoidanceTargetEntities.Length, Allocator.TempJob); 
            ComponentDataFromEntity<SBAvoidanceTarget> allAvoidanceTargets = GetComponentDataFromEntity<SBAvoidanceTarget>(isReadOnly: true);
            ComponentDataFromEntity<SBPosition2D> allPositions = GetComponentDataFromEntity<SBPosition2D>(isReadOnly: true);
            ComponentDataFromEntity<SBRotation2D> allRotations = GetComponentDataFromEntity<SBRotation2D>(isReadOnly: true);
            ComponentDataFromEntity<SBVelocity2D> allVelocities = GetComponentDataFromEntity<SBVelocity2D>(isReadOnly: true);
            
            JobHandle prepareJobHandle = Job
            .WithReadOnly(allAvoidanceTargets)
            .WithReadOnly(allAvoidanceTargetEntities)
            .WithReadOnly(allPositions)
            .WithReadOnly(allRotations)
            .WithReadOnly(allVelocities)
            .WithDeallocateOnJobCompletion(allAvoidanceTargetEntities)
            .WithName("PrepareInnerLoopContainers")
            .WithCode(() =>
            {
                for (int i = 0; i < allInnerLoopContainers.Length; i++)
                {
                    // index must match allAvoidanceTargetEntities
                    Entity entity = allAvoidanceTargetEntities[i];
                    
                    SBRotation2D rotation = allRotations[entity];
                    float2 otherForwardNormalized = new float2();
                    math.sincos(rotation.HeadingAngle, out otherForwardNormalized.x, out otherForwardNormalized.y); // avoid inside inner loop 
                    
                    SBVelocity2D velocity = allVelocities[entity];
                    
                    allInnerLoopContainers[i] = new InnerLoopContainer
                    {
                        entity = entity,
                        sbAvoidanceTarget = allAvoidanceTargets[entity],
                        position = allPositions[entity],

                        rotation = rotation,
                        forwardNormalized = otherForwardNormalized,
                        
                        velocity = velocity,
                        speed = math.length(velocity.Value), // avoid sqrt in inner loop
                    }; 
                }
            }).Schedule(Dependency);

            Dependency = JobHandle.CombineDependencies(Dependency, prepareJobHandle);
            
            JobHandle jobHandle2 = Entities
            .WithBurst(Unity.Burst.FloatMode.Fast, Unity.Burst.FloatPrecision.Low)
            .WithReadOnly(allInnerLoopContainers)
            .WithDeallocateOnJobCompletion(allInnerLoopContainers)
            .WithName("UnalignedCollisionAvoidanceJob")
            .ForEach(
            (Entity entity, ref UnalignedCollisionAvoidanceSteeringForce unalignedCollisionAvoidanceSteeringForce,
                in SBAvoidanceSource sbAvoidanceSource, in SBAvoidanceTarget sbAvoidanceTarget,
                in SimpleVehicle simpleVehicle,
                in SBPosition2D position, in SBRotation2D rotation, in SBVelocity2D velocity
            ) =>
            {
                float2 steeringForce = SteerToAvoidNeighbors(entity, in simpleVehicle, in sbAvoidanceSource,
                    in position, in rotation, in velocity, sbAvoidanceTarget.Radius, allInnerLoopContainers);
                unalignedCollisionAvoidanceSteeringForce = new UnalignedCollisionAvoidanceSteeringForce
                {
                    Value = steeringForce
                };
            }).ScheduleParallel(prepareJobHandle);

            Dependency = JobHandle.CombineDependencies(Dependency, jobHandle2);            
            avoidanceTargetQuery.AddDependency(prepareJobHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 SteerToAvoidNeighbors(Entity entity, in SimpleVehicle simpleVehicle, in SBAvoidanceSource sbAvoidanceSource,
            in SBPosition2D myPosition, in SBRotation2D myRotation, in SBVelocity2D myVelocity, float myRadius, NativeArray<InnerLoopContainer> allInnerLoopContainers)
        {
            float mySpeed = math.length(myVelocity.Value);
        
            SBAvoidanceBitMask myMask = sbAvoidanceSource.BitMask;

            float2 myForwardDirectionNormalized = new float2();
            math.sincos(myRotation.HeadingAngle, out myForwardDirectionNormalized.x, out myForwardDirectionNormalized.y);

            float2 mySideDirectionNormalized = new float2(myForwardDirectionNormalized.y, -myForwardDirectionNormalized.x);

            float myVisibilityRangeSquared = sbAvoidanceSource.VisibilityRange * sbAvoidanceSource.VisibilityRange;

            float steer = 0;
            Entity threat = Entity.Null;
            SBPosition2D threatPosition = new SBPosition2D();
            SBRotation2D threatRotation = new SBRotation2D();
            SBVelocity2D threatVelocity = new SBVelocity2D();
            float threatSpeed = 0;

            // Time (in seconds) until the most immediate collision threat found
            // so far.  Initial value is a threshold: don't look more than this
            // many frames into the future.
            float minTime = minTimeToCollision;

            // xxx solely for annotation
            float2 xxxThreatPositionAtNearestApproach = float2.zero;
            //float2 xxxOurPositionAtNearestApproach;

            // for each of the other vehicles, determine which (if any)
            // pose the most immediate threat of collision.
            for (int i = 0; i < allInnerLoopContainers.Length; i++)
            {
                InnerLoopContainer innerLoopContainer = allInnerLoopContainers[i]; // indices match; struct copy
                Entity otherEntity = innerLoopContainer.entity;
                
                if ((otherEntity != entity) && (otherEntity != Entity.Null))
                {
                    SBAvoidanceTarget otherAvoidanceTarget = innerLoopContainer.sbAvoidanceTarget;

                    if ((myMask & otherAvoidanceTarget.BitMask) != 0)
                    {
                        SBPosition2D otherPosition = innerLoopContainer.position;
                        SBRotation2D otherRotation = innerLoopContainer.rotation;

                        float2 relPositionOfMeRelativeToTarget = myPosition.Value - otherPosition.Value;

                        float distanceSquared = math.lengthsq(relPositionOfMeRelativeToTarget);
                        if (distanceSquared <= myVisibilityRangeSquared)
                        {
                            // avoid when future positions are this close (or less)
                             // the whole thing is incorrect, needs radius of large target
                            //float collisionDangerThreshold = simpleVehicle.Radius * 3; // 2 is too small
                            float collisionDangerThreshold = myRadius + otherAvoidanceTarget.Radius; // 2 is too small
                            float collisionDangerThresholdSquared = collisionDangerThreshold * collisionDangerThreshold;

                            // predicted time until nearest approach of "this" and "other"
                            SBVelocity2D otherVelocity = innerLoopContainer.velocity;
                            // imagine we are at the origin with no velocity,
                            // compute the relative velocity of the other vehicle
                            float2 relVelocity = otherVelocity.Value - myVelocity.Value;

                            float time = PredictNearestApproachTime(relPositionOfMeRelativeToTarget, relVelocity);

                            // If the time is in the future, sooner than any other
                            // threatened collision...
                            if ((time >= 0) && (time < minTime))
                            {
                                float2 otherForwardNormalized = innerLoopContainer.forwardNormalized;
                                float otherSpeed = innerLoopContainer.speed;

                                // if the two will be close enough to collide,
                                // make a note of it
                                float approachDistanceSquared = ComputeNearestApproachPositions(
                                        myForwardDirectionNormalized, mySpeed, myPosition,
                                        otherForwardNormalized, otherSpeed, otherPosition,
                                        time,
                                        out float2 hisPositionAtNearestApproach);
                        
                                if (approachDistanceSquared < collisionDangerThresholdSquared)
                                {
                                    minTime = time;
                                    threat = otherEntity;
                                    threatPosition = otherPosition;
                                    threatRotation = otherRotation;
                                    threatVelocity = otherVelocity;
                                    threatSpeed = otherSpeed;

                                    xxxThreatPositionAtNearestApproach = hisPositionAtNearestApproach;
                                    //xxxOurPositionAtNearestApproach = ourPositionAtNearestApproach;
                                }
                            }
                        }
                    }
                }
            }

            // if a potential collision was found, compute steering to avoid
            if (threat != Entity.Null)
            {
                if (threatSpeed <= minimumSpeed)
                {
                    // direction has no meaning for stationary targets
                    // I am flying towards a stationary target
                    float2 offset = xxxThreatPositionAtNearestApproach - myPosition.Value;
                    float sideDot = math.dot(offset, mySideDirectionNormalized);
                    steer = (sideDot > 0) ? -1.0f : 1.0f;
                }
                else
                {
                    float2 threatForward = new float2();
                    math.sincos(threatRotation.HeadingAngle, out threatForward.x, out threatForward.y);

                    // parallel: +1, perpendicular: 0, anti-parallel: -1
                    float parallelness = math.dot(myForwardDirectionNormalized, threatForward);
                    float angle = 0.707f;

                    if (parallelness < -angle)
                    {
                        // anti-parallel "head on" paths:
                        // steer away from future threat position
                        float2 offset = xxxThreatPositionAtNearestApproach - myPosition.Value;
                        float sideDot = math.dot(offset, mySideDirectionNormalized);
                        steer = (sideDot > 0) ? -1.0f : 1.0f;
                    }
                    else
                    {
                        if (parallelness > angle)
                        {
                            // parallel paths: steer away from threat
                            float2 offset = threatPosition.Value - myPosition.Value;
                            float sideDot = math.dot(offset, mySideDirectionNormalized);
                            steer = (sideDot > 0) ? -1.0f : 1.0f;
                        }
                        else
                        {
                            // perpendicular paths: steer behind threat
                            // (only the slower of the two does this)
                            if (threatSpeed <= mySpeed)
                            {
                                float sideDot = math.dot(mySideDirectionNormalized, threatVelocity.Value);
                                steer = (sideDot > 0) ? -1.0f : 1.0f;
                            }
                        }
                    }
                }

                //AnnotateAvoidNeighbor (*threat,
                //                       steer,
                //                       xxxOurPositionAtNearestApproach,
                //                       xxxThreatPositionAtNearestApproach);
            }

            // unit vector is too tiny to make any difference
            // incorrect, uses current speed, should use SimpleVehicle.MaxSpeed
            // steer *= mySpeed; // less than 400 - repair ships collide with main station

            // greater than 100 - ships jump very unnaturally when avoiding asteroids or other ships
            // less than 400 - repair ships collide with main station
            steer *= simpleVehicle.MaxSpeed;
            return mySideDirectionNormalized * steer;
        }

        // Given two vehicles, based on their current positions and velocities,
        // determine the time until nearest approach
        //
        // XXX should this return zero if they are already in contact?

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float PredictNearestApproachTime(float2 relPosition, float2 relVelocity)
        {
            float relSpeed = math.length(relVelocity);

            // for parallel paths, the vehicles will always be at the same distance,
            // so return 0 (aka "now") since "there is no time like the present"
            if (relSpeed == 0) return 0;

            // Now consider the path of the other vehicle in this relative
            // space, a line defined by the relative position and velocity.
            // The distance from the origin (our vehicle) to that line is
            // the nearest approach.

            // Take the unit tangent along the other vehicle's path
            float2 relTangent = relVelocity / relSpeed;

            // find distance from its path to origin (compute offset from
            // other to us, find length of projection onto path)
            float projection = math.dot(relTangent, relPosition);

            return projection / relSpeed;
        }

        // Given the time until nearest approach (predictNearestApproachTime)
        // determine position of each vehicle at that time, and the distance
        // between them

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ComputeNearestApproachPositions(
            float2 myForward,    float mySpeed,    SBPosition2D myPosition,
            float2 otherForward, float otherSpeed, SBPosition2D otherPosition,
            float time,
            out float2 hisPositionAtNearestApproach
            //out float2 ourPositionAtNearestApproach
        )
        {
            float2    myTravel =    myForward *    mySpeed * time;
            float2 otherTravel = otherForward * otherSpeed * time;

            float2    myFinal =    myPosition.Value +    myTravel;
            float2 otherFinal = otherPosition.Value + otherTravel;

            // xxx for annotation
            //ourPositionAtNearestApproach = myFinal;
            hisPositionAtNearestApproach = otherFinal;

            return math.distancesq(myFinal, otherFinal);
        }
    }
}
