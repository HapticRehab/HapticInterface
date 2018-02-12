using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadCorrectScene : MonoBehaviour {

    //Variabile per l'identificazione degli errori di comunicazione con il server
    //Se se ne verifica uno allora bisogna fermare il conto alla rovescia
    public static int ErrorDetected = 0;

    //Coroutine per muovere il cubo dell'esercizio di chasing
    private IEnumerator MoveTheCube;

    //Corotuine per effetturare il conto alla rovescia dell'inizializzazione
    private IEnumerator InitCoroutine;

    //Variabile per effetturare la gestione degli errori di connessione una sola volta
    private int ErrorHandled = 0;

    //elementi Text usati per il countdown di inizio esercizio e
    //per visualizzare quanto tempo manca alla fine dell'attività 
    public Text countdown;
    public Text initialCountdown;

    //elemento text per vedere se gli Hololens sono connessi
    private Text HololensInfo;

    //GameObject per vedere quante ripetizioni si sono effettuate.
    //Si usa un GameObject per controllare un semplice text perchè è necessario
    //accedere alla proprietà "SetActive".
    public GameObject repetitions;

    //Bottone per tornare all'impostazione degli esercizi, viene abilitato
    //quando finisce un esercizio o al verificarsi di un problema di connessione
    public Button BackButton;

    //Componente che permette di riprodurre suoni
    public AudioSource audioPlayer;

    //Clip audio utilizzate
    public AudioClip startClip;
    public AudioClip countClip;
    public AudioClip stopClip;

    //////////////////////////////////////
    //VARIABILI PER L'ESERCIZIO DI REACHING
    //////////////////////////////////////

    //Modello della sfera con cui collidere
    public GameObject SpherePrefab;
    public GameObject SpherePrefab2;

    //collider per inviare i dati di posizione e per il conteggio. Questi collider
    //vengono abilitati solo dopo che c'è stata l'inizializzazione della posizone del robot
    private Collider collider_sphere1;
    private Collider collider_sphere2;
    private Collider collider_count1;
    private Collider collider_count2;

    //gameobject Trigger per il conteggio
    public GameObject trigger1;
    public GameObject trigger2;

    //Materiali per il colore delle sfere
    public Material SphereRed;
    public Material SphereBlue;
    public Material SphereGreen;
    public Material SphereYellow;

    //Renderer per assegnare i materiali alle sfere
    private Renderer material1;
    private Renderer material2;

    //Orientazione delle sfere(1=orizzontale,2=verticale)
    private int orientation;    
    
    //Posizione da cui si parte per disporre le sfere
    private Vector3 initialPositionHorizontal = new Vector3(0f, 0.3f, 0.25f);
   
    //Variabili usate da altri script ed aclcune anche per i diversi esercizi(NumOfReps,TimeAllowed,ForceFeed,audioOn)
    public static int NumOfReps;
    public static int TimeAllowed;
    public static int ForceFeed;
    public static Vector3[] spawn;
    public static float diameter;
    public static int TimeIsFinished = 0;
    //Variabile per far partire l'audio(1 SI/0 NO)
    private int audioOn;

    /////////////////////////////////////////
    //VARIABILI PER L'ESERCIZIO DI CONTOURING
    /////////////////////////////////////////

    //Tutti i prefab delle le pareti cilindriche
    //interne del percorso da compiere
    public GameObject Cylinder3cmInternal;
    public GameObject Cylinder5cmInternal;
    public GameObject Cylinder6cmInternal;
    public GameObject Cylinder8cmInternal;
    //Tutti i prefab delle le pareti cilindriche
    //esterne del percorso da compiere
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

    //trigger che permette di inviare dati al server durante l'esrcizio di contouring
    public GameObject triggerContouring;
    private Collider contouringCollider;

    //Dimensioni del percorso(S,M,L,XL)
    public static float pathDimension;

    //Larghezza del percorso
    public static float pathWidth;

    //Materiali per i colori delle pareti cilindriche
    public Material BlueBorder;
    public Material RedBorder;
    public Material GreenBorder;
    public Material YellowBorder;

    //trigger che permettono di vedere quanti giri del percorso
    //sono stati fatti
    public GameObject PathCountTrigger1;
    public GameObject PathCountTrigger2;
    public GameObject PathCountTrigger3;
    public GameObject PathCountTrigger4;
    private Collider PathCountCollider1;
    private Collider PathCountCollider2;
    private Collider PathCountCollider3;
    private Collider PathCountCollider4;

    /////////////////////////////////////////
    //VARIABILI PER L'ESERCIZIO DI CHASING///
    ////////////////////////////////////////

    //materiali scegliere il colore del cubo da inseguire
    public Material BlueChase;
    public Material GreenChase;
    public Material YellowChase;

    //gameobject contenenti i prefab del percorso su cui effettuare
    //l'inseguimento
    public GameObject Cylinder4cmPath;
    public GameObject Cylinder6cmPath;
    public GameObject Cylinder8cmPath;
    public GameObject Cylinder10cmPath;

    //Gameobject usato per gestire il percorso
    private GameObject path_Chasing;

    //gameobject del cubo da inseguire
    public GameObject CubeToChase;
    private GameObject cube;

    //trigger usati per inviare i dati di posizione
    public GameObject TriggerChasing;
    private Collider ColliderChasing;

    //Variabile per segnalare che l'esrcizio è in esecuzione
    private int excerciseIsPlaying=1;

    //Coroutine per tenere traccia del tempo che rimane alla fine dell'esercizio
    private Coroutine timeCounterCoroutine;

    //componenti dell'interfaccia grafica usate nell'esercizio di chasing
    public GameObject ChaseDimensionLabel;
    public Dropdown ChaseDimensionDropdown;
    public GameObject ChaseSpeedIndicatorLabel;
    public GameObject ChaseSpeedLabel;
    public Slider ChaseSpeedSlider;
    public GameObject Canvas;

    //Dropdown e slider usati per controllare l'UI nell'esercizio di chasing
    private Dropdown dp1;    
    private Slider slide;   

    //variabili per modificare la grandezza e il periodo del chase
    private double radius, period;
    //sampling time rappresenta la distanza temporale tra due campioni del riferimento
    //seguito dal cubo nel suo movimento
    private double sampling_time = 0.001f;




    //////////////////////////////////////////////////////
    //Funzioni per ricavare i valori per creare la scena//
    //////////////////////////////////////////////////////

    //Funzione per ricavare il diametro del cerchio interno che costituisce il percorso da seguire.
    //il diametro è relativo al bordo esterno del cerchio che uso per disegnare il percorso
    private float PathDim()
    {
        float dim;

        switch (SaveStateAndLoadScene._pathDim)
        {
            case 1:
                dim = 0.03f;
                break;
            case 2:
                dim = 0.05f;
                break;
            case 3:
                dim = 0.06f;
                break;
            case 4:
                dim = 0.08f;
                break;
            default:
                dim = 0;
                Debug.Log("Wrong Path Width");
                break;
        }

        return dim;
    }

    //Funzione per spawnare la parete cilindrica interna della dimensione corretta nel contouring
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

    //Funzione per spawnare la parete cilindrica esterna della dimensione corretta nel contouring
    private GameObject ExternalSpawner(float pathDimension,float pathWidth)
    {
        GameObject border;
        
        switch (pathDimension.ToString()) {

            //CASO GRANDEZZA S
            case "0.03":
                switch (pathWidth.ToString())
                {
                    case "1.4":
                        border= Cylinder_4_4_cmExternal;
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
                        break;
                }
                break;

            default:
                border = null;
                break;
        }
        return border;
    }

    //Funzione per spawnare il percorso che si deve seguire nel CHASING
    private GameObject PathSpawner(int pathDimension)
    {
        GameObject border;
        switch (pathDimension)
        {
            case 1:
                border = Cylinder4cmPath;
                radius = 0.04f;
                break;
            case 2:
                border = Cylinder6cmPath;
                radius = 0.06f;
                break;
            case 3:
                border = Cylinder8cmPath;
                radius = 0.08f;
                break;
            case 4:
                border = Cylinder10cmPath;
                radius = 0.10f;
                break;
            default:
                Debug.Log("Stai sbagliando la generazione del percorso del chase");
                border = null;
                break;
        }
        return border;
    }

    //Funzione per Ricavare il Diametro delle Sfere nel REACHING
    private float Diameter()
    {
        float diam;

        switch (SaveStateAndLoadScene._sphDim) //i possibili diametri in metri
        {
            case 1:
                diam = 0.01f;
                break;
            case 2:
                diam = 0.015f;
                break;
            case 3:
                diam = 0.02f;
                break;
            case 4:
                diam = 0.025f;
                break;
            case 5:
                diam = 0.03f;
                break;
            case 6:
                diam = 0.035f;
                break;
            case 7:
                diam = 0.04f;
                break;
            case 8:
                diam = 0.045f;
                break;
            case 9:
                diam = 0.05f;
                break;
            case 10:
                diam = 0.055f;
                break;
            case 11:
                diam = 0.06f;
                break;

            default:
                diam = 0;
                Debug.Log("WRONG DIAMETER");
                break;
        }
        return diam;
    }

    //Funzione per impostare le posizioni corrette dello spawn delle sfere. La sfera 1 sta nella prima
    //posizione dell'array (prendo come centro il punto (0;0.21))
    private Vector3[] SpawnSpheres()
    {
        Vector3[] position=new Vector3[2]; //Contiene le posizioni

        if (orientation == 1)
        {
            position[0] = initialPositionHorizontal + new Vector3(-(SaveStateAndLoadScene._pathLen / 200) - (diameter / 2), 0f, 0f);
            position[1] = initialPositionHorizontal + new Vector3((SaveStateAndLoadScene._pathLen / 200) + (diameter / 2), 0f, 0f);
        }
        else
        {
            position[0] = initialPositionHorizontal + new Vector3(0f, 0f, -(SaveStateAndLoadScene._pathLen / 200) - (diameter / 2));
            position[1] = initialPositionHorizontal + new Vector3(0f, 0f, (SaveStateAndLoadScene._pathLen / 200) + (diameter / 2));
        }
        return position;
    }

    //Funzione per dare il colore giusto alle sfere
    private Material SphereColor(int choice)
    {
        Material mat= new Material(Shader.Find("Standard"));
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
        }
        return mat;
    }


    //Funzione per dare il colore giusto alle pareti cilindriche del conotouring
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
        }
        return mat;
    }

    //Funzione per dare il colore giusto al cubo per il Chasing
    private Material ChasingColor(int choice)
    {
        Material mat = new Material(Shader.Find("Standard"));
        switch (choice)
        {
            case (1):
                mat = BlueChase;
                break;
            case (2):
                mat = YellowChase;
                break;
            case (3):
                mat = GreenChase;
                break;
        }
        return mat;
    }

    //Funzione chiamata all'istanziazione della scena
    void Awake () {

        TimeIsFinished = 0;

        //A seconda dell'esercizio si spawnano diversi elementi
        switch (SaveStateAndLoadScene._excercise)
        {
            case 1: //REACHING

                //Si generano le due sfere da toccare e le si da il colore corretto
                GameObject Sphere1 = Instantiate(SpherePrefab);
                GameObject Sphere2 = Instantiate(SpherePrefab2);
                material1 = Sphere1.GetComponent<Renderer>();
                material2 = Sphere2.GetComponent<Renderer>();

                collider_sphere1 = Sphere1.GetComponent<Collider>();
                collider_sphere2 = Sphere2.GetComponent<Collider>();

                //Si disabilitano i collider finchè non è iniziato l'esercizio
                collider_sphere1.enabled = false;
                collider_sphere2.enabled = false;

                //Si imposta la dimensione delle sfere
                diameter = Diameter();
                Sphere1.transform.localScale = new Vector3(diameter, diameter, diameter);
                Sphere2.transform.localScale = new Vector3(diameter, diameter, diameter);

                //Si imposta il numero di Ripetizioni
                NumOfReps = SaveStateAndLoadScene._numRep;

                //Si imposta l'orientazione delle sfere(Verticale/Orizzontale)
                orientation = SaveStateAndLoadScene._sphOri;

                //Si posizionano le sfere nel punto giusto ed i trigger
                spawn = SpawnSpheres();
                Sphere1.transform.position = spawn[0];
                Sphere2.transform.position = spawn[1];

                //trigger per il conteggio di quante volte si toccano le sfere
                GameObject trig1 = Instantiate(trigger1, Sphere1.transform.position, Quaternion.identity);
                GameObject trig2 = Instantiate(trigger2, Sphere2.transform.position, Quaternion.identity);

                trig1.transform.localScale = new Vector3 (diameter,diameter,diameter);
                trig2.transform.localScale = new Vector3(diameter, diameter, diameter);

                collider_count1 = trig1.GetComponent<Collider>();
                collider_count2 = trig2.GetComponent<Collider>();

                collider_count1.enabled = false;
                collider_count2.enabled = false;

                //Tempo Concesso
                TimeAllowed = SaveStateAndLoadScene._timAll;
                countdown.text = "Time Remaining: "+ TimeAllowed.ToString();

                //Impostazione del colore delle sfere
                material1.material = SphereColor(SaveStateAndLoadScene._sph1);
                material2.material= SphereColor(SaveStateAndLoadScene._sph2);

                //Force Feedback Richiesto
                ForceFeed = SaveStateAndLoadScene._forFeed;

                //Audio
                audioOn = SaveStateAndLoadScene._audio;

                break;

            case 2: //CONTOURING

                //Si ricava la dimensione del percorso
                pathDimension = PathDim();

                //Si ricava la larghezza del percorso
                pathWidth = SaveStateAndLoadScene._pathWid;


                //Si spawnanoo i bordi del percorso, i trigger per gestire il contatto e gli si da il colore giusto
                GameObject internalBorder = Instantiate(InternalSpawner(pathDimension));
                GameObject externalBorder = Instantiate(ExternalSpawner(pathDimension,pathWidth));

                GameObject path_c1= Instantiate(PathCountTrigger1);
                GameObject path_c2 = Instantiate(PathCountTrigger2);
                GameObject path_c3 = Instantiate(PathCountTrigger3);
                GameObject path_c4 = Instantiate(PathCountTrigger4);

                PathCountCollider1 = path_c1.GetComponent<Collider>();
                PathCountCollider2 = path_c2.GetComponent<Collider>();
                PathCountCollider3 = path_c3.GetComponent<Collider>();
                PathCountCollider4 = path_c4.GetComponent<Collider>();

                PathCountCollider1.enabled = false;
                PathCountCollider2.enabled = false;
                PathCountCollider3.enabled = false;
                PathCountCollider4.enabled = false;

                material1 = internalBorder.GetComponent<Renderer>();
                material2 = externalBorder.GetComponent<Renderer>();

                //Si posizionano le pareti cilindriche nella posizione corretta
                internalBorder.transform.position = initialPositionHorizontal;
                externalBorder.transform.position = initialPositionHorizontal;

                material1.material = BorderColor(SaveStateAndLoadScene._pathCol);
                material2.material = BorderColor(SaveStateAndLoadScene._pathCol);

                GameObject trigger = Instantiate(triggerContouring, initialPositionHorizontal, Quaternion.identity);

                contouringCollider = trigger.GetComponent<Collider>();

                //Si disabilita il collider che triggera l'invio di dati al server
                contouringCollider.enabled = false;

                //Numero di Ripetizioni
                NumOfReps = SaveStateAndLoadScene._numRep_Cont;

                //Orientazione Percorso(Verticale/Orizzontale)
                orientation = SaveStateAndLoadScene._pathOri;

                //Tempo Concesso
                TimeAllowed = SaveStateAndLoadScene._timAll_Cont;
                countdown.text = "Time Remaining: " + TimeAllowed.ToString();

                //Force Feedback Richiesto
                ForceFeed = SaveStateAndLoadScene._forFeed_Cont;

                //Audio
                audioOn = SaveStateAndLoadScene._audio_Cont;
                break;

            case 3: //CHASING

                //Si instanzia il percorso che compie l'inseguimento
                path_Chasing = Instantiate(PathSpawner(SaveStateAndLoadScene._chaseWid));

                //Si instanzia il cubo da inseguire e gli si da il colore giusto
                cube = Instantiate(CubeToChase);
                cube.transform.position = new Vector3((float)(radius * Math.Sin(-0.155)), 0.3f, (float)(radius * Math.Cos(-0.155))+0.25f);

                material1 = cube.GetComponent<Renderer>();
                material1.material = ChasingColor(SaveStateAndLoadScene._chaseCol);

                //Si instanzia il trigger per inviare i dati
                GameObject chase_trig = Instantiate(TriggerChasing, initialPositionHorizontal, Quaternion.identity);

                ColliderChasing = chase_trig.GetComponent<Collider>();
                //Si disabilita il collider che triggera l'invio di dati al server
                ColliderChasing.enabled = false;

                //In questo esercizio non serve il numero di ripetizioni, si basa tutto sul tempo di esecuzione
                NumOfReps = -1;
                //Si disabilita il text per visualizzare il numero di ripetizioni effettuate
                repetitions.SetActive(false);

                //Tempo concesso
                TimeAllowed = SaveStateAndLoadScene._timAll_Chase;
                countdown.text = "Time Remaining: " + TimeAllowed.ToString();

                //Force Feedback Richiesto
                ForceFeed = SaveStateAndLoadScene._forFeed_Chase;

                //audio
                audioOn = SaveStateAndLoadScene._audio_Chase;

                //istanziazione dei componenti di UI per regolare la velocità e l'ampiezza del percorso
                GameObject lbl1 =Instantiate(ChaseDimensionLabel);
                dp1 =Instantiate(ChaseDimensionDropdown);
                GameObject lbl2=Instantiate(ChaseSpeedLabel);
                slide=Instantiate(ChaseSpeedSlider);
                GameObject indicLbl=Instantiate(ChaseSpeedIndicatorLabel);

                lbl1.transform.parent = Canvas.transform;
                dp1.transform.parent = Canvas.transform;
                lbl2.transform.parent = Canvas.transform;
                slide.transform.parent = Canvas.transform;
                indicLbl.transform.parent = Canvas.transform;

                //si inizializza il valore del dropdown del chasing in modo che sia coerente con le impostazioni effettuate nella scena precedente e 
                //da codice si imposta la funzione che viene eseguita quando si cambia il suo valore
                dp1.value = SaveStateAndLoadScene._chaseWid - 1;
                dp1.onValueChanged.AddListener(delegate { ChangePath();});

                //si inizializza il valore dello slider del chasing in modo che sia coerente con le impostazioni effettuate nella scena precedente e 
                //da codice si imposta la funzione che viene eseguita quando si cambia il suo valore
                slide.value = (SaveStateAndLoadScene._chasePeriod - 4) / 0.2f;
                slide.onValueChanged.AddListener(delegate { ChangePeriod(); });
                period = SaveStateAndLoadScene._chasePeriod;

                break;
            default:

                break;

        }
	}


    //Si fa partire la Coroutine per fare il conto alla rovescia iniziale(3...2...1...start)
    void Start()
    {
        GameObject HololensPanel = GameObject.Find("HololensInfo");
        HololensPanel.SetActive(true);
        InitCoroutine = InitCountdown();
        StartCoroutine(InitCoroutine);
    }

    //Quando sono finite le ripetizioni da compiere si ferma il conto alla rovescia per il tempo rimanente
    //e si fa partire la Coroutine per il suono di STOP
    void Update()
    {
        if (SaveStateAndLoadScene._excercise != 3) {  
            if(SocketConnection.repetitions == NumOfReps && excerciseIsPlaying==1|| SocketConnectionContouring.repetitions == NumOfReps && excerciseIsPlaying == 1)        //excerciseIsPlaying serve a chiamare la couroutine del stopSound solo una volta altrimenti la chiama ad ogni frame
            {
                excerciseIsPlaying = 0;
                StopCoroutine(timeCounterCoroutine);
                StartCoroutine(StopSoundOut(stopClip));
            }
        }
        if (ErrorDetected == 1 && ErrorHandled == 0)
        {
            ErrorHandled = 1;
            excerciseIsPlaying = 0;
            StopCoroutine(timeCounterCoroutine);
            StopCoroutine(InitCoroutine);
            if (SaveStateAndLoadScene._excercise == 3)
            {
                StopCoroutine(MoveTheCube);
            }
        }
    }


    //Corotuine per il suono e scritta di STOP
    IEnumerator StopSoundOut(AudioClip sound)
    {
        int soundPlayed = 0;
        initialCountdown.text = "STOP!";

        if (SaveStateAndLoadScene._excercise == 3)
        {
            dp1.interactable = false;
            slide.interactable = false;
        }

        while (soundPlayed < 5)
        {
            soundPlayed++;
            if (audioOn == 1) { 
                audioPlayer.PlayOneShot(sound);
            }
            yield return new WaitForSeconds(0.3f);
        }
        initialCountdown.text = null;
        yield break;
    }

    //Coroutine per il countdown iniziale(3...2...1...start)
    IEnumerator InitCountdown()
    {
        int count = 5;
        while (count > 0)
        {
            initialCountdown.fontSize = 100;
            initialCountdown.text = "INITIALIZING";
            count--;
            yield return new WaitForSeconds(1);
        }

        count = 3;

        initialCountdown.fontSize = 300;
        while (count >=0)
        {
            if(count != 0) { 
                initialCountdown.text = count.ToString();
                if (audioOn == 1)
                {
                    audioPlayer.PlayOneShot(countClip);
                }
            }
            else
            {
                initialCountdown.text = "START!";
                if (audioOn == 1) { 
                    audioPlayer.PlayOneShot(startClip);
                }
            }
            count--;
            yield return new WaitForSeconds(1);
        }

        //Una volta fatta la fase di inzializzazione si da il via all'esercizio 
        //abilitando i collider per l'invio di dati al server e si segnala a una 
        //della funzione "SocketConnectio..." che l'esercizio comincia
        switch (SaveStateAndLoadScene._excercise)
        {
            case 1:
                SocketConnection.waitHandle.Set();
                yield return new WaitForEndOfFrame();
                collider_sphere1.enabled = true;
                collider_sphere2.enabled = true;
                collider_count1.enabled = true;
                collider_count2.enabled = true;
                break;
            case 2:
                SocketConnectionContouring.waitHandle.Set();
                yield return new WaitForEndOfFrame();
                PathCountCollider1.enabled = true;
                PathCountCollider2.enabled = true;
                PathCountCollider3.enabled = true;
                PathCountCollider4.enabled = true;
                contouringCollider.enabled= true;

                break;
            case 3:
                SocketConncetionChasing.waitHandle.Set();
                yield return new WaitForEndOfFrame();
                ColliderChasing.enabled = true;
           
                break;
        }

        initialCountdown.text = null;

        timeCounterCoroutine=StartCoroutine(ExcerciseTime());
        if (SaveStateAndLoadScene._excercise == 3)
        {
            MoveTheCube = CubeMover();
            StartCoroutine(MoveTheCube);
        }
        yield break;
    }

    //Coroutine che inizia il conto alla rovescia partendo dal tempo concesso per l'esercizio
    IEnumerator ExcerciseTime()
    {
        int count = TimeAllowed;
        while (count >= 0)
        {
            countdown.text = "Time Remaining: "+count.ToString();
            if(count == 0)
            {
                StartCoroutine(StopSoundOut(stopClip));    
                TimeIsFinished = 1;
            }
            count--;
            yield return new WaitForSeconds(1);
        }
        yield break;
    }

    //Coroutine per muovere il cubo del chase con la giusta velocità ed il giusto raggio
    IEnumerator CubeMover()
    {
        double  freq_circle;
        double old_samples_per_period, new_samples_per_period;
        double xf, yf;
        double x_offset_circle = 0;
        double y_offset_circle = 0.25f;

        freq_circle = ((2 * Math.PI) / period);
        old_samples_per_period = (2 * Math.PI) / (freq_circle * sampling_time);
        int i = 0;
            
        while (TimeIsFinished == 0)
        {
            freq_circle = ((2 * Math.PI) / period);
            new_samples_per_period= (2 * Math.PI) / (freq_circle * sampling_time);

            
            i =(int)(i * (new_samples_per_period / old_samples_per_period));
            
            xf = x_offset_circle + radius * Math.Sin((freq_circle * i * sampling_time)-0.155f);
            yf = y_offset_circle + radius * Math.Cos((freq_circle * i * sampling_time)-0.155f);

            if (freq_circle * i * sampling_time > 2 * Math.PI)
            {
                i = 0;
            }

            cube.transform.position = new Vector3((float)xf, 0.3f, (float)yf);
            cube.transform.eulerAngles =new Vector3(0, -(float)(Mathf.Rad2Deg*Math.Atan2((yf - y_offset_circle), (xf - x_offset_circle))),0);
            i++;

            old_samples_per_period = new_samples_per_period;
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    //Coroutine per modificare la dimensione del percorso del chasing
    //Funzione chiamata quando si modifica il valore del dropdown dell'esercizio di chasing
    public void ChangePath()
    {
        Destroy(path_Chasing);
        switch (dp1.value)
        {
            case 0:
                SaveStateAndLoadScene._chaseWid = 1;
                path_Chasing = Instantiate(PathSpawner(SaveStateAndLoadScene._chaseWid));
                break;
            case 1:
                SaveStateAndLoadScene._chaseWid = 2;
                path_Chasing = Instantiate(PathSpawner(SaveStateAndLoadScene._chaseWid));
                break;
            case 2:
                SaveStateAndLoadScene._chaseWid = 3;
                path_Chasing = Instantiate(PathSpawner(SaveStateAndLoadScene._chaseWid));
                break;
            case 3:
                SaveStateAndLoadScene._chaseWid = 4;
                path_Chasing = Instantiate(PathSpawner(SaveStateAndLoadScene._chaseWid));
                break;
        }
    }

    //Funzione chiamata per modificare il periodo del chasing
    //Funzione chiamata quando si modifica il valore dello slider dell'esercizio di chasing
    public void ChangePeriod()
    {
        SaveStateAndLoadScene._chasePeriod = 4 + slide.value * 0.2f;
        period = SaveStateAndLoadScene._chasePeriod;
    }
}
