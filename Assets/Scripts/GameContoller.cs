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
		if (instance == null) {
			instance = this;
		} else if(instance != this) {
			Destroy(this);
		}

		q = new Queue (MAX_LOG_ENTRIES);

		log ("Game started!");
	}

	public void log(string message)
	{
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
