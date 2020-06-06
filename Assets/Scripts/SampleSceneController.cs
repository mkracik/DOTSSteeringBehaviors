using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

using SteeringBehaviors;

public class SampleSceneController : MonoBehaviour
{
    public GameObject ShipPrefab;

    // Start is called before the first frame update
    void Start()
    {
        SpawnRandomShips();
        
    }

    private void SpawnRandomShips()
    {
        Entity shipPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(ShipPrefab,
            GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null));
            
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)System.Environment.TickCount);
        for (int i = 0; i < 20; i++)
        {
            Entity newShipEntity = entityManager.Instantiate(shipPrefabEntity);
            entityManager.SetComponentData(newShipEntity, new SBPosition2D { Value = random.NextFloat2(-15, 15) });
            float headingAngle = random.NextFloat(-math.PI / 2, math.PI / 2);
            entityManager.SetComponentData(newShipEntity, new SBRotation2D { HeadingAngle = headingAngle }); // useless, set by velocity
            float2 velocity;
            math.sincos(headingAngle, out velocity.x, out velocity.y);
            velocity *= random.NextFloat(1, 3);
            
            entityManager.SetComponentData(newShipEntity, new SBVelocity2D { Value = velocity });
            
        }
    }
}
