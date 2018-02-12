using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableStartButtonChasing : MonoBehaviour {

    Dropdown dd1;
    public Dropdown dd2;
    public Dropdown dd3;
    public Button startButton;
    public Text startText;

    // Use this for initialization
    void Start()
    {
        dd1 = GetComponent<Dropdown>();
    }

    //Funzione che controlla che siano stati impostati tutti i valori dei dropdown dell'esercizio di chasing prima di abilitare
    //il pulsante di start
    public void CheckIfStartAllowed()
    {
        if ((dd1.value != 0) && (dd2.value != 0) && (dd3.value != 0))
        {
            startButton.interactable = true;
            startText.color = Color.white;
        }
        else
        {
            if (startButton.interactable == true)
            {
                startButton.interactable = false;
                startText.color = new Color(165.0f / 255.0f, 165.0f / 255.0f, 165.0f / 255.0f, 255.0f / 255.0f);
            }
        }
    }
}
