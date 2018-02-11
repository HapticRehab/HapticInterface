using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using Assets.Scripts;
using System.Globalization;
using UnityEngine.XR.WSA;

public class SocketConnection : MonoBehaviour
{
    //public Text received;                       //blocco di testo in cui vedo cioò che la socket riceve
    //private int abortion_called = 0;


    #region TEXT FOR SEARCH QR-CODE MESSAGE
    //Questi due elementi text servono a visualizzare il messaggio "Search for QR-Code".
    //la variabile message_displayed è usata per posizionare il contenuto di Search_Target in una posizione
    //scelta utilizzando l'interfaccia grafica di UNITY

    public Text message_displayed;              //blocco di testo che mi dice che sta partendo il programma
    public static Text Search_Target;           //blocco di testo che viene visualizzato quando l'Image Target non è stato identificato
                                                //o è stato perso
    #endregion

    #region Canvas e Text
    public Canvas ConnectedCanvas;              //Canvas che contiene l'elemento text per dare informazioni sulla connessione
    public Text connected;                      //blocco di testo che mi dice se si è connessi al server, nel caso in cui non si riesca a connettersi 
                                                //dopo 3 tentativi informa l'utente della possibilità di chiudere l'applicazione o riprovare a connettersi

    public Canvas CountdownCanvas;              //Canvas per contenere l'elemento text per visualizzare il countdown di inizio esercizio
    public Text countDown;                      //blocco di testo in cui si visualizza un conuntdown iniziale(INITIALIZING,3..2..1..start)

    public Canvas TimeRemainingCanvas;          //Canvas che contiene l'elemento text per visualizzare il tempo mancante alla fine dell'esercizio
    public Text TimeRemainingCounter;           //blocco di testo per visualizzare il tempo mancante alla fine dell'esercizio

    public Canvas RepetitionsCanvas;            //Canvas che contiene l'elemento text per visualizzare il numero di ripetizioni effettuate
    public Text RepetitionsCounter;             //blocco di testo per visualizzare il numero di ripetizioni effettuate
    #endregion

    #region Oggetti per l'uso del MYO
    //Transform che contengono i prefab delle frecce e del cerchio, vengono impostate nell'interfaccia grafica di UNITY
    public Transform ElbowUpPrefab;
    public Transform ElbowDownPrefab;
    public Transform ElbowOkPrefab;
    //Transform che vengono impostate nel codice per visualizzare i prefab delle frecce e del cerchio
    private Transform ElbowUp;
    private Transform ElbowDown;
    private Transform ElbowOk;
    #endregion

    #region variabili per il riconoscimento vocale
    private KeywordRecognizer keywordRecognizer = null;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    #endregion

    //Varibili per la gestione della connessione al server
    private int connection_attempts = 0;        //Variabile usata per tenere traccia del numero di tentativi di connessione fatti
    private int problemReported = 0;            //Flag usato per indicare che è già stato riportato all'utente che vi è un problema di connessione
    private int retryConnection = 0;            //Flag usato per indicare di riprovare a connettersi al server

    private Coroutine ExcerciseTimeCoroutine;   //Corotuine per gestire il conto alla rovescia dell'esercizio

    #region Transform per l'Avatar dell'Organo Terminale
    public Transform EndEffectorPrefabPath;     //Prefab per la scia dell'endEffector
    public Transform EndEffectorPrefabPlayer;   //Prefab dell'EndEffector
    private Transform myPathTrf;                //Transform utilizzato nel codice per istanziare la scia dell'avatar
    private Transform myEndEffectorTrf;         //Transform utilizzato nel codice per spostare l'avatar
    #endregion

    //Variabili usate nell'esercizio di Reaching
    #region GameObject per le 2 sfere dell'esercizio di Reaching
    //GameObject utilizzati per contenere i prefab delle 2 sfere usate nel reaching
    public GameObject ColliderSphere1Prefab;
    public GameObject ColliderSphere2Prefab;
    //GameObject utilizzati nel codice per istanziare le 2 sfere usate nel reaching
    private GameObject Sphere1;
    private GameObject Sphere2;
    #endregion

    #region Materiali per le sfere dell'esercizio di Reaching
    public Material SphereRed;
    public Material SphereBlue;
    public Material SphereGreen;
    public Material SphereYellow;
    #endregion

    #region variabili per inizializzare l'esercizio di Reaching
    private float diameter;                                     //diametro da scegliere per le sfere
    private float[] Sphere1Coordinates = new float[2];          //Coordinate X e Y del centro della sfera 1
    private float[] Sphere2Coordinates = new float[2];          //Coordinate X e Y del centro della sfera 2
    private int Sphere1Color;                                   //Parametro che indica il colore della sfera 1
    private int Sphere2Color;                                   //Parametro che indica il colore della sfera 2
    #endregion

    //Variabili usate nel Contouring
    #region GameObject per la parete interna ed esterna del Contouring
    //GameObject utilizzate nel codice per contenere i prefab dei bordi interni ed esterni
    private GameObject internalBorder;
    private GameObject externalBorder;
    #endregion

