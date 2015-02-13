using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameContoller : MonoBehaviour {
    const int MAX_LOG_ENTRIES = 50;

    public static GameContoller instance;
    public Text console;

    private Queue q;

    void Start()
    {
        Screen.showCursor = false;
        if (instance == null) {
            instance = this;
        } else if(instance != this) {
            Destroy(this);
        }

        q = new Queue (MAX_LOG_ENTRIES);

        log ("Game started!");
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.KeypadMultiply)) {
            console.enabled = !console.enabled;
            q.Clear();
            console.text = "";
        }
    }

    public void log(string message)
    {
        if (!console.enabled) {
            return;
        }

        while(q.Count >= MAX_LOG_ENTRIES) {
            q.Dequeue();
        }

        q.Enqueue(message + "\n");

        console.text = "";
        foreach (string queueMessage in q) {
            console.text += queueMessage;
        }
    }
}
