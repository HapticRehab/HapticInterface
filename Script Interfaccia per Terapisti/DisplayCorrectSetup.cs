using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Script che mostra i vari menu dei singoli esercizi a seconda del valore del dropdown della scelta dell'esercizio

public class DisplayCorrectSetup : MonoBehaviour {

    Dropdown drop;
    public GameObject ReachingPanel;
    public GameObject ContouringPanel;
    public GameObject ChasingPanel;

	// Use this for initialization
	void Start () {
        drop = GetComponent<Dropdown>();
	}
	
	// Update is called once per frame
	public void Activate()
    {
        switch (drop.value)
        {
            case 0:
                ReachingPanel.SetActive(false);
                ContouringPanel.SetActive(false);
                ChasingPanel.SetActive(false);
                break;
            case 1:
                ReachingPanel.SetActive(true);
                ContouringPanel.SetActive(false);
                ChasingPanel.SetActive(false);
                break;
            case 2:
                ReachingPanel.SetActive(false);
                ContouringPanel.SetActive(true);
                ChasingPanel.SetActive(false);
                break;
            case 3:
                ReachingPanel.SetActive(false);
                ContouringPanel.SetActive(false);
                ChasingPanel.SetActive(true);
                break;
            default:
                ReachingPanel.SetActive(false);
                ContouringPanel.SetActive(false);
                break;

        }
    }
}