    #region GameObject per le pareti cilindriche del Contouring
    //Tutti i gameobject che costituiscono i bordi interni
    public GameObject Cylinder3cmInternal;
    public GameObject Cylinder5cmInternal;
    public GameObject Cylinder6cmInternal;
    public GameObject Cylinder8cmInternal;
    //Tutti i gameobject che costituiscono i bordi esterni
    public GameObject Cylinder_4_4_cmExternal;
    public GameObject Cylinder_4_6_cmExternal;
    public GameObject Cylinder_4_8_cmExternal;
    public GameObject Cylinder_5_cmExternal;
    public GameObject Cylinder_5_2_cmExternal;
    public GameObject Cylinder_5_4_cmExternal;
    public GameObject Cylinder_5_6_cmExternal;
    public GameObject Cylinder_5_8_cmExternal;
    public GameObject Cylinder_6_cmExternal;
    public GameObject Cylinder_6_2_cmExternal;
    public GameObject Cylinder_6_4_cmExternal;
    public GameObject Cylinder_6_6_cmExternal;
    public GameObject Cylinder_6_8_cmExternal;
    public GameObject Cylinder_7_cmExternal;
    public GameObject Cylinder_7_2_cmExternal;
    public GameObject Cylinder_7_4_cmExternal;
    public GameObject Cylinder_7_6_cmExternal;
    public GameObject Cylinder_7_8_cmExternal;
    public GameObject Cylinder_8_cmExternal;
    public GameObject Cylinder_8_2_cmExternal;
    public GameObject Cylinder_8_4_cmExternal;
    public GameObject Cylinder_8_6_cmExternal;
    public GameObject Cylinder_8_8_cmExternal;
    public GameObject Cylinder_9_cmExternal;
    public GameObject Cylinder_9_2_cmExternal;
    public GameObject Cylinder_9_4_cmExternal;
    public GameObject Cylinder_9_6_cmExternal;
    public GameObject Cylinder_9_8_cmExternal;
    public GameObject Cylinder_10_cmExternal;
    public GameObject Cylinder_10_2_cmExternal;
    public GameObject Cylinder_10_4_cmExternal;
    public GameObject Cylinder_10_6_cmExternal;
    public GameObject Cylinder_10_8_cmExternal;
    public GameObject Cylinder_11_cmExternal;
    public GameObject Cylinder_11_2_cmExternal;
    public GameObject Cylinder_11_4_cmExternal;
    public GameObject Cylinder_11_6_cmExternal;
    public GameObject Cylinder_11_8_cmExternal;
    public GameObject Cylinder_12_cmExternal;
    public GameObject Cylinder_12_2_cmExternal;
    public GameObject Cylinder_12_4_cmExternal;
    public GameObject Cylinder_12_6_cmExternal;
    public GameObject Cylinder_12_8_cmExternal;
    public GameObject Cylinder_13_cmExternal;
    #endregion

    #region materiali per il colore delle pareti cilindriche del Contouring
    public Material RedBorder;
    public Material BlueBorder;
    public Material GreenBorder;
    public Material YellowBorder;
    #endregion

    #region Variabili per l'inizializzazione del Contouring
    private float x_path_center, y_path_center;             //posizione del centro delle pareti cilindriche
    private float pathDimension;                            //dimensione del percorso del contouring(S,M,L,XL)
    private float pathWidth;                                //distanza tra le due pareti cilindriche
    private int pathColor;                                  //colore delle pareti cilindriche
    #endregion

    //variabili per il chasing
    #region Variabili per gestire il percorso del chasing
    //GameObject contenenti i prefab dei percorsi del chasing
    public GameObject path4cm;
    public GameObject path6cm;
    public GameObject path8cm;
    public GameObject path10cm;
    //Variabile per gestire da codice il percorso del chasing
    private GameObject path_Chasing;            
    #endregion

    #region Variabili per gestire il cubo da inseguire nel chasing
    public GameObject CubeToChase;              //GameObject che contiene il prefab del cubo
    private GameObject cubeChase;               //GameObject per gestire da codice il cubo del chasing
    #endregion

    #region variabile per gestire la freccia nel chasing
    public GameObject ArrowChasing;             //GameObject contenente il prefab della freccia 
    private GameObject arrow_chase;             //GameObject usato per gestire la freccia da codice
    private Renderer arrowMaterial;             //Renderer per controllare il colore della freccia
    #endregion

    #region material per i colori del cubo del chasing
    public Material BlueChase;
    public Material YellowChase;
    public Material GreenChase;
    #endregion

    #region variabili per l'inizializzazione dell'esercizio di chasing
    private float x_offset_chasing,y_offset_chasing;        //coordinate del centro del percorso del chasing
    private int path_choice;                                //Parametro per impostare la dimensione del percorso di chasing
    private int chaseColor;                                 //Parametro per impostare il colore del cubo del chasing
    private float radius;                                   //Variabile che contiene il primo raggio della circonferenza del chasing, viene usata solo per spawnare la circonferenza
    private int arrow_flag = 0;                             //Flag che indica quando istanziare la freccia del chasing
    private float distance,arrow_angle;                     //Vairabili usate per modificare la grandezze e l'orientazione della freccia
    private int old_radius, new_radius;                     //Variabili usate per capire se è necessario modificare la grandezza della circonferenza del chasing
    #endregion

    #region Funzioni per la scelta dei colori dei GameObject
    //Funzione per dare alle sfere del reaching il colore giusto
    private Material SphereColor(int choice)
    {
        Material mat = new Material(Shader.Find("Standard"));
        switch (choice)
        {
            case (1):
                mat = SphereBlue;
                break;
            case (2):
                mat = SphereRed;
                break;
            case (3):
                mat = SphereYellow;
                break;
            case (4):
                mat = SphereGreen;
                break;
            default:
                mat = SphereBlue;
                break;
        }
        return mat;
    }

    //Funzione per dare alle pareti cilindriche del contouring il colore giusto
    private Material BorderColor(int choice)
    {
        Material mat = new Material(Shader.Find("Standard"));
        switch (choice)
        {
            case (1):
                mat = BlueBorder;
                break;
            case (2):
                mat = RedBorder;
                break;
            case (3):
                mat = YellowBorder;
                break;
            case (4):
                mat = GreenBorder;
                break;
            default:
                mat = SphereBlue;
                break;
        }
        return mat;
    }

