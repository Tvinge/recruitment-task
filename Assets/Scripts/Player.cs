using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5f;
}
public struct PlayerData : IComponentData
{

    public float speed;
}

    public class PlayerBaker : Baker<Player>
{
    public override void Bake(Player authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerData
        {
            speed = authoring.speed,
        });
        AddComponent<PlayerInputData>(entity);
    }
}
