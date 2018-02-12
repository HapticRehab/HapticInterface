using System.Collections;
using UnityEngine;

//Script per rilevare un eventuale errore della socket che controlla lo stato di connessione degli Hololens
//In caso di errore l'applicazione si mostra una schermata di errore fatale
public class Detect_Fatal_Error : MonoBehaviour {

    public GameObject FatalErrorPanel;
    public GameObject SetupPanel;
    public GameObject MenuPanel;
    public GameObject ReachingPanel;
    public GameObject ContouringPanel;
    public GameObject ChasingPanel;

    //Coroutine per gestire l'errore
    IEnumerator DetectFatal() {

        while (true)
        {
            if (CheckHololens.FATAL_ERROR == 1)
            {
                FatalErrorPanel.SetActive(true);
                SetupPanel.SetActive(false);
                MenuPanel.SetActive(false);
                ReachingPanel.SetActive(false);
                ContouringPanel.SetActive(false);
                ChasingPanel.SetActive(false);
                break;
            }
            yield return new WaitForSeconds(3);
        }
        yield break;
    }

	void Start () {
        FatalErrorPanel.SetActive(false);
        StartCoroutine(DetectFatal());
	}
}