    //Funzione per dare al cubo del chasing il colore giusto
    private Material ChasingColor(int choice)
    {
        Material mat = new Material(Shader.Find("Standard"));
        switch (choice)
        {
            case 1:
                mat = BlueChase;
                break;
            case 2:
                mat = YellowChase;
                break;
            case 3:
                mat = GreenChase;
                break;
        }
        return mat;
    }
    #endregion

    #region Funzioni per spawnare le pareti cilindriche del contouring di dimensione corretta
    //Funzione per spawnare la parete cilindrica interna del contouring corretta
    private GameObject InternalSpawner(float pathDimension)
    {
        GameObject border;
        switch (pathDimension.ToString())
        {
            case "0.03":
                border = Cylinder3cmInternal;
                break;
            case "0.05":
                border = Cylinder5cmInternal;
                break;
            case "0.06":
                border = Cylinder6cmInternal;
                break;
            case "0.08":
                border = Cylinder8cmInternal;
                break;
            default:
                Debug.Log("Stai sbagliando la generazione del bordo interno");
                border = null;
                break;
        }
        return border;
    }

    //Funzione per spawnare la parete cilindrica esterna del contouring corretta
    private GameObject ExternalSpawner(float pathDimension, float pathWidth)
    {
        GameObject border;

        switch (pathDimension.ToString())
        {

            //CASO GRANDEZZA S
            case "0.03":
                switch (pathWidth.ToString())
                {
                    case "1.4":
                        border = Cylinder_4_4_cmExternal;
                        break;
                    case "1.6":
                        border = Cylinder_4_6_cmExternal;
                        break;
                    case "1.8":
                        border = Cylinder_4_8_cmExternal;
                        break;
                    case "2":
                        border = Cylinder_5_cmExternal;
                        break;
                    case "2.2":
                        border = Cylinder_5_2_cmExternal;
                        break;
                    case "2.4":
                        border = Cylinder_5_4_cmExternal;
                        break;
                    case "2.6":
                        border = Cylinder_5_6_cmExternal;
                        break;
                    case "2.8":
                        border = Cylinder_5_8_cmExternal;
                        break;
                    case "3":
                        border = Cylinder_6_cmExternal;
                        break;
                    case "3.2":
                        border = Cylinder_6_2_cmExternal;
                        break;
                    case "3.4":
                        border = Cylinder_6_4_cmExternal;
                        break;
                    case "3.6":
                        border = Cylinder_6_6_cmExternal;
                        break;
                    case "3.8":
                        border = Cylinder_6_8_cmExternal;
                        break;
                    case "4":
                        border = Cylinder_7_cmExternal;
                        break;
                    case "4.2":
                        border = Cylinder_7_2_cmExternal;
                        break;
                    case "4.4":
                        border = Cylinder_7_4_cmExternal;
                        break;
                    case "4.6":
                        border = Cylinder_7_6_cmExternal;
                        break;
                    case "4.8":
                        border = Cylinder_7_8_cmExternal;
                        break;
                    case "5":
                        border = Cylinder_8_cmExternal;
                        break;
                    default:
                        border = null;
                        Debug.Log("Stai sbagliando la larghezza del percorso");
                        break;
                }
                break;

            //CASO GRANDEZZA M
            case "0.05":
                switch (pathWidth.ToString())
                {
                    case "1.4":
                        border = Cylinder_6_4_cmExternal;
                        break;
                    case "1.6":
                        border = Cylinder_6_6_cmExternal;
                        break;
                    case "1.8":
                        border = Cylinder_6_8_cmExternal;
                        break;
                    case "2":
                        border = Cylinder_7_cmExternal;
                        break;
                    case "2.2":
                        border = Cylinder_7_2_cmExternal;
                        break;
                    case "2.4":
                        border = Cylinder_7_4_cmExternal;
                        break;
                    case "2.6":
                        border = Cylinder_7_6_cmExternal;
                        break;
                    case "2.8":
                        border = Cylinder_7_8_cmExternal;
                        break;
                    case "3":
                        border = Cylinder_8_cmExternal;
                        break;
                    case "3.2":
                        border = Cylinder_8_2_cmExternal;
                        break;
                    case "3.4":
                        border = Cylinder_8_4_cmExternal;
                        break;
                    case "3.6":
                        border = Cylinder_8_6_cmExternal;
                        break;
                    case "3.8":
                        border = Cylinder_8_8_cmExternal;
                        break;
                    case "4":
                        border = Cylinder_9_cmExternal;
                        break;
                    case "4.2":
                        border = Cylinder_9_2_cmExternal;
                        break;
                    case "4.4":
                        border = Cylinder_9_4_cmExternal;
                        break;
                    case "4.6":
                        border = Cylinder_9_6_cmExternal;
                        break;
                    case "4.8":
                        border = Cylinder_9_8_cmExternal;
                        break;
                    case "5":
                        border = Cylinder_10_cmExternal;
                        break;
                    default:
                        border = null;
                        Debug.Log("Stai sbagliando la larghezza del percorso");
                        break;
                }
                break;

            //CASO GRANDEZZA L
            case "0.06":
                switch (pathWidth.ToString())
                {
                    case "1.4":
                        border = Cylinder_7_4_cmExternal;
                        break;
                    case "1.6":
                        border = Cylinder_7_6_cmExternal;
                        break;
                    case "1.8":
                        border = Cylinder_7_8_cmExternal;
                        break;
                    case "2":
                        border = Cylinder_8_cmExternal;
                        break;
                    case "2.2":
                        border = Cylinder_8_2_cmExternal;
                        break;
                    case "2.4":
                        border = Cylinder_8_4_cmExternal;
                        break;
                    case "2.6":
                        border = Cylinder_8_6_cmExternal;
                        break;
                    case "2.8":
                        border = Cylinder_8_8_cmExternal;
                        break;
                    case "3":
                        border = Cylinder_9_cmExternal;
                        break;
                    case "3.2":
                        border = Cylinder_9_2_cmExternal;
                        break;
                    case "3.4":
                        border = Cylinder_9_4_cmExternal;
                        break;
                    case "3.6":
                        border = Cylinder_9_6_cmExternal;
                        break;
                    case "3.8":
                        border = Cylinder_9_8_cmExternal;
                        break;
                    case "4":
                        border = Cylinder_10_cmExternal;
                        break;
                    case "4.2":
                        border = Cylinder_10_2_cmExternal;
                        break;
                    case "4.4":
                        border = Cylinder_10_4_cmExternal;
                        break;
                    case "4.6":
                        border = Cylinder_10_6_cmExternal;
                        break;
                    case "4.8":
                        border = Cylinder_10_8_cmExternal;
                        break;
                    case "5":
                        border = Cylinder_11_cmExternal;
                        break;
                    default:
                        border = null;
                        Debug.Log("Stai sbagliando la larghezza del percorso");
                        break;
                }
                break;

            //CASO GRANDEZZA XL
            case "0.08":
                switch (pathWidth.ToString())
                {
                    case "1.4":
                        border = Cylinder_9_4_cmExternal;
                        break;
                    case "1.6":
                        border = Cylinder_9_6_cmExternal;
                        break;
                    case "1.8":
                        border = Cylinder_9_8_cmExternal;
                        break;
                    case "2":
                        border = Cylinder_10_cmExternal;
                        break;
                    case "2.2":
                        border = Cylinder_10_2_cmExternal;
                        break;
                    case "2.4":
                        border = Cylinder_10_4_cmExternal;
                        break;
                    case "2.6":
                        border = Cylinder_10_6_cmExternal;
                        break;
                    case "2.8":
                        border = Cylinder_10_8_cmExternal;
                        break;
                    case "3":
                        border = Cylinder_11_cmExternal;
                        break;
                    case "3.2":
                        border = Cylinder_11_2_cmExternal;
                        break;
                    case "3.4":
                        border = Cylinder_11_4_cmExternal;
                        break;
                    case "3.6":
                        border = Cylinder_11_6_cmExternal;
                        break;
                    case "3.8":
                        border = Cylinder_11_8_cmExternal;
                        break;
                    case "4":
                        border = Cylinder_12_cmExternal;
                        break;
                    case "4.2":
                        border = Cylinder_12_2_cmExternal;
                        break;
                    case "4.4":
                        border = Cylinder_12_4_cmExternal;
                        break;
                    case "4.6":
                        border = Cylinder_12_6_cmExternal;
                        break;
                    case "4.8":
                        border = Cylinder_12_8_cmExternal;
                        break;
                    case "5":
                        border = Cylinder_13_cmExternal;
                        break;
                    default:
                        border = null;
                        Debug.Log("Stai sbagliando la larghezza del percorso");
                        break;
                }
                break;

            default:
                border = null;
                Debug.Log("Stai sbaglia nello spawn del bordo esterno");
                break;
        }
        return border;
    }
    #endregion

