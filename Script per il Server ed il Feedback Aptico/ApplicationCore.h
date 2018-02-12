#ifndef  APPLICATION_CORE
#define APPLICATION_CORE

#include "HapticSocket.h";
#include "Sensoray626.h";
#include "HapticDevice.h";
#include "DataCollector_MYO.h"

#include <windows.h>
#include <iostream>
#include <sstream>
#include <thread>

class ApplicationCore
{
public:

	ApplicationCore();
	~ApplicationCore();	

	//Funzione che crea una socket listener e quando riceve una connessione inizia a inviare dati ad unity
	void sender_to_UNITY(HapticSock& server_send_to_UNITY, HapticDevice& robot, Sensoray626& board);
	//Funzione che invia dati ad UNITY
	void SEND_DATA(HapticSock& server_send_to_UNITY, Sensoray626& board, HapticDevice& robot);				
	//Funzione che rileva la posizione del gomito usando l'armband MYO
	void findElbowPose(DataCollector& armBand, myo::Hub& hub);		
	//Funzione che cerca un armband MYO. Se non  si trova si offre la possibilità di riprovare a cercare
	//oppure si chiude l'applicazione. Se il MYO viene trovato si fa partire l'applicazione UNITY per impostare
	//gli esercizi. Se non viene trovato l'eseguibile dell'applicazione si chiude tutto il programma
	void findMYO(myo::Hub& hub, myo::Myo*& myo, DataCollector& armBand, HapticDevice robot, Sensoray626 board);			
	//Funzione che invia informazioni agli hololens
	void send_to_HOLOLENS_with_MYO(HapticSock& server_send_to_HOLOLENS, Sensoray626& board, HapticDevice& robot, DataCollector& armBand);
	//Funzione che crea una socket listener e quando riceve una connessione riceve dati da unity per gestire gli esercizi
	void receive_from_UNITY_and_HANDLE_DATA(HapticSock& server_receive_from_UNITY, Sensoray626& board, HapticDevice& robot);
	//Funzione che regola gli esercizi 
	void HANDLE_DATA(HapticSock& server_receive_from_UNITY, Sensoray626& board, HapticDevice& robot);
	//Funzione che controlla se gli hololens sono connessi e che fa terminare tutte le thread se l'applicazione unity viene chiusa
	void CHECK_IF_HOLOLENS_CONNECTED();																		
	//Funzione per chiudere le socket in stato di listen
	void close_all_listener_sockets();

	void main_wait_for_MYOcheck_done(Sensoray626& board, HapticDevice& robot);								//NON PIU UTILIZZATA!!!!!!!!!!!    Funzione per far attendere il main per il controllo del MYO. Se il MYO è presente si fa partire l'applicazione UNITY

	//Socket che gestiscono l'invio di dati a unity, la ricezione dei dati da unity e l'invio di dati all'hololens
	SOCKET listener_send;
	SOCKET listener_handle;
	SOCKET listener_hololens;
	SOCKET listener_checkHololens;

private:

	//Variabile per la scelta dell'esercizio
	int excercise = 0;

	//Parametri generali degli esercizi:
	//Tempo concesso
	int TimeAllowed;		
	//Ripetizioni desiderate
	int repetitions_required;						

	//Variabili per il Reaching:
	//Diametro delle sfere da visualizzare
	float diameter_Spheres;
	//Colori delle sfere(1 sfera sinistra, 2 sfera destra)
	int Sphere1Color, Sphere2Color;
	//Coordinate dei centri delle sfere virtuali(1 sfera sinistra, 2 sfera destra)
	float x_molla1, y_molla1, x_molla2, y_molla2;

	//Variabili per il Contouring:
	//Coordinate del centro del percorso da visualizzare
	float x_centroPercorso, y_centroPercorso;
	//Dimensione del percorso(S,M,L,XL) e larghezza del percorso
	float path_dim, path_wid;
	//Colore del percorso
	int pathColor;

