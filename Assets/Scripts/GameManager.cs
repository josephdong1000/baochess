using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        } else if (Input.GetKeyDown(KeyCode.Q)) {
            Application.Quit();
        }
    }
}
