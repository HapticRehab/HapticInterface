using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Globalization;

namespace Assets.Scripts
{
    public class HapticSock : SocketConnection
    {

        //socket per ricevere dati
        Socket connection_receive = null;
        //socket per inviare dati                    
        Socket connection_send = null;
        //socket per ricevere informazioni sulla connession dell'Hololens                                      
        Socket connection_Holo = null;

        //buffer per ricevere dati                                             
        public byte[] receive_buffer = new byte[256];
        //flag che indica l'avvenuta ricezione di un messaggio                         
        public int flag_receive = 0;
        //flag che indica che si è connessi al server                                                
        public int flag_connected = 0;
        //flag per indicare che un messaggio è stato inviato                                              
        public int flag_sent = 0;
        //flag che indica l'avvenuta ricezione di un messaggio con informazioni sulla connession dell'Hololens                                              
        public int flag_receive_check = 0;
        //stringa che contiene il messaggio ricevuto                                          
        public string message_received;
        //array che contiene le coordinate ricevute
        public float[] coordinateX_Z = new float[2];
        //array che contiene le ccordinate ricevute del cubo da inseguire
        public float[] chased_coordinateX_Z = new float[2];

        //stringa che contiene la posizione da inviare
        public string position;
        //stringa per dire se l'hololens è connesso o meno
        public string check;
        //flag per indicare se sono avvenuti problemi di connessione                                  
        public int Connection_problem = 0;

        //array che contiene i parametri divisi del messaggio ricevuto
        private string[] numbers;
        //byte ricevuti totali
        private int br = 0;
        //byte ricevuti con un singolo recive                                                       
        private int bytes_received = 0;
        //byte inviati                                            
        public int bytes_sent = 0;
        //separatore dei dati da inviare
        private string[] stringSeparators = new string[] { "[stop]" };
        //buffer per inviare messaggi            
        public byte[] msg = new byte[256];

        //Funzione per inizializzare le socket
        public HapticSock(string ip,int port) {
            if (port == PORT_FOR_RECEIVE)
            {
                connection_receive = socket();
                try
                {
                    connection_receive.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                    connection_receive.ReceiveTimeout = 2000;
                    flag_connected = 1;
                }
                catch(SocketException e)
                {
                    Connection_problem = 1;
                }
            }
            else
            {
                if (port == PORT_FOR_SEND) { 
                    connection_send = socket();
                    try
                    {
                        connection_send.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                        connection_send.SendTimeout = 2000;
                        flag_connected = 1;
                    }
                    catch(SocketException e)
                    {
                        Connection_problem = 1;
                    }
                }
                else
                {
                    connection_Holo = socket();
                    connection_Holo.ReceiveTimeout = 4000;
                    try
                    {
                        connection_Holo.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                        flag_connected = 1;
                    }
                    catch (SocketException e)
                    {
                        Connection_problem = 1;
                    }
                }
            }
        }

        //Distruttore
        ~HapticSock() {}

        //Funzione per chiudere le Socket
        public void CloseSock()
        {
            if(connection_receive!= null) { 
                connection_receive.Shutdown(SocketShutdown.Both);
                connection_receive.Close();
            }

            if (connection_send != null)
            {
                connection_send.Shutdown(SocketShutdown.Both);
                connection_send.Close();
            }

            if (connection_Holo != null)
            {
                connection_Holo.Shutdown(SocketShutdown.Both);
                connection_Holo.Close();
            }
        }


        //Restituisce una TCP socket
        Socket socket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }


        //Funzione che riceve la posizione dell'organo terminale dal server
        public void Client_Receive()
        {
         
            while (true)
            {
                br = 0;
                try
                {
                    //Fino a quando non si è riempito il buffer vuol dire che non si è ricevuto tutto il messaggio e bisogna continuare a ricevere
                    while (br < receive_buffer.Length)
                    {
                        bytes_received = connection_receive.Receive(receive_buffer, br, receive_buffer.Length - br, 0);
                        if (bytes_received == 0) {
                            throw new SocketException(1);
                        }
                        br += bytes_received;
                    }
                    //Si analizza il messaggio ricevuto e si estraggono i dati da esso
                    message_received = Encoding.ASCII.GetString(receive_buffer, 0, bytes_received);
                    numbers = message_received.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    coordinateX_Z[0] = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                    coordinateX_Z[1] = float.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    flag_receive = 1;
                }
                catch
                {
                    //Se c'è stato un problema di connessione si termina la funzione e si modifica un flag
                    flag_receive = 1;
                    Connection_problem = 1;
                    break;
                }
            }
        }

        //Funzione per ricevere dal server informazioni riguardo allo stato della connessione degli Hololens
        public void Client_Hololens_Checker()
        {
            while (true)
            {
                br = 0;
                try
                {
                    //Fino a quando non si è riempito il buffer vuol dire che non si è ricevuto tutto il messaggio e bisogna continuare a ricevere
                    while (br < receive_buffer.Length)
                    {
                        bytes_received = connection_Holo.Receive(receive_buffer, br, receive_buffer.Length - br, 0);
                        if (bytes_received == 0)
                        {
                            throw new SocketException(1);
                        }
                        br += bytes_received;
                    }
                    //Si analizza il messaggio ricevuto e si estraggono i dati da esso
                    message_received = Encoding.ASCII.GetString(receive_buffer, 0, bytes_received);
                    numbers = message_received.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    check = numbers[0];
                    
                    flag_receive_check = 1;
                }
                catch
                {
                    //Se c'è stato un problema di connessione si termina la funzione e si modifica un flag
                    flag_receive_check = 1;
                    Connection_problem = 1;
                    return;
                }
            }
        }

        //Funzione per inviare i dati al server
        public void Client_Send(string message)
        {
                    msg = Encoding.UTF8.GetBytes(message);
                    Array.Resize<byte>(ref msg, 256);

                    try
                    {
                        bytes_sent = connection_send.Send(msg, msg.Length, 0);
                    }
                    catch (SocketException e)
                    {
                        Connection_problem = 1;
                    }
        }
    }
}