    private int excercise;                      //Flag per indicare che l'esercizio che si deve svolgere

    private int flag_Initialization = 0;        //Flag che indica che sono stati ricevuti i dati per l'inizializzazione della scena dell'esercizio

    private int TimeAllowed;                    //Variabile che indica il tempo a disposizione per l'esecuzione dell'esercizio
    private int RepetitionsRequired;            //Variabile che indica il numero di ripetizioni da effettuare durante l'esercizio

    private HapticSocket client;                //istanza della classe che contiene le socket per dialogare con il server
    private Thread clientThread;                //la thread che gestisce la ricezione dei dati dal server durante l'esercizio
    private Thread initializeClient;            //thread che riceve dati dal server per inizializzare l'esercizio


    //FUNZIONI

    //Funzione per spawnare il percorso del chasing della dimensione corretta
    private GameObject PathSpawner(int pathDimension)
    {
        GameObject border;
        switch(pathDimension){
            case 1:
                border = path4cm;
                radius = 0.04f;
                break;
            case 2:
                border = path6cm;
                radius = 0.06f;
                break;
            case 3:
                border = path8cm;
                radius = 0.08f;
                break;
            case 4:
                border = path10cm;
                radius = 0.10f;
                break;
            default:
                border = null;
                break;
        }
        return border;
    }

    //Funzione per gestire il riconoscimento vocale
    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    //Funzione eseguita allo spawn dello script. Si inzializza il gestore del riconoscimento vocale
    private void Awake()
    {
        //Si imposta il FrameRate a 60 fps
        Application.targetFrameRate = 60;

        //Si aggiungono le parole "go" e "close" al dizionario del riconoscimento
        //vocale e si impostano le funzioni chiamate qualora le due parole vengano
        //riconosciute
        keywords.Add("go", () =>
        {
            retryConnection = 1;
            excercise = -1;
            keywordRecognizer.Stop();
        });

        keywords.Add("close", () =>
        {
            keywordRecognizer.Stop();
            Application.Quit();
        });

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
    }

