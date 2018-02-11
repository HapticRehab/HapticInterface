using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Globalization;

//Classe che contiene gli elementi per generare delle socket per dialogare con il server e gestire la ricezione di dati
//per l'inizializzazione degli esercizi e la ricezione della posizione dell'organo terminale

namespace Assets.Scripts
{
    public class HapticSocket : SocketConnection
    {
        public Socket conn;                                             //socket per connettersi al server
        public byte[] receive_buffer = new byte[256];                   //buffer per ricevere dati dalla socket
        public int flag_receive = 0;                                    //flag che indica che la ricezione dati è avvenuta   
        public int flag_connected = 0;                                  //flag che indica se si è connessi al server
        public int flag_excercise_ended = 0;                            //flag per capire che l'esercizio è finito e devo ripartire da capo
        public string message_received;                                 //stringa che contiene il messaggio ricevuto
        public float[] coordinateX_Z=new float[2];                      //array che contiene le coordinate dell'organo terminale ricevute
        public float[] coordinateX_Z_cubo = new float[2];               //array che contiene le coordinate del cubo che si muove nell'esercizio di chasing 
        public string elbowPose;                                        //stringa che contiene l'informazione ricavata dall'armband MYO
        public int radius_circle_chasing;                               //parametro che indica di che grandezza è la circonferenza del chasing
        public int repetitions_count=0;                                 //variabile che contiene il numero di ripetizioni che sono state effettuate durante l'esercizio

        private string[] numbers;                                       //la stringa ricevuta dalla socket viene salvata in message_received e poi divisa in più stringhe,
                                                                        //a seconda di quanti numeri ci sono al suo interno, e vengono salvate in questo array

        private string[] stringSeparators = new string[] { "[stop]" };  //separatore utilizzato per dividere i singoli parametri nei messaggi ricevuti 

        //Variabili necessarie alla ricezione dei messaggi. Può essere necessario più di un receive per
        //ricevere un singolo messaggio. Per questo si riceve fino a quando la quantità di byte ricevuti è di 256
        private int br = 0;                                             //byte ricevuti in totale nella ricezione di un messaggio
        private int bytes_received = 0;                                 //byte ricevuti nella singola operazione di ricezione
        
        //alla creazione dell'istanza si inizializza una socket
        public HapticSocket() {
            conn = socket();
            flag_excercise_ended = 0;
        }

        //Funzione per chiudere la socket
        public void CloseSocket()
        {
            conn.Shutdown(SocketShutdown.Both);
            conn.Close();
        }

        //Funzione per connettersi al server
        public void ConnectToServer()
        {
            try
            {
                conn.Connect(new IPEndPoint(IPAddress.Parse("147.162.95.114"), 9000));
                flag_connected = 1;
                
            }
            catch(SocketException e)
            {
                flag_connected = 0;
            }   
        }

        ~HapticSocket() { }
        
        //Funzione che inizializza una socket
        Socket socket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        //Funzione che riceve un messaggio dal server in C++ per inizializzare la scena corretta in base all'esercizio
        public string[] ClientInitializeScene()
        {
            br = 0;
            try
            {
                //Si continua a ricevere finchè non si è riempito il buffer per la ricezione
                while (br < receive_buffer.Length)
                {
                    bytes_received = conn.Receive(receive_buffer, br, receive_buffer.Length - br, 0);
                    br += bytes_received;
                }

                //il messaggio ricevuto viene convertito in una stringa per poi essere analizzato nella funzione "init" della main thread
                message_received = Encoding.ASCII.GetString(receive_buffer, 0, bytes_received);
                numbers = message_received.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                culture.NumberFormat.NumberDecimalSeparator = ".";

                return numbers;
            }
            catch
            {
                return null;
            }
        }

        //Funzione che riceve dati dal server durante gli esercizi di contouring e reaching
        public void Client()
        { 
            //Fino a quando l'esercizio è in corso si ricevono dati
            while (flag_excercise_ended==0)
            {
                br = 0;
                try
                {
                    //Si continua a ricevere finchè non si è riempito il buffer per la ricezione
                    while (br < receive_buffer.Length)
                    {
                        bytes_received = conn.Receive(receive_buffer, br, receive_buffer.Length - br, 0);
                        if (bytes_received == 0)
                        {
                            throw new SocketException(1);
                        }
                        br += bytes_received;
                    }
                    //il messaggio ricevuto viene convertito in una stringa
                    message_received = Encoding.ASCII.GetString(receive_buffer, 0, bytes_received);
                    numbers = message_received.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    //Se si è ricevuto "close" vuol dire che l'esercizio è finito
                    if (numbers[0] != "close") {
                        //la stringa viene divisa nei diversi parametri che la compongono, sono divisi dalla dicitura "[stop]"
                        coordinateX_Z[0] = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        coordinateX_Z[1] = float.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    
                        elbowPose = numbers[2];
                        repetitions_count=int.Parse(numbers[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                        flag_receive = 1;
                    }else
                    {
                        flag_excercise_ended = 1;
                        return;
                    }
                }
                catch 
                {
                    flag_excercise_ended = 1;
                    if (bytes_received == 0)
                    {
                        flag_connected = 0;
                    }
                    return;
                }
            }
        }

        //Funzione che riceve dati dal server durante l'esercizio di chasing.
        //Serve una funzione differente dagli altri 2 esercizi poichè nel chasing si deve ricevere
        //la posizione del cubo da inseguire ed anche il raggio della circonferenza lungo la quale avviene
        //l'inseguimento, in quanto questo può variare e si deve modificare la scena di conseguenza
        public void Client_Chasing()
        {
            //Fino a quando l'esercizio è in corso si ricevono dati
            while (flag_excercise_ended == 0)
            {
                br = 0;
                try
                {
                    //Si continua a ricevere finchè non si è riempito il buffer per la ricezione
                    while (br < receive_buffer.Length)
                    {
                        bytes_received = conn.Receive(receive_buffer, br, receive_buffer.Length - br, 0);
                        if (bytes_received == 0)
                        {
                            throw new SocketException(1);
                        }
                        br += bytes_received;
                    }
                    //il messaggio ricevuto viene convertito in una stringa
                    message_received = Encoding.ASCII.GetString(receive_buffer, 0, bytes_received);
                    numbers = message_received.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    //Se si è ricevuto "close" vuol dire che l'esercizio è finito
                    if (numbers[0] != "close")
                    {
                        //la stringa viene divisa nei diversi parametri che la compongono, sono divisi dalla dicitura "[stop]"
                        coordinateX_Z[0] = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                        coordinateX_Z[1] = float.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                        
                        coordinateX_Z_cubo[0] = float.Parse(numbers[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                        coordinateX_Z_cubo[1] = float.Parse(numbers[3], NumberStyles.Any, CultureInfo.InvariantCulture);

                        elbowPose = numbers[4];

                        radius_circle_chasing = int.Parse(numbers[5], NumberStyles.Any, CultureInfo.InvariantCulture);
                        message_received = radius_circle_chasing.ToString();
                        flag_receive = 1;
                    }
                    else
                    {
                        flag_excercise_ended = 1;
                        return;
                    }
                }
                catch
                {
                    flag_excercise_ended = 1;
                    if (bytes_received == 0)
                    {
                        flag_connected = 0;
                    }
                    return;
                }
            }
        }
    }
}
