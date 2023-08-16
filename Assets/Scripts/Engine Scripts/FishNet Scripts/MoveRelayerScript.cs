using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class MoveRelayerScript : NetworkBehaviour {
    public static MoveRelayerScript Instance { get; private set; }

    // // ((int, int), (int, int), (int, int), string) SelectedSingleMove;
    // [SyncObject, HideInInspector] public readonly SyncList<int> SelectedSingleMove = new SyncList<int>();
    // [SyncVar, HideInInspector] public string selectedSingleMoveName;
    // // (((int, int), (int, int)), ((int, int), (int, int)), string) SelectedMultipleMove;
    // [SyncObject, HideInInspector] public readonly SyncList<int> SelectedMultipleMove = new SyncList<int>();
    // [SyncVar, HideInInspector] public string selectedMultipleMoveName;

    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized), HideInInspector]
    public bool[] receivedBoardState = default;

    [field: SyncVar, HideInInspector] public PieceScript.Side thisSide { get; private set; }


    public override void OnStartClient() {
        base.OnStartClient();
        if (!base.IsOwner) {
            Debug.Log("Disabled a move relayer script");
            GetComponent<MoveRelayerScript>().enabled = false; // Deactivate this script if not client
        } else {
            Instance = this;
            // receivedBoardState = null;
        }
    }

    public override void OnStartServer() {
        base.OnStartServer();
        MoveRelayerManager.Instance.ConnectedClients.Add(this);
    }

    public override void OnStopServer() {
        base.OnStopServer();
        MoveRelayerManager.Instance.ConnectedClients.Remove(this);
    }

    [Client]
    public void SetClientSide(PieceScript.Side side) {
        // Tell server to tell clients to set their side
        thisSide = side;
    }

    [Client(RequireOwnership = false)]
    public void CommitBoardState(bool[] boardState) {
        MoveRelayerManager.Instance.CommitBoardStateManager(boardState, this);
    }

    [Client(RequireOwnership = false)]
    public void RelayBoardState(bool[] boardState) {
        // MoveRelayerManager.Instance.CommitBoardStateManager(boardState, this);
        receivedBoardState = boardState;
        Debug.Log(string.Join("-", receivedBoardState));
        // Receiving a boardstate correctly
        // HOWEVER, the corresponding 
        // throw new NotImplementedException("PLEASE IMPLEMENT THIS FUNCTION!");
    }
}