    //Funzione eseguita prima del primo frame
    void Start()
    {
        //received.text = null;

        //Inizializzazione degli elementi text
        connected.text = null;
        countDown.text = null;
        Search_Target = message_displayed;
        message_displayed.text = "Search For QR Code";

        //Istanziazione dei GameObject usati per la gestione dei dati provenienti dal MYO
        ElbowUp = Instantiate(ElbowUpPrefab) as Transform;
        ElbowUp.parent= DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        ElbowUp.localPosition = new Vector3(15.77138f * 0.16f, 0, 15.77138f * 0.25f);
        ElbowUp.localEulerAngles = new Vector3(90, 0, 0);
        ElbowUp.localScale = new Vector3(0.05f * 15.7713781f, 0.06f * 15.7713781f, 15.7713781f);
        ElbowUp.gameObject.SetActive(false);

        ElbowDown = Instantiate(ElbowDownPrefab) as Transform;
        ElbowDown.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        ElbowDown.localPosition = new Vector3(15.77138f * 0.16f, 0, 15.77138f * 0.25f);
        ElbowDown.localEulerAngles = new Vector3(90, 0, 180);
        ElbowDown.localScale = new Vector3(0.05f * 15.7713781f, 0.06f * 15.7713781f, 15.7713781f);
        ElbowDown.gameObject.SetActive(false);

        ElbowOk = Instantiate(ElbowOkPrefab) as Transform;
        ElbowOk.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        ElbowOk.localPosition = new Vector3(15.77138f * 0.16f, 0, 15.77138f * 0.25f);
        ElbowOk.localEulerAngles = new Vector3(90, 0, 0);
        ElbowOk.localScale = new Vector3(0.05f * 15.7713781f, 0.06f * 15.7713781f, 15.7713781f);
        ElbowOk.gameObject.SetActive(false);

        //Impostazione delle posizioni degli elementi text, i canvas contengono i text
        CountdownCanvas.transform.parent= DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        CountdownCanvas.transform.localPosition = new Vector3(0, 15.77138f * 0.05f, 15.77138f * 0.25f);
        CountdownCanvas.transform.localEulerAngles = new Vector3(45, 0, 0);
        CountdownCanvas.transform.localScale = new Vector3(2, 2, 2);

        ConnectedCanvas.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        ConnectedCanvas.transform.localPosition = new Vector3(-15.77138f * 0.16f, 15.77138f * 0.05f, 15.77138f * 0.38f);
        ConnectedCanvas.transform.localEulerAngles = new Vector3(45,-30, 0);
        ConnectedCanvas.transform.localScale = new Vector3(1, 1, 1);

        RepetitionsCanvas.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        RepetitionsCanvas.transform.localPosition = new Vector3(0, 15.77138f * 0.05f, 15.77138f * 0.44f);
        RepetitionsCanvas.transform.localEulerAngles = new Vector3(45, 0, 0);
        RepetitionsCanvas.transform.localScale = new Vector3(1, 1, 1);

        TimeRemainingCanvas.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        TimeRemainingCanvas.transform.localPosition = new Vector3(15.77138f * 0.16f, 15.77138f * 0.05f, 15.77138f * 0.38f);
        TimeRemainingCanvas.transform.localEulerAngles = new Vector3(45, 30, 0);
        TimeRemainingCanvas.transform.localScale = new Vector3(1, 1, 1);

        //Spawn della sfera che rappresenta l'endEffector
        myEndEffectorTrf = Instantiate(EndEffectorPrefabPlayer) as Transform;
        myEndEffectorTrf.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
        myEndEffectorTrf.localPosition = new Vector3(0, 0, 15.77138f * 0.10f);
        myEndEffectorTrf.localRotation = Quaternion.identity;
        myEndEffectorTrf.localScale = new Vector3(0.1577138f, 0.1577138f, 0.1577138f);
        //myEndEffectorTrf.gameObject.SetActive(false);

        //Inizializzo l'istanza della classe HapticSocket e preparo una thread a partire con la funzione che permette di ricevere i dati dal server
        client = new HapticSocket();
        clientThread = new Thread(client.Client); //il client va su una thread perchè "receive" blocca e mi bloccherebbe i frame se lo mettessi in update
        
        //Thread che inizializza la connessione, la scena e fa partire una clientThread
        initializeClient = new Thread(init);
        initializeClient.Start();
    }

