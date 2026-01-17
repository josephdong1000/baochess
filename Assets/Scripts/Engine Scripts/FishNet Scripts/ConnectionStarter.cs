using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour {
    private static Tugboat _tugboat;

    private void OnEnable() {
        // ClientManager is only the local client.
        // This relays changes from the local ClientManager to the script
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable() {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args) {
        // If the local client is going to start, give a message
        if (args.ConnectionState == LocalConnectionState.Starting) {
            Debug.Log("Starting client", this);
        }
        // If the local client is going to stop, give a message
        if (args.ConnectionState == LocalConnectionState.Stopping) {
            Debug.Log("Stopping client (perhaps server stopped)", this);
        }
    }

    private void Start() {
        if (TryGetComponent(out Tugboat _t)) {
            _tugboat = _t;
        } else {
            Debug.LogError("No tugboat present", this);
            return;
        }
    }

    public static void StartServerConnection() {
        _tugboat.StartConnection(true);
    }

    public static void StartClientConnection() {
        _tugboat.StartConnection(false);
    }
    
}
