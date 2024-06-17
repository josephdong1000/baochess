using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoveRelayerManager : NetworkBehaviour { // NetworkBehavior
    public static MoveRelayerManager Instance { get; private set; }
    
    private readonly SyncList<int> _connectedClients = new();
    public List<int> ConnectedClients => (List<int>)_connectedClients.Collection;
    
}