    //Funzione chiamata ad ogni frame
    void Update()
    {
        //Questa riga serve ad impostare il FocusPoint dell'Hololens sull'Image Target per rendere più stabili gli ologrammi
        HolographicSettings.SetFocusPointForFrame(DefaultTrackableEventHandler.mTrackableBehaviour.transform.position);

        //Inizio di una serie di if-else per mostrare sull'elemento text "connect" le scritte giuste e per far partire il riconoscimento vocale se necessario
        //DefaultTrackableEventHandler.TargetFound è l'Image Target
        if (client.flag_connected == 1 && DefaultTrackableEventHandler.TargetFound==1)
        {
            connected.text = "Connected to Server";
        }else
        {
            if(client.flag_connected==0 && DefaultTrackableEventHandler.TargetFound == 1)
            {
                if (connection_attempts != 3) { 
                    connected.text = "Searching For Server...\n" + (connection_attempts+1).ToString() + " attempt/s";
                }
                if (connection_attempts == 3 && problemReported==0)
                {
                    keywordRecognizer.Start();
                    connected.text = "Say GO To Retry Or CLOSE To\n Exit Application";
                    problemReported = 1;
                }
                if (problemReported == 1)
                {
                    connected.text = "Say GO To Retry Or CLOSE To\n Exit Application";
                }
            }else
            {
                connected.text = null;
                if (connection_attempts == 3 && problemReported == 0)
                {
                    keywordRecognizer.Start();
                    problemReported = 1;
                }
            }
        }
        
        //Se ho ricevuto i dati per inizializzare l'esercizio si possono istanziare i GameObject di conseguenza
        if (flag_Initialization == 1)
        {
            flag_Initialization = 0;

            switch (excercise)
            {
                //REACHING
                case 1:
                    //istanziazione delle sfere nella posizione corretta
                    Sphere1 = Instantiate(ColliderSphere1Prefab);
                    Sphere2 = Instantiate(ColliderSphere2Prefab);

                    Sphere1.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    Sphere1.transform.localPosition = new Vector3(Sphere1Coordinates[0] * 15.7713781f, 0, Sphere1Coordinates[1] * 15.7713781f);
                    Sphere1.transform.localRotation = Quaternion.identity;
                    Sphere1.transform.localScale = new Vector3(diameter * 15.7713781f, diameter * 15.7713781f, diameter * 15.7713781f);

                    Sphere2.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    Sphere2.transform.localPosition = new Vector3(Sphere2Coordinates[0] * 15.7713781f, 0, Sphere2Coordinates[1] * 15.7713781f);
                    Sphere2.transform.localRotation = Quaternion.identity;
                    Sphere2.transform.localScale = new Vector3(diameter * 15.7713781f, diameter * 15.7713781f, diameter * 15.7713781f);

                    //impostazione dei colori delle sfere
                    Renderer material1 = Sphere1.GetComponent<Renderer>();
                    Renderer material2 = Sphere2.GetComponent<Renderer>();

                    material1.material = SphereColor(Sphere1Color);
                    material2.material = SphereColor(Sphere2Color);

                    //impostazione degli elementi text per vedere il tempo rimanente alla fine delle esercizio e le ripetizioni compiute
                    TimeRemainingCounter.text = TimeAllowed.ToString();
                    client.repetitions_count = 0;
                    RepetitionsCounter.text = client.repetitions_count.ToString(); ;
                    break;

                //CONTOURING
                case 2:
                    //istanziazione delle pareti cilindriche nella posizione corretta
                    internalBorder = Instantiate(InternalSpawner(pathDimension));
                    externalBorder = Instantiate(ExternalSpawner(pathDimension, pathWidth*100));

                    internalBorder.transform.parent= DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    internalBorder.transform.localPosition = new Vector3(x_path_center * 15.7713781f, 0, y_path_center * 15.7713781f);
                    internalBorder.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    internalBorder.transform.localScale = new Vector3(100 * 15.7713781f, 100 * 15.7713781f, 100 * 15.7713781f);

                    externalBorder.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    externalBorder.transform.localPosition = new Vector3(x_path_center * 15.7713781f, 0, y_path_center * 15.7713781f);
                    externalBorder.transform.localRotation = Quaternion.Euler(-90,0,0);
                    externalBorder.transform.localScale = new Vector3(100 * 15.7713781f, 100 * 15.7713781f, 100 * 15.7713781f);

                    //impostazione dei colori del colore delle pareti cilindriche
                    Renderer material3 = internalBorder.GetComponent<Renderer>();
                    Renderer material4 = externalBorder.GetComponent<Renderer>();

                    material3.material = BorderColor(pathColor);
                    material4.material = BorderColor(pathColor);

                    //impostazione degli elementi text per vedere il tempo rimanente alla fine delle esercizio e le ripetizioni compiute
                    TimeRemainingCounter.text = TimeAllowed.ToString();
                    client.repetitions_count = 0;
                    RepetitionsCounter.text = client.repetitions_count.ToString();
                    break;
                
                //CHASING
                case 3:
                    //istanziazione del percorso su cui si svolge l'esercizio e del cubo da inseguire
                    path_Chasing = Instantiate(PathSpawner(path_choice));
                    path_Chasing.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    path_Chasing.transform.localPosition = new Vector3(x_offset_chasing * 15.7713781f, 0, y_offset_chasing * 15.7713781f);
                    path_Chasing.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    path_Chasing.transform.localScale = new Vector3(100 * 15.7713781f, 100 * 15.7713781f, 10 * 15.7713781f);

                    cubeChase = Instantiate(CubeToChase);

                    cubeChase.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    cubeChase.transform.localPosition = new Vector3((x_offset_chasing + radius * Mathf.Sin(-0.155f)) * 15.7713781f, 0, (y_offset_chasing + radius * Mathf.Cos(-0.155f)) * 15.7713781f);
                    cubeChase.transform.localRotation = Quaternion.identity;
                    cubeChase.transform.localScale = new Vector3(0.008f * 15.7713781f, 0.008f * 15.7713781f, 0.008f * 15.7713781f);

                    //impostazione dei colori del colore del cubo
                    Renderer material5 = cubeChase.GetComponent<Renderer>();
                    material5.material = ChasingColor(chaseColor);

                    //istanziazione della freccia
                    arrow_chase = Instantiate(ArrowChasing);
                    arrow_chase.transform.parent=DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    arrow_chase.SetActive(false);

                    arrowMaterial = arrow_chase.transform.GetChild(0).GetComponent<Renderer>();

                    //viene salvato il valore del raggio del percorso con cui si inizia l'esercizio
                    old_radius = client.radius_circle_chasing;

                    //impostazione degll'elemento text per vedere il tempo rimanente alla fine delle esercizio
                    TimeRemainingCounter.text = TimeAllowed.ToString();

                    break;
            }
            
            //Inizia la Coroutine per il countdown prima dell'esercizio
            StartCoroutine(InitCountdown());

        }
        
        /* Se voglio vedere i dati ricevuti sottoforma di una scritta
        if (client.flag_receive == 1)
        {
            received.text = client.message_received;
        }
        */

        //Se la thread clientThread ha ricevuto un dato si sposta l'avatar dell'organo terminale
        if (client.flag_receive == 1)
        {
            //istanziazione della scia dell'avatar dell'organo terminale e spostamento di quest'ultimo nella posizione
            //ricevuta
            myPathTrf = Instantiate(EndEffectorPrefabPath) as Transform;

            myPathTrf.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
            myPathTrf.localPosition = new Vector3(client.coordinateX_Z[0] * 15.7713781f, 0f, client.coordinateX_Z[1] * 15.7713781f);
            myPathTrf.localRotation = Quaternion.identity;
            myPathTrf.localScale = new Vector3(7f, 1f, 7f);

            //myPathTrf.gameObject.SetActive(true);

            myEndEffectorTrf.gameObject.SetActive(true);
            myEndEffectorTrf.localPosition = new Vector3(client.coordinateX_Z[0] * 15.7713781f, 0f, client.coordinateX_Z[1] * 15.7713781f);

            //Viene aggiornato il numero delle ripetizioni effettuate
            RepetitionsCounter.text = client.repetitions_count.ToString();

            //Se l'esercizio è il CHASING si deve verificare se la dimensione della circonferenza è cambiata ed in quel caso
            //Cambiare il GameObject del percorso. Si deve inoltre spostare il cubo da inseguire
            if (excercise == 3)
            {
                RepetitionsCounter.text = "--";
                //viene salvato il valore del raggio del percorso appena ricevuto
                new_radius = client.radius_circle_chasing;
                //se il raggio nuovo è diverso da quello vecchio, si distrugge il vecchio percorso e se ne istanzia uno nuovo
                if (new_radius != old_radius)
                {
                    Destroy(path_Chasing);
                    path_Chasing = Instantiate(PathSpawner(new_radius));
                    path_Chasing.transform.parent = DefaultTrackableEventHandler.mTrackableBehaviour.transform;
                    path_Chasing.transform.localPosition = new Vector3(x_offset_chasing * 15.7713781f, 0, y_offset_chasing * 15.7713781f);
                    path_Chasing.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    path_Chasing.transform.localScale = new Vector3(100 * 15.7713781f, 100 * 15.7713781f, 10 * 15.7713781f);
                    old_radius = new_radius;           
                }
                //Si sposta il cubo da inseguire secondo i dati ricevuti
                cubeChase.transform.localPosition = new Vector3(client.coordinateX_Z_cubo[0] * 15.7713781f, 0f, client.coordinateX_Z_cubo[1] * 15.7713781f);
                cubeChase.transform.localEulerAngles = new Vector3(0, -Mathf.Rad2Deg * Mathf.Atan2((client.coordinateX_Z_cubo[1] - y_offset_chasing), (client.coordinateX_Z_cubo[0] - x_offset_chasing)), 0);
                MoveArrow();
            }

            //Si valuta quale informazione visualizzare riguardo all'orientazione dell'avambraccio
            switch (client.elbowPose)
            {
                case "up":
                    ElbowUp.gameObject.SetActive(true);
                    ElbowDown.gameObject.SetActive(false);
                    ElbowOk.gameObject.SetActive(false);
                    break;
                case "down":
                    ElbowUp.gameObject.SetActive(false);
                    ElbowDown.gameObject.SetActive(true);
                    ElbowOk.gameObject.SetActive(false);
                    break;
                case "ok":
                    ElbowUp.gameObject.SetActive(false);
                    ElbowDown.gameObject.SetActive(false);
                    ElbowOk.gameObject.SetActive(true);
                    break;
            }
            client.flag_receive = 0;
        }

        //Se l'esercizio è finito o non si vuole cercare di riconettersi al server dopo i 3 tentativi effettuati
        //Si eliminano alcuni gameobject presenti nella scena e si fa ripartire il processo di inizializzazione
        if (client.flag_excercise_ended == 1 || retryConnection==1)
        {
            switch (excercise)
            {
                //REACHING
                case 1:
                    Destroy(Sphere1);
                    Destroy(Sphere2);
                    if (client.repetitions_count == RepetitionsRequired)
                    {
                        StopCoroutine(ExcerciseTimeCoroutine);
                        StartCoroutine(StopExcercise());
                    }
                    break;
                //CONTOURING
                case 2:
                    Destroy(internalBorder);
                    Destroy(externalBorder);
                    if (client.repetitions_count == RepetitionsRequired)
                    {
                        StopCoroutine(ExcerciseTimeCoroutine);
                        StartCoroutine(StopExcercise());
                    }
                    break;
                //CHASING
                case 3:
                    Destroy(path_Chasing);
                    Destroy(cubeChase);
                    Destroy(arrow_chase);
                    break;
            }

            //attendo la terminazione della thread initializeClient
            initializeClient.Join();
            //excercise è uguale a -1 solo se la connessione al server è fallita. In quel caso non è partita la clientThread
            if (excercise != -1)
            {
                clientThread.Join();
            }

            retryConnection = 0;
            client.flag_excercise_ended = 0;

            //Si fa ripartire la thread per inizializzare la scena dell'esercizio
            initializeClient = new Thread(init);
            initializeClient.Start();       
        }
    }

