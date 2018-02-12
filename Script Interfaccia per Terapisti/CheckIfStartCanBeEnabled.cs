using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckIfStartCanBeEnabled : MonoBehaviour {
    
    //Dropdown che permette di scegliere l'esercizio da fare
    Dropdown excercise;
    public Dropdown dd1, dd2, dd3, dd4, dd5;        //Dropdown del reaching
    public Dropdown dd6, dd7, dd8, dd9;             //Dropdown del contouring
    public Dropdown dd10, dd11, dd12;               //Dropdown del chasing
    public Button startButton;                      //Bottone per iniziare l'esercizio
    public Text startText;                          //Testo visualizzato dal bottone per iniziare l'esrcizio

	// Use this for initialization
	void Start () {
        excercise = GetComponent<Dropdown>();
	}
	
    //Funzione che, ogni qual volta varia il valore del dropdown che controlla l'esercizio da eseguire, controlla se il bottone di start può essere abilitato
    public void Checker()
    {
        switch (excercise.value)
        {
            case 1:
                if((dd1.value != 0) && (dd2.value != 0) && (dd3.value != 0) && (dd4.value != 0) && (dd5.value != 0))
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
                break;
            case 2:
                if ((dd6.value != 0) && (dd7.value != 0) && (dd8.value != 0) && (dd9.value != 0))
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
                break;
            case 3:
                if ((dd10.value != 0) && (dd11.value != 0) && (dd12.value != 0))
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
                break;
            default:
                startButton.interactable = false;
                startText.color = new Color(165.0f / 255.0f, 165.0f / 255.0f, 165.0f / 255.0f, 255.0f / 255.0f);
                break;
        }

    }
}
