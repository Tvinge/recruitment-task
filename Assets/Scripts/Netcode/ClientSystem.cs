using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
//using UnityEditor.Rendering;
using UnityEngine;


public struct ClientMassageRpcCommand : IRpcCommand 
{
    public FixedString64Bytes message;
}

public struct SpawnUnitRPCCommand : IRpcCommand
{
 
}


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientSystem : SystemBase
{
    protected override void OnCreate()
    {
     RequireForUpdate<NetworkId>();
    }
    protected override void OnUpdate()
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ServerMassageRpcCommand>>().WithEntityAccess())
        {
            Debug.Log(command.ValueRO.message);
            commandBuffer.DestroyEntity(entity);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnUnitRpc(ConnectionManager.clientWorld);
        }
        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }

    public void SendMassageRpc(string text, World world)
    {
        if (world == null || world.IsCreated == false)
        {
            return;
        }
        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ClientMassageRpcCommand));
        world.EntityManager.SetComponentData(entity, new ClientMassageRpcCommand()
        {
            message = text
        });

    }

    public void SpawnUnitRpc(World world)
    {
        if (world == null || world.IsCreated == false)
        {
            return;
        }
        world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(SpawnUnitRPCCommand));
    }
}
