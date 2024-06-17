using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using SceneManager = FishNet.Managing.Scened.SceneManager;

public sealed class MoveRelayerScript : NetworkBehaviour {
    public static MoveRelayerScript Instance { get; private set; }

    private void Awake() {
        
    }

    // [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized), HideInInspector]
    // public bool[] receivedBoardState = default;

    // [field: SyncVar, HideInInspector] public PieceScript.Side thisSide { get; private set; }

    // [SyncObject(WritePermissions = WritePermission.ServerOnly)]
    // public readonly SyncList<NetworkConnection> ConnectedClients = new SyncList<NetworkConnection>();
    // [field: SyncObject(ReadPermissions = ReadPermission.Observers, WritePermissions = WritePermission.ServerOnly)]
    // public SyncList<NetworkConnection> ConnectedClients { get; } = new SyncList<NetworkConnection>();


    // [SyncObject(WritePermissions = WritePermission.ServerOnly)] private readonly SyncList<int> _connectedClients = new();
    private readonly SyncList<int> _connectedClients = new();
    public List<int> ConnectedClients => (List<int>)_connectedClients.Collection;

    [HideInInspector] public readonly SyncVar<bool> startMultiplayerGame = new();

    public PieceScript.Side ThisSide { get; private set; }
    // [field: SyncVar] public PieceScript.Side PlayingSide { get; private set; }
    // [SyncVar] public int PlayingSideInt;
    public readonly SyncVar<int> PlayingSideInt = new();

    private SceneManager _sceneManager;

    public override void OnStartClient() {
        base.OnStartClient();
        if (!base.IsOwner) {
            Debug.Log($"Disabling move relay script for client {OwnerId}");
            // GetComponent<MoveRelayerScript>().enabled = false; // Deactivate this script if not client
        } else {
            Debug.Log($"Setting singleton Instance to client {OwnerId}");
            Instance = this;
            // ServerAddConnectedClient(Owner);
        }
    }

    public override void OnStartNetwork() {
        base.OnStartNetwork();
        
        if (IsServer) {
            Debug.Log("OnStartNetwork called for server");
            startMultiplayerGame.Value = false;
            PlayingSideInt.Value = 0;
            _sceneManager = NetworkManager.SceneManager;
            _sceneManager.OnClientPresenceChangeEnd += ChangeConnectedClientsEvent;
            _connectedClients.OnChange += DetectStartGameEvent;
        }
        // if (IsClient && base.Owner.IsLocalClient) {
        //     if (base.Owner.IsLocalClient) {
        //         Debug.Log($"Setting singleton Instance to client {OwnerId}");
        //                     Instance = this;
        //     } else {
        //         Debug.Log($"Disabling move relay script for client {OwnerId}");
        //         GetComponent<MoveRelayerScript>().enabled = false;
        //     }
        // }
    }

    private void ChangeConnectedClientsEvent(ClientPresenceChangeEventArgs obj) {
        if (obj.Added) {
            _connectedClients.Add(obj.Connection.ClientId);
        } else {
            _connectedClients.Remove(obj.Connection.ClientId);
        }
    }

    private void DetectStartGameEvent(SyncListOperation op, int index, int olditem, int newitem, bool asserver) {
        if (op == SyncListOperation.Add && _connectedClients.Count == 2) {
            ServerSetSides();
            // PlayingSide = PieceScript.Side.White; // White starts the game
            PlayingSideInt.Value = 1;
            startMultiplayerGame.Value = true;
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

    // [ServerRpc]
    // public void ServerAddConnectedClient(NetworkConnection client) {
    //     Debug.Log(IsServer);
    //     Debug.Log($"ConnectedClients: {String.Join(", ", _connectedClients)}");
    //     _connectedClients.Add(client.ClientId);
    //     Debug.Log($"AddConnectedClient called on server with client {client}");
    //     Debug.Log($"ConnectedClients: {String.Join(", ", _connectedClients)}");
    //     
    //     // Move this to a new thing
    //     if (_connectedClients.Count == 2) {
    //         Debug.Log("Server has received 2 clients");
    //         ServerSetSides();
    //         PlayingSide = PieceScript.Side.White; // White starts the game
    //         startMultiplayerGame = true;
    //     } else if (_connectedClients.Count > 2) {
    //         throw new NotImplementedException("More than 2 clients in _connectedClients!");
    //     }
    // }

    // public override void OnStopClient() {
    //     base.OnStopClient();
    //     if (base.IsOwner) {
    //         Debug.Log($"Client {Owner} being removed");
    //         ServerRemoveConnectedClient(Owner);
    //     }
    // }

    // [ServerRpc]
    // public void ServerRemoveConnectedClient(NetworkConnection client) {
    //     _connectedClients.Remove(client.ClientId);
    // }

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
        SetClientSide(ServerManager.Clients[_connectedClients[0]], sides.Item1);
        SetClientSide(ServerManager.Clients[_connectedClients[1]], sides.Item2);
    }

    [TargetRpc]
    public void SetClientSide(NetworkConnection client, PieceScript.Side side) {
        ThisSide = side;
    }

    // private void Start() {
    //     if (IsServer) {
    //         startMultiplayerGame = false;
    //     }
    // }

    private void Update() {
        // if (IsServer) {
        //     RemoveNullClients();
        // }

        if (Input.GetKeyDown(KeyCode.Z)) {
            Debug.Log(String.Join(", ", _connectedClients));
            Debug.Log($"Is client: {IsClient}");
            Debug.Log($"Client ID and IsOwner: {(OwnerId, IsOwner)}");
            Debug.Log($"Start multi: {startMultiplayerGame}");
            Debug.Log("-----");
        }
    }


    // [Client(RequireOwnership = false)]
    /// <summary>
    /// Called from any client onto the server. Triggers a TargetRpc to send the board information
    /// to the other client
    /// </summary>
    [ServerRpc]
    public void SendBoardState(bool[] boardState, int callingClientID) {
        // MoveRelayerManager.Instance.CommitBoardStateManager(boardState, this);
        if (_connectedClients[0] == callingClientID) {
            ReceiveBoardState(ServerManager.Clients[_connectedClients[1]], boardState);
        } else {
            ReceiveBoardState(ServerManager.Clients[_connectedClients[0]], boardState);
        }

        throw new NotImplementedException();
    }

    [ServerRpc]
    public void NetworkFlipPlayingSide() {
        // PlayingSide = BoardScript.InvertSide(PlayingSide);
        PlayingSideInt.Value *= -1;
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


    // [ServerRpc]
    // public void RemoveNullClients() {
    //     _connectedClients.RemoveAll(item => item == null);
    // }
}