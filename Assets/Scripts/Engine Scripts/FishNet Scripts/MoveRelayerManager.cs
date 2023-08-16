using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoveRelayerManager : NetworkBehaviour {
    public static MoveRelayerManager Instance { get; private set; }

    [field: SyncObject]
    public SyncList<MoveRelayerScript> ConnectedClients { get; } = new SyncList<MoveRelayerScript>();

    public int NumConnectedClients { get; private set; }
    [HideInInspector] public bool startMultiplayerGame { get; private set; }

    [SyncVar, HideInInspector] public PieceScript.Side playingSide;


    private void Awake() {
        Instance = this;
        startMultiplayerGame = false;
    }

    private void Start() {
        // Debug.Log("Start routine called");
    }

    private void Update() {
        if (!IsServer) return;
        RemoveNullClients();
        UpdateNumConnectedClients(ConnectedClients.Count);
    }

    public override void OnStartNetwork() {
        base.OnStartNetwork();
        if (IsServer) {
            playingSide = PieceScript.Side.White; // White plays first
            StartCoroutine(InitiatePlayerSides());
        }
    }

    [Server]
    IEnumerator InitiatePlayerSides() {
        Debug.Log("Waiting for players to queue in");
        yield return new WaitUntil(() => ConnectedClients.Count >= 2);
        SetClientSides();
        yield return new WaitUntil(() => ConnectedClients[0].thisSide != PieceScript.Side.None &&
                                         ConnectedClients[1].thisSide != PieceScript.Side.None);
        Debug.Log(ConnectedClients[0].thisSide);
        Debug.Log(ConnectedClients[1].thisSide);
        SetStartMultiplayerObservers(true);
        // StartCoroutine(RelayMovesGameLoop());
    }

    // [Server]
    // IEnumerator RelayMovesGameLoop() {
    //     
    // }

    // [Server]
    // public void CommitBoardState(bool[] boardState) {
    //     
    // }
    
    

    [Server]
    public void SetClientSides() {
        var sides = PickRandomSides();
        ConnectedClients[0].SetClientSide(sides.Item1);
        ConnectedClients[1].SetClientSide(sides.Item2);
    }

    [Server]
    public (PieceScript.Side, PieceScript.Side) PickRandomSides() {
        
        return (PieceScript.Side.White, PieceScript.Side.Black);
        // FIX THIS LATER
        throw new NotImplementedException("Holy moly");
        
        if (Random.Range(0, 2) == 0) {
            return (PieceScript.Side.White, PieceScript.Side.Black);
        }

        return (PieceScript.Side.Black, PieceScript.Side.White);
    }

    [ObserversRpc]
    public void RemoveNullClients() {
        ConnectedClients.RemoveAll(item => item == null);
    }

    [ObserversRpc]
    public void UpdateNumConnectedClients(int numConnectedClients) {
        NumConnectedClients = numConnectedClients;
    }

    [ObserversRpc]
    public void SetStartMultiplayerObservers(bool status) {
        startMultiplayerGame = status;
        // BoardScript.SelectingMove = false;
        // yield return BoardScript.ResetGameLoop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void CommitBoardStateManager(bool[] boardState, MoveRelayerScript script) {
        if (script.thisSide == ConnectedClients[0].thisSide) {
            ConnectedClients[1].RelayBoardState(boardState);
        } else if (script.thisSide == ConnectedClients[1].thisSide) {
            ConnectedClients[0].RelayBoardState(boardState);
        } else {
            throw new NotImplementedException("Not valid script");
        }
        // if (ReferenceEquals(script, ConnectedClients[0])) {
        //     ConnectedClients[1].RelayBoardState(boardState);
        // } else if (ReferenceEquals(script, ConnectedClients[1])) {
        //     ConnectedClients[0].RelayBoardState(boardState);
        // } else {
        //     throw new NotImplementedException("Not valid script");
        // }
        FlipPlayingSide();
    }
    
    // [ObserversRpc]
    // public void RelayBoardStateManager(bool[] boardState, MoveRelayerScript script){
        // if(connclie)
    // }
    
    [Server]
    public void FlipPlayingSide() {
        playingSide = BoardScript.InvertSide(playingSide);
        // for (int i = 0; i < ConnectedClients.Count; i++) {
        //     ConnectedClients[i].SelectedSingleMove.Clear();
        //     ConnectedClients[i].selectedSingleMoveName = "";
        //     ConnectedClients[i].SelectedMultipleMove.Clear();
        //     ConnectedClients[i].selectedMultipleMoveName = "";
        // }
    }
}