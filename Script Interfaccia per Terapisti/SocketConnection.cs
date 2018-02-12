using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Assets.Scripts;

public class SocketConnection : MonoBehaviour {

    //variabile che serve a far si che si aspetti il momento giusto per far iniziare l'esercizio dopo il 3..2..1..start
    //aspetto che arrivi la scritta start e invio a c++ un messaggio che cambia un flag. In questo modo io inizializzo la posizione del robot
    //e finchè non arriva start non lascio che il robot si sposti da li per evitare che l'utente vada in zone in cui rischia di farsi del male
    public static AutoResetEvent waitHandle = new AutoResetEvent(false);

    //Text che dice se si è connessi al server per ricevere i dati
    public Text connected;

    //Text che scrive il messaggio che ricevo
    public Text received;

    //Text che riporta quante ripetizioni dell'esercizio si sono fatte
    public Text repCounter;

    //Text che serve a visualizzare un messaggio di errore quando una socket interrompe in modo inaspettato
    //il suo funzionamento
    public Text LostConnection;

    //Bottone per tornare al menu SETTING
    public GameObject BackButton;

    //Thread per ricevere i dati di posizione dal server
    private Thread clientThreadReceive;
    //thread che attende la fine dell'inizializzazione dell'esercizio per segnalare al server di iniziare
    //a gestire il feedback aptico
    private Thread signalTheStart;

    //IPlocale del server
    private string ipAddress = "127.0.0.1";
    //Porta per ricevere i dati dal server
    public const int PORT_FOR_RECEIVE = 9001;
    //Porta per mandare i dati al server
    public const int PORT_FOR_SEND = 9002;

    //client per ricevere i dati di posizione
    private HapticSock client_for_receive;
    //client per inviare i dati di posizione quando c'è un contatto e per inizializzare in modo corretto l'esercizio
    private HapticSock client_for_send;

    //stringa contenente la posizione da inviare al server
    private string position_to_send;

    //flag per indicare se la connessione al server è aperta
    private int connectionOpen = 0;
    //variabile che tiene traccia delle ripetizioni fatte
    public static int repetitions = 0;

    //Variabili che servono a far sì che il conteggio delle ripetizioni avanzi solo se si tocca in modo alternato le sfere
    private int contactSphere1=0;           
    private int contactSphere2=0;

    //Funzione per costruire il messaggio iniziale da mandare a C++ per impostare l'esercizio
    private string InitialMessageBuilder()
    {
        return SaveStateAndLoadScene._excercise.ToString() +"[stop]"+ LoadCorrectScene.diameter + "[stop]" + LoadCorrectScene.spawn[0].x.ToString() + "[stop]" + LoadCorrectScene.spawn[0].z.ToString() + "[stop]" + LoadCorrectScene.spawn[1].x.ToString() + "[stop]" + LoadCorrectScene.spawn[1].z.ToString()+ "[stop]" + LoadCorrectScene.ForceFeed.ToString()+"[stop]" + SaveStateAndLoadScene._sph1.ToString()+ "[stop]" + SaveStateAndLoadScene._sph2.ToString() + "[stop]" + LoadCorrectScene.TimeAllowed + "[stop]" + LoadCorrectScene.NumOfReps + "[stop]";
    }

    //funzione chiamata all'istanziazione della scena, che elimina lo script se l'esercizio scelto non è quello gestito dallo script 
    private void Awake()
    {
        if (SaveStateAndLoadScene._excercise != 1)
        {
           Destroy(this);
        }
    }


