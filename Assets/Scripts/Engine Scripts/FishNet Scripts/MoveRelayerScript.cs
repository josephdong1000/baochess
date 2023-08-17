using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class MoveRelayerScript : NetworkBehaviour {
    public static MoveRelayerScript Instance { get; private set; }

    // [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized), HideInInspector]
    // public bool[] receivedBoardState = default;

    // [field: SyncVar, HideInInspector] public PieceScript.Side thisSide { get; private set; }

    // [SyncObject(WritePermissions = WritePermission.ServerOnly)]
    // public readonly SyncList<NetworkConnection> ConnectedClients = new SyncList<NetworkConnection>();
    [field: SyncObject(ReadPermissions = ReadPermission.Observers, WritePermissions = WritePermission.ServerOnly)]
    public SyncList<NetworkConnection> ConnectedClients { get; } = new SyncList<NetworkConnection>();

    [field: SyncVar] public bool StartMultiplayerGame { get; private set; }

    public PieceScript.Side ThisSide { get; private set; }
    [field: SyncVar] public PieceScript.Side PlayingSide { get; private set; }

    public override void OnStartClient() {
        base.OnStartClient();
        
        if (!base.IsOwner) {
            Debug.Log("Disabled a move relayer script");
            GetComponent<MoveRelayerScript>().enabled = false; // Deactivate this script if not client
        } else {
            Debug.Log("Adding a NetworkConnection client to ConnectedClients");
            Instance = this;
            ServerAddConnectedClient(Owner);
        }
    }

    // public override void OnStartServer() {
    //     // Called on the server after objects are initialized.
    //     // Will be run many times, each time for each client connected
    //     base.OnStartServer();
    //     ConnectedClients.Add(this);
    //     if (ConnectedClients.Count == 2) {
    //         ServerSetSides();
    //         PlayingSide = PieceScript.Side.White; // White starts the game
    //         startMultiplayerGame = true;
    //     } else if (ConnectedClients.Count > 2) {
    //         throw new NotImplementedException("More than 2 clients in ConnectedClients!");
    //     }
    // }


    [ServerRpc]
    public void ServerAddConnectedClient(NetworkConnection client) {
        Debug.Log(IsServer);
        Debug.Log($"ConnectedClients: {String.Join(", ", ConnectedClients)}");
        ConnectedClients.Add(client);
        Debug.Log($"AddConnectedClient called on server with client {client}");
        Debug.Log($"ConnectedClients: {String.Join(", ", ConnectedClients)}");
        if (ConnectedClients.Count == 2) {
            Debug.Log("Server has received 2 clients");
            ServerSetSides();
            PlayingSide = PieceScript.Side.White; // White starts the game
            StartMultiplayerGame = true;
        } else if (ConnectedClients.Count > 2) {
            throw new NotImplementedException("More than 2 clients in ConnectedClients!");
        }
    }
    
    public override void OnStopClient() {
        base.OnStopClient();
        if (base.IsOwner) {
            Debug.Log($"Client {Owner} being removed");
            ServerRemoveConnectedClient(Owner);
        }
    }

    [ServerRpc]
    public void ServerRemoveConnectedClient(NetworkConnection client) {
        ConnectedClients.Remove(client);
    }

    // public override void OnStopServer() {
    //     // Called on the server right before objects are de-initialized.
    //     // Will be run many times, once for every disconnected client.
    //     base.OnStopServer();
    //     // MoveRelayerManager.Instance.ConnectedClients.Remove(this);
    //     ConnectedClients.Remove(this);
    // }

    /// <summary>
    /// Run by the server. Sets each client to a random side.
    /// Assumes only 2 clients connected (fix this?)
    /// </summary>
    public void ServerSetSides() {
        // Pick random sides
        (PieceScript.Side, PieceScript.Side) sides;
        if (Random.Range(0, 2) == 0) {
            sides = (PieceScript.Side.White, PieceScript.Side.Black);
        } else {
            sides = (PieceScript.Side.Black, PieceScript.Side.White);
        }

        // Assign those sides
        SetClientSide(ConnectedClients[0], sides.Item1);
        SetClientSide(ConnectedClients[1], sides.Item2);
    }

    [TargetRpc]
    public void SetClientSide(NetworkConnection client, PieceScript.Side side) {
        ThisSide = side;
    }

    private void Start() {
        if (IsServer) {
            StartMultiplayerGame = false;
        }
    }

    private void Update() {
        if (IsServer) {
            RemoveNullClients();
        }
    }


    // [Client(RequireOwnership = false)]
    /// <summary>
    /// Called from any client onto the server. Triggers a TargetRpc to send the board information
    /// to the other client
    /// </summary>
    /// <param name="boardState">The boardstate bool[] array</param>
    /// <param name="playedSide">The side that just played the move</param>
    [ServerRpc]
    public void SendBoardState(bool[] boardState, NetworkConnection callingClient) {
        // MoveRelayerManager.Instance.CommitBoardStateManager(boardState, this);
        if (ConnectedClients[0] == callingClient) {
            ReceiveBoardState(ConnectedClients[1], boardState);
        } else {
            ReceiveBoardState(ConnectedClients[0], boardState);
        }

        throw new NotImplementedException();
    }

    [ServerRpc]
    public void NetworkFlipPlayingSide() {
        PlayingSide = BoardScript.InvertSide(PlayingSide);
    }

    // [Client(RequireOwnership = false)]
    // public void RelayBoardState(bool[] boardState) {
    //     // MoveRelayerManager.Instance.CommitBoardStateManager(boardState, this);
    //     receivedBoardState = boardState;
    //     Debug.Log(string.Join("-", receivedBoardState));
    //     // Receiving a boardstate correctly
    //     // HOWEVER, the corresponding 
    // }

    [TargetRpc]
    public void ReceiveBoardState(NetworkConnection client, bool[] boardState) {
        BoardScript.receivedEnemyBoardState = boardState;
    }


    [ServerRpc]
    public void RemoveNullClients() {
        ConnectedClients.RemoveAll(item => item == null);
    }
}