    //Funzione posta in una thread per ricvere i dati per inizializzare la scena dell'esercizio
    public void init()
    {
        connection_attempts = 0;
        problemReported = 0;

        //Ciclo per provare a connettersi al server per al massimo 3 volte
        while (client.flag_connected == 0  && connection_attempts!=3)
        {
            client.ConnectToServer();
            if (client.flag_connected == 0)
            {
                connection_attempts++;
            }
        }

        //Se la connessione è riuscita si attende un messaggio che riporta i parametri per inizializzare la scena
        if (connection_attempts != 3)
        {
            //Si riceve il messaggio di inizializzazione
            string[] initParameters = client.ClientInitializeScene();

            //A seconda dell'esercizio che si vuole svolgere si impostano diversi parametri
            excercise = int.Parse(initParameters[0], NumberStyles.Any, CultureInfo.InvariantCulture);
            switch (excercise)
            {
                //REACHING
                case 1:

                    Sphere1Coordinates[0] = float.Parse(initParameters[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(Sphere1Coordinates[0]);

                    Sphere1Coordinates[1] = float.Parse(initParameters[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(Sphere1Coordinates[1]);

                    Sphere2Coordinates[0] = float.Parse(initParameters[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(Sphere2Coordinates[0]);

                    Sphere2Coordinates[1] = float.Parse(initParameters[4], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(Sphere2Coordinates[1]);

                    diameter = float.Parse(initParameters[5], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(diameter);

                    Sphere1Color = int.Parse(initParameters[6], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(Sphere1Color);

                    Sphere2Color = int.Parse(initParameters[7], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(Sphere2Color);

                    TimeAllowed = int.Parse(initParameters[8], NumberStyles.Any, CultureInfo.InvariantCulture);

                    RepetitionsRequired = int.Parse(initParameters[9], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;

                //CONTOURING
                case 2:

                    x_path_center = float.Parse(initParameters[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(x_path_center);

                    y_path_center = float.Parse(initParameters[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(y_path_center);

                    pathDimension = float.Parse(initParameters[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(pathDimension);

                    pathWidth = float.Parse(initParameters[4], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(pathWidth);

                    pathColor = int.Parse(initParameters[5], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(pathColor);

                    TimeAllowed = int.Parse(initParameters[6], NumberStyles.Any, CultureInfo.InvariantCulture);

                    RepetitionsRequired = int.Parse(initParameters[7], NumberStyles.Any, CultureInfo.InvariantCulture);
                    break;
                
                //CHASING
                case 3:

                    x_offset_chasing = float.Parse(initParameters[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(x_offset_chasing);

                    y_offset_chasing = float.Parse(initParameters[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    Debug.Log(y_offset_chasing);

                    path_choice = int.Parse(initParameters[3], NumberStyles.Any, CultureInfo.InvariantCulture);

                    chaseColor = int.Parse(initParameters[4], NumberStyles.Any, CultureInfo.InvariantCulture);

                    TimeAllowed = int.Parse(initParameters[5], NumberStyles.Any, CultureInfo.InvariantCulture);

                    break;
            }

            //si segnala che l'inizializzazione è possibile
            flag_Initialization = 1;

            //clientThread = new Thread(client.Client); //il client va su una thread perchè "receive" blocca e mi bloccherebbe i frame se lo mettessi in update
            //Si fa partire la thread adatta all'esercizio
            if (excercise != 3)
            {
                clientThread = new Thread(client.Client);
                clientThread.Start();
            }
            else
            {
                clientThread = new Thread(client.Client_Chasing);
                clientThread.Start();
            }
        }
    }

    //Quando l'applicazione termina si chiama una funzione per chiudere le socket attive
    void OnApplicationQuit()
    {
        client.CloseSocket();
    }

    //Funzione introdotta in un Coroutine per il countdown iniziale
    IEnumerator InitCountdown()
    {
        arrow_flag = 0;
        int count = 5;
        countDown.fontSize = 240;
        while (count > 0)
        {
            countDown.text = "INITIALIZING";
            count--;
            yield return new WaitForSeconds(1);
        }

        count = 3;

        while (count >= 0)
        {
            if (count != 0)
            {
                countDown.text = count.ToString();
            }else
            {
                countDown.text = "START!";
            }
            count--;
            yield return new WaitForSeconds(1);
        }
        countDown.text = null;
        arrow_flag = 1;
        ExcerciseTimeCoroutine= StartCoroutine(ExcerciseCountdown());
        yield break;
    }

    //Funzione introdotta in una coroutine per il countdown dell'esercizio
    IEnumerator ExcerciseCountdown()
    {
        int count = TimeAllowed;
        while(count >= 1)
        {
            TimeRemainingCounter.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1);
        }
        TimeRemainingCounter.text = "0";
        StartCoroutine(StopExcercise());        
        yield break;
    }

    //Funzione introdotta in una Coroutine che alla fine dell'esercizio mostra la scritta "STOP"
    IEnumerator StopExcercise()
    {
        int count=0;
        countDown.text = "STOP";
        while (count < 5)
        {
            count++;
            yield return new WaitForSeconds(0.3f);
        }
        countDown.text = null;
        yield break;
    }

    //Funzione per muovere ed orientare la freccia
    private void MoveArrow()
    {
        distance = Vector3.Distance(myEndEffectorTrf.transform.position, cubeChase.transform.position);
        //Se l'organo terminale è distante più di 1 cm dal cubo allora si visualizza la freccia altrimenti no
        if (Mathf.Abs(distance)>0.01 && arrow_flag == 1)
        {
            arrow_chase.SetActive(true);
            arrow_chase.transform.localPosition= new Vector3(myEndEffectorTrf.transform.localPosition.x, myEndEffectorTrf.transform.localPosition.y, myEndEffectorTrf.transform.localPosition.z);
            arrow_angle = (float)Mathf.Atan2((cubeChase.transform.localPosition.z - myEndEffectorTrf.transform.localPosition.z), (cubeChase.transform.localPosition.x - myEndEffectorTrf.transform.localPosition.x));
            arrow_chase.transform.localEulerAngles = new Vector3(0, -Mathf.Rad2Deg * arrow_angle, 0);
            arrow_chase.transform.localScale = new Vector3((distance / 0.0331f) * 15.7713781f, 15.7713781f, (distance / 0.0331f) * 15.7713781f);

            //Sezione di codice che serve a regolare il colore della freccia in modo che vari da verde a rosso in modo graduale
            if (distance>0.01f && distance < 0.04f)
            {
                arrowMaterial.material.color = new Color(((255 * (distance - 0.01f)) / 0.03f) / 255f, 1, 0);
            }else
            {
                if (distance < 0.06f)
                {
                    arrowMaterial.material.color = new Color(1,(255f-(255 * (distance - 0.04f)) / 0.02f) / 255f, 0);
                }
            }
        }else
        {
            arrow_chase.SetActive(false);
        }           
    } 
   
}