    //funzione chiamata prima del primo frame della scena
    void Start () {
        //inizializzazione di diverse componenti testuali
        repetitions = 0;
        received.text = null;
        LostConnection.text = null;
        repCounter.text = "Repetitions: 0";

        //Si creano le socket per la comunicazione, se c'è stato qualche problema si mostra un messaggio di errore
        //e si abilita il pulsante per tornare alla scena precedente
        client_for_receive = new HapticSock(ipAddress, 9001);
        client_for_send = new HapticSock(ipAddress, 9002);
        if (client_for_receive.Connection_problem == 1 || client_for_send.Connection_problem == 1) {
            LostConnection.text = "ERROR: can't connect to the server";
            BackButton.SetActive(true);
        }

        //Se si è connessio al server si visualizza la scritta "Connected"
        if (client_for_receive.flag_connected == 1)
        {
            connected.text = "Connected";
            connectionOpen = 1;
        }
        else
        {
            connected.text = null;
        }

        //Se non ci sono stati problemi di connessione si fa partire la thread per ricevera dati dal server
        clientThreadReceive = new Thread(client_for_receive.Client_Receive);
        if (client_for_receive.Connection_problem == 0)
        {
            clientThreadReceive.Start();
        }

        //Se non ci sono stati problemi di connessione si invia un messaggio di inizializzazione al server
        if (client_for_receive.Connection_problem == 0 && client_for_send.Connection_problem == 0)
        {
            //Si deve dire al server che esercizio si sta facendo, il diametro delle sfere e dove stanno le sfere, il loro colore e la durezza della molla
            string InitialMessage = InitialMessageBuilder();
            client_for_send.Client_Send(InitialMessage);

            //Si fa partire un thread che rimane in attesa di un segnale dallo script LoadCorrectScene che indica che la fase di inizializzazione è finita e può iniziare l'esercizio
            signalTheStart = new Thread(SendStartMessage);
            signalTheStart.Start();
        }
    }

    //Funzione chiamata ad ogni frame
    void Update () {

        //Se è finito il tempo o ho finito le ripetizioni chiudo tutte le socket e attivo il bottone che lascia tornare al menu di setting
        if ((LoadCorrectScene.TimeIsFinished == 1 && connectionOpen == 1)||(LoadCorrectScene.NumOfReps == repetitions && connectionOpen == 1))
        {
            connectionOpen = 0;
            client_for_receive.CloseSock();
            client_for_send.CloseSock();

            BackButton.SetActive(true);
        }

        //Se è finito il tempo o ho finito le ripetizioni chiudo tutte le socket e attivo il bottone che lascia tornare al menu di setting
        if (CheckHololens.flag_Hololens == 0)
        {
            if (connectionOpen == 1)
            { 
                connectionOpen = 0;
                client_for_receive.CloseSock();
                client_for_send.CloseSock();
            }
            LoadCorrectScene.ErrorDetected = 1;
            LostConnection.text = "ERROR: lost connection to the Hololens.\n Press Back Button to go back to setup.";
            BackButton.SetActive(true);

        }

        //Se c'è stato un problema di connessione con le socket che inviano e ricevono dati si mostra un messaggio di errore e si abilita il pulsante per tornare al menu di setting
        if ((client_for_receive.Connection_problem == 1 && LoadCorrectScene.TimeIsFinished == 0 && repetitions < LoadCorrectScene.NumOfReps) || (client_for_send.Connection_problem == 1 && LoadCorrectScene.TimeIsFinished == 0 && repetitions < LoadCorrectScene.NumOfReps))
        {
            if (connectionOpen == 1)
            {
                connectionOpen = 0;
                client_for_receive.CloseSock();
                client_for_send.CloseSock();
            }
            LoadCorrectScene.ErrorDetected = 1;
            LostConnection.text = "ERROR: lost connection to the Server.\n Press Back Button to go back to setup.";
            BackButton.SetActive(true);
        }
    }
    
    
    //Quando l'organo terminale entra nel trigger delle sfere inizio a mandare la posizione a C++. Il trigger delle sfere è più grande della sfera stessa così da essere in grado di annullare la forza generata dai motori
    //altrimenti io tocco la sfera ma appena mollo il trigger non mando posizioni e quindi rimani un valore scritto nel DAC della Sensoray626. In questa funzione si gestisce anche il contatto con il trigger per il conteggio
    //che è invece della stessa dimensione delle sfere
    void OnTriggerEnter(Collider other)
    {
        //Se si tocca un trigger usato per l'invio di dati si invia un messaggio al server
        if (connectionOpen != 0 && !(other.gameObject.CompareTag("TriggerSphere1")|| other.gameObject.CompareTag("TriggerSphere2"))) { 
            string x = gameObject.transform.position.x.ToString();
            string z = gameObject.transform.position.z.ToString();

            if (other.gameObject.CompareTag("Sphere_Collider"))
            {
                position_to_send ="1"+ "[stop]" + x + "[stop]" + z + "[stop]" + repetitions + "[stop]";
                client_for_send.Client_Send(position_to_send);
            }
            else
            {
                position_to_send = "2" + "[stop]" + x + "[stop]" + z + "[stop]" + repetitions + "[stop]";
                client_for_send.Client_Send(position_to_send);
            }
        }
        //Se si tocca un trigger per il conteggio delle ripetizioni si valuta se si sta toccando alternativamente le sfere o no
        //per aumentare il conteggio delle ripetizioni
        else
        {
            if(other.gameObject.CompareTag("TriggerSphere1") && contactSphere1 == 0) {
                contactSphere1 = 1;
                contactSphere2 = 0;
                repetitions++;
                repCounter.text = "Repetions: " + repetitions.ToString();
            }
            else
            {
                if (other.gameObject.CompareTag("TriggerSphere2") && contactSphere2 == 0)
                {
                    contactSphere2 = 1;
                    contactSphere1 = 0;
                    repetitions++;
                    repCounter.text = "Repetions: " + repetitions.ToString();
                }
            }
        }
    }

