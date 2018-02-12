using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts;
using System.Threading;

//Questa classe server a controllare lo stato della connessione dell'Hololens al server
public class CheckHololens : MonoBehaviour {

    //Elemento text per visualizzare un messaggio relativo allo stato della connessione dell'Hololens
    public Text HololensInfo;

    //Istanza della classe per gestire la comunicazione
    private HapticSock checkHololens;

    //Dati per inizializzare la socket
    private string ipAddress = "127.0.0.1";
    private int port = 9003;

    //Thread per ricevere i dati sulla connessione dell'Hololens
    private Thread communication;

    //Flag per segnalare l'avvenuta ricezione di un dato legato all'Hololens
    public static int flag_Hololens = 0;

    //Flag per segnalare un errore per cui si deve chiudere l'applicazione
    public static int FATAL_ERROR = 0;

    //Istanza della classe. Serve a non duplicare il GameObject ad ogni cambio scena
    public static CheckHololens playerInstance;

    //ID delle scene
    private const int MenuScene = 0;
    private const int ExcerciseScene = 1;
    
    
    //Si fa in modo di non duplicare questo GameObject ad ogni nuova scena
    void Awake()
    {
        DontDestroyOnLoad(this);

        if (playerInstance == null)
        {
            playerInstance = this;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }

    //Inizializzazione della coneessione
    void Start () {
        
        HololensInfo.text = "Hololens Is Not Connected";
        checkHololens = new HapticSock(ipAddress, port);
        communication = new Thread(checkHololens.Client_Hololens_Checker);
        communication.Start();
	}


    void Update () {
        
        //Controllo dello stato della connessione con l'Hololens
        if (checkHololens.flag_receive_check == 1)
        {
            if (checkHololens.check == "no")
            {
                HololensInfo.text = "Hololens Is Not Connected";
                flag_Hololens = 0;
            }
            else
            {
                 if (checkHololens.check == "si")
                 {
                    HololensInfo.text = "Hololens Is Connected";
                    flag_Hololens = 1;
                 }
            }
        }

        //Gestione della rilevazione di un problema di connessione
        if(checkHololens.Connection_problem==1)
        {
            if (SceneManager.GetActiveScene().buildIndex == ExcerciseScene)
            {
                SceneManager.LoadScene(MenuScene);
            }
            FATAL_ERROR = 1;
        }
	}

    //Alla chiusura dell'applicazione si chiude la socket
    void OnApplicationQuit()
    {
        checkHololens.CloseSock();    
    }
}
