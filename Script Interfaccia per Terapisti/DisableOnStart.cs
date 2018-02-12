using UnityEngine;
using UnityEngine.UI;

//Rende invisibile un elemento text alla partenza dell'applicazione
public class DisableOnStart : MonoBehaviour {

    private Text text;
	// Use this for initialization
	void Awake () {
        text = GetComponent<Text>();
        text.enabled = false;
	}
}
