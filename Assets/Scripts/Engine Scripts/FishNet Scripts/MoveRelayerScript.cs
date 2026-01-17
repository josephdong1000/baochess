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

// Singleton, networked, for servers + clients
// Multiple copies of this script object will be created around the map
public sealed class MoveRelayerScript : NetworkBehaviour {
    // Singleton object, referenced everywhere
    public static MoveRelayerScript Instance { get; private set; }

    // public readonly SyncVar<bool> StartMultiplayerGame = new();
    private readonly SyncVar<GameState> _gameState = new();
    private readonly SyncVar<int> _numPlayersJoined = new();
    private readonly SyncList<MoveRelayerScript> _playersOnline = new();
    private readonly SyncList<MoveRelayerScript> _playersInThisGame = new();

    private PieceScript.Side _myPlayingSide; // Not networked, only set locally
    
    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    public override void OnStartNetwork() {
        base.OnStartNetwork();
        
        // if (IsServerInitialized) {
        //     ChangeGameState(GameState.InitBoard);
        // }
    }
    

    public override void OnStartClient() {
        base.OnStartClient();
        // On the client, run for every object instantiated (yours or not).
        // However, externally spawned objects don't have this script, so no check needed
        
        Instance._numPlayersJoined.Value++;
        Instance._playersOnline.Add(Instance);
        if (_playersOnline.Count <= 2) {
            Debug.Log("playersOnline.count <= 2");
            Instance._playersInThisGame.Add(Instance);
            if (Instance._playersInThisGame.Count == 2) {
                Debug.Log("playersinthisgame.count == 2");
                ChangeGameState(GameState.InitBoard);
            }
        }
    }

    public override void OnStopClient() {
        base.OnStopClient();

        Instance._numPlayersJoined.Value--;
        Instance._playersOnline.Remove(Instance);
        Instance._playersInThisGame.Remove(Instance);
    }

    /// <summary>
    /// Should only be run by the server, never by a client directly
    /// </summary>
    /// <param name="_newState"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [Server, ServerRpc]
    private void ChangeGameState(GameState _newState) {
        _gameState.Value = _newState;
        switch (_newState) {
            case GameState.None:
                break;
            case GameState.InitBoard:
                InitializeBoard();
                break;
            case GameState.WhiteTurn:
                break;
            case GameState.BlackTurn:
                break;
            case GameState.RoundEnd:
                break;
            case GameState.GameEnd:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_newState), _newState, null);
        }
    }
    
    /// <summary>
    /// Initializes the clients' PlayingSides 
    /// </summary>
    [ServerRpc]
    private void InitializeBoard() {
        Debug.Log("Initializing board called");
        (PieceScript.Side, PieceScript.Side) sides = GenerateRandomSides();
        _playersInThisGame[0].SetMyPlayingSide(sides.Item1);
        _playersInThisGame[1].SetMyPlayingSide(sides.Item2);
        ChangeGameState(GameState.WhiteTurn);
    }

    private (PieceScript.Side, PieceScript.Side) GenerateRandomSides() {
        if (Random.Range(0, 2) == 0) {
            return (PieceScript.Side.White, PieceScript.Side.Black);
        }
        return (PieceScript.Side.Black, PieceScript.Side.White);
    }
    
    public void NetworkFlipPlayingSide() {
        switch (_gameState.Value) {
            case GameState.WhiteTurn:
                _gameState.Value = GameState.BlackTurn;
                break;
            case GameState.BlackTurn:
                _gameState.Value = GameState.WhiteTurn;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Must complete!!!!!!!!
    /// </summary>
    /// <param name="boardState"></param>
    /// <param name="callingClientID"></param>
    public void SendBoardState(bool[] boardState, int callingClientID) {
        
    }

    public bool CanStartMultiplayerGame() {
        return _gameState.Value is GameState.WhiteTurn or GameState.BlackTurn;
    }

    public PieceScript.Side GetMyPlayingSide() {
        return _myPlayingSide;
    }

    public void SetMyPlayingSide(PieceScript.Side mySide) {
        _myPlayingSide = mySide;
        Debug.Log($"Set my playing side to {_myPlayingSide}");
    }

    public int GetNumPlayersJoined() {
        return _numPlayersJoined.Value;
    }


    enum GameState {
        None = 0,
        InitBoard,
        WhiteTurn,
        BlackTurn,
        RoundEnd, // May not be used
        GameEnd
    }
}

/*
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

private void Update() {

    if (Input.GetKeyDown(KeyCode.Z)) {
        Debug.Log(String.Join(", ", _connectedClients));
        Debug.Log($"Is client: {IsClient}");
        Debug.Log($"Client ID and IsOwner: {(OwnerId, IsOwner)}");
        Debug.Log($"Start multi: {startMultiplayerGame}");
        Debug.Log("-----");
    }
}


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

[TargetRpc]
public void ReceiveBoardState(NetworkConnection client, bool[] boardState) {
    BoardScript.receivedEnemyBoardState = boardState;
}
}*/