	//Variabili per il Chasing:
	//Coordinate del centro del percorso su cui si fa il chasing
	float x_offset_chasing, y_offset_chasing;
	//Raggio della circonferenza lungo la quale si fa il chasing
	float chase_radius;					
	//Colore del cubo che si deve inseguire
	int chaseColor;				
	//intero che va posto in uno switch per capire il raggio della circonferenza del chasing
	int radius;										
	//Coordinate del cubo che si deve inseguire, questa variabile va protetta con una mutex perchè va usata da due funzioni in due thread diverse
	float coordinateX_Y_mollaCHASING[2];

	//Queste condizioni sono necessarie perchè la variabile coordinateX_Y_mollaCHASING è condivisa:
	//condizione che indica che è possibile inviare i dati del chasing all'hololens
	std::condition_variable data_ready_to_send;		
	//condizione che indica se è necessario attendere l'invio di un dato precedente agli hololens prima di riceverne un altro da unity
	std::condition_variable data_sent;				
	//mutex usata nel chasing
	std::mutex mtx_chasing;							
	//variabile che indica se è presente un messaggio ancora da inviare o no (0 se no, 1 se sì)
	int data_to_send = 0;							

	//Variabile per conoscere la posizione del gomito
	char *elbowMovement;

	//Variabile per inviare agli Hololens il numero di ripetizioni eseguite
	int repetitions_count;

	//Variabile usatA per stabilire quando terminare le comunicazioni con UNITY e con gli HOLOLENS(quando finisce l'esercizio)
	int stop_dialog = 0;

	//Variabile che serve a capire quando inizia l'esercizio di chasing. Quando si è ancora in fase di inizializzazione dell'esercizio è necessario invare
	//agli hololens la posizione iniziale del cubo da inseguire. Questa posizione viene fornita a UNITY, ma visto che l'esercizio non è ancora iniziato
	//non c'è ancora un dialogo che trasmetta tale informazione. Si invia agli hololens la posizione iniziale nota ed impostata in UNITY. Negli altri due esercizi non 
	//serve fare questo perchè non ci sono elementi che si muovono a parte la sfera che descrive l'organo terminale e per sapere le coordinate di questa 
	//basta fare la cinematica diretta di posizione
	int exc_started = 0;

	//Variabili per inidicare quando sono stati ricevuti i dati per inizializzare l'esercizio:
	//Flag per controllare lo stato di inizializzazione della thread che invia dati a UNITY
	int init_send = 0;
	//Flag per controllare lo stato di inizializzazione della thread che invia dati all'hololens
	int init_holo = 0;
	//la thread dedicata all'hololens aspetta su questa condizione fino a quando non è possibile inviare i dati inizializzare l'ambiente virtuale
	std::condition_variable initialization_ready;
	//mutex per sincronizzare le thread in fase di inizializzazione. Dopo che è stata fatta l'inizializzazione si inizia a trasmetterer dati
	std::mutex mtx_init;

	//Variabile che indica se la socket che parla con gli hololens è aperta(1 se aperta, 0 se no)
	int HololensConnected = 0;

	//Variabile per indicare quando l'applicazione unity è stata chiusa ed è necessario chiudere la connessione con gli hololens
	int close_ALL = 0;

	//Variabili necessarie a far partire in modo corretto l'applicazione. 
	//variabile che indica se il check sui MYO è stato fatto. Finchè non si rileva un armband è possibile ripetere il check o chiudere l'applicazione
	int MYOcheckDone = 0;	
	//in quel caso dontStart diventa 1 e l'applicazione termina		
	int dontStart = 0;			
	//conidizione che serve a far attendere il check sui MYO
	std::condition_variable canIstart;					
	//mutex per attendere il check sui MYO
	std::mutex startingMtx;								

	//flag per segnalare che l'inizializzazione di una socket listener è fallita
	int SocketListener_ERROR;
	//flag per segnalare che la socket che dialoga con gli Hololens è terminata
	int HOLOLENS_SOCKET_DIED;
};

#endif 
