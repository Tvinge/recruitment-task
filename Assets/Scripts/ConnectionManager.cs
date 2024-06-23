using System;
using System.Collections;
using System.Net;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
//using Unity.Scenes.Editor;
using UnityEngine;
using Unity.Services.Core;

public class ConnectionManager : MonoBehaviour
{
    /*
    [SerializeField] public string _listenIP = "35.228.46.14";
    [SerializeField] public string _connectIP = "35.228.46.14";
    [SerializeField] public ushort _port = 9000;
    */
    [SerializeField] public string _listenIP = "127.0.0.1";
    [SerializeField] public string _connectIP = "127.0.0.1";
    [SerializeField] public ushort _port = 7979;


    public static World serverWorld = null;
    public static World clientWorld = null;



    public enum Role
    {
        ServerClient = 0, Server = 1, Client = 2
    }

    private static Role _role = Role.ServerClient;


    private async void Start()
    {
        Application.targetFrameRate = 60;
        await UnityServices.InitializeAsync();
    }

    public void StartGame()
    {
        if (Application.isEditor)
        {
            _role = Role.ServerClient;
        }
        else if (Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.LinuxServer|| Application.platform == RuntimePlatform.OSXServer)
        {
            _role = Role.Server;
        }
        else
        {
            _role = Role.Client;
        }
        Debug.Log("game started");
        StartCoroutine(Connect());

    }
    private IEnumerator Connect()
    {
        Debug.Log("Connect was invoked");

        if (_role == Role.ServerClient || _role == Role.Server)
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        }
        if (_role == Role.ServerClient || _role == Role.Client)
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        }

        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if (serverWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        }
        else if (clientWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = clientWorld;
        }

        SubScene[] subScenes = FindObjectsByType<SubScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        //Server listens for incoming connections
        if (serverWorld != null)
        {
            while (!serverWorld.IsCreated)
            {
                yield return null;
            }
            if (subScenes != null)
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(serverWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while (!SceneSystem.IsSceneLoaded(serverWorld.Unmanaged, sceneEntity))
                    {
                        serverWorld.Update();
                    }
                }
            }
            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.Parse(_listenIP, _port));
        }

        //endpoint is the server IP adress and port which the client will connect to
        if (clientWorld != null)
        {
            while (!clientWorld.IsCreated)
            {
                yield return null;
            }
            if (subScenes != null)
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(clientWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while (!SceneSystem.IsSceneLoaded(clientWorld.Unmanaged, sceneEntity))
                    {
                        clientWorld.Update();
                    }
                }
            }
            //connect client to server
            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.Parse(_connectIP, _port));
        }
    }
}
