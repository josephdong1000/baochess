using System;
using TMPro;
using UnityEngine;

public sealed class PlayersJoinedScript : MonoBehaviour {
    public static PlayersJoinedScript Instance { get; private set; }

    private TMP_Text _tmpText;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void Update() {
        UpdatePlayersJoined();
        // Debug.Log(string.Join(",", MoveRelayerManager.Instance.ConnectedClients));

        // string output = "";
        // foreach (MoveRelayerScript moveRelayerScript in MoveRelayerManager.Instance.ConnectedClients) {
        //     output = output + moveRelayerScript.LocalConnection + "|||| ";
        // }
        // Debug.Log(output);
    }

    public void UpdatePlayersJoined() {
        if (MoveRelayerScript.Instance != null) {
            _tmpText.text = GenerateDisplayText(MoveRelayerScript.Instance.ConnectedClients.Count);
        }
    }

    private string GenerateDisplayText(int numPlayers) {
        return $"Players: {numPlayers}/2";
    }
}