    //Come onTriggerEnter solo che questa funzione va ad ogni istante in cui l'organo terminale si trova dentro al trigger delle sfere
    void OnTriggerStay(Collider other)
    {
        //Se si tocca un trigger usato per l'invio di dati si invia un messaggio al server
        if (connectionOpen != 0 && !(other.gameObject.CompareTag("TriggerSphere1") || other.gameObject.CompareTag("TriggerSphere2"))) { 
            string x = gameObject.transform.position.x.ToString();
            string z = gameObject.transform.position.z.ToString();
        
            if (other.gameObject.CompareTag("Sphere_Collider"))
            {
                position_to_send = "1" + "[stop]" + x + "[stop]" + z + "[stop]" + repetitions + "[stop]";
                client_for_send.Client_Send(position_to_send);
            }
            else
            {
                position_to_send = "2" + "[stop]" + x + "[stop]" + z + "[stop]" + repetitions + "[stop]";
                client_for_send.Client_Send(position_to_send);
            }
        }
    }


    //Se si è ricevuto un dato della posizione dell'organo terminale dal server si sposta l'avatar di conseguenza
    void FixedUpdate()
    {
        if (connectionOpen != 0) { 
            if (client_for_receive.flag_receive == 1)
            {
                received.text = client_for_receive.message_received;
                gameObject.transform.position = new Vector3(client_for_receive.coordinateX_Z[0], 0.3f, client_for_receive.coordinateX_Z[1]);
                client_for_receive.flag_receive = 0;
            }
        }
    }

    //Funzione chiamata quando viene premuto il bottone per tornare alle impostazioni
    public void BackButtonPressed()
    {
        if (connectionOpen != 0) { 
            client_for_receive.CloseSock();
            client_for_send.CloseSock();
        }
    }

    //Funzione che attende un segnale che indica che l'esercizio può iniziare e 
    //invia un messaggio al servere per comunicargli questo evento
    private void SendStartMessage()
    {
        waitHandle.WaitOne();
        string StartMessage = 1+"[stop]";
        client_for_send.Client_Send(StartMessage);
    }
}
