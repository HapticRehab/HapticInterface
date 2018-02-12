using UnityEngine;

public class MenuInitializer : MonoBehaviour {

    // variabile che serve per capire se la scene viene fatta partire dopo aver svolto un esercizio e aver premuto il pulsante di ritorno alle impostazioni
    //così si visualizza più il menu iniziale con start e quit, ma direttamente quello di setup
    public static int flag_second_call_of_the_scene = 0;          

    //Tutti i panel che gestiscono i menu
    public GameObject MainMenuPanel;
    public GameObject SetupPanel;
    public GameObject ReachingPanel;
    public GameObject ContouringPanel;
    public GameObject ChasingPanel;

	//Visualizza il menu giusto, controllando se è già stato svolto un esercizio o meno
	void Awake () {

        if (flag_second_call_of_the_scene == 0)
        {
            MainMenuPanel.SetActive(true);
            SetupPanel.SetActive(false);
            ReachingPanel.SetActive(false);
            ContouringPanel.SetActive(false);
            ChasingPanel.SetActive(false);
        }
        else
        {
            MainMenuPanel.SetActive(false);
            SetupPanel.SetActive(true);
            ReachingPanel.SetActive(false);
            ContouringPanel.SetActive(false);
            ChasingPanel.SetActive(false);
        }
		
	}
	
}
