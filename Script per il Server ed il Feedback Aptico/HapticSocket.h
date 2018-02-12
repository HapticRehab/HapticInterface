#ifndef HAPTIC_SOCKET
#define HAPTIC_SOCKET

#include "stdafx.h"
#include <WinSock2.h>
#include <iostream>
#include <WS2tcpip.h>
#include <mutex>
#include <condition_variable>

#pragma comment(lib,"Ws2_32.lib")

#define LOCALIP 127.0.0.1
#define LOCALPORTSEND 9001
#define LOCALPORTRECEIVE 9002
#define CHECKHOLOLENSPORT 9003
#define HOLOLENSIP 147.162.95.14
#define HOLOLENSPORT 9000

class HapticSock
{
public:
	HapticSock();
	~HapticSock();

	//Funzione che restituisce una socket che è in listen su un determinato indirizzo
	SOCKET CreateHapticServer(PCSTR IpAddress, u_short Port);
	
	//Funzione che permette al server di ricevere dati
	void ServerReceive();
	//Funzione che permette al server di inviare il messaggio "message"
	void ServerSend(char message[]);

	/*Flag usato per indicare che vi è stato qualche problema durante la creazione
	della socket listener*/
	int Failure_Encountered;

	//socket da porre in stato di listen	
	SOCKET sListen;
	//socket che accetta la nuova connessione in ingresso
	SOCKET newConn;

	//buffer per l'invio e la ricezione di messaggi
	char buffer_receive[256] = {};
	char buffer_send[256] = {};

	//flag usati per capire se la connessione è ancora attiva o meno
	int fromClient_connection_closed = 0;
	int toClient_connection_closed = 0;

	//variabili usate per impostare le socket del server
	SOCKADDR_IN addr;
	int addrlen;

	/*Funzione che genera una socket che si mette in listen ed
	accetta una nuova connessione
	La connessione viene gestita dalla socket newConn*/
	void CreateServer(PCSTR IpAddress, u_short Port);

	/*Funzioni per realizzare un client

	void CreateClient(PCSTR IpAddress, u_short Port);							//Funzione che genera una socket che si connette ad un indirizzo specificato
	void ClientSend(char message[]);											//Funzione che permette al client di inviare il messaggio "message"
	void ClientReceive();														//Funzione che permette al client di ricevere dati

	//variabili usate per impostare le socket del client
	SOCKADDR_IN addr_Client;
	int  addrlen_Client;
	WSADATA wsaData_Client;
	WORD DllVersion_Client;
	SOCKET Connect;																//socket utilizzata dal client per connettersi al server
	*/

	/*Funzioni DEMO usate per delle prove
	void Server_CreateAndKeepSending(char(*message)[256]);						//Funzione che crea un server che continua ad inviare messaggi
	void generate(char(*message)[256]);											//Funzione che genera dati che la funzione Server_CreateAndKeepSending
	//continua ad inviare. Il tutto protetto da una mutex

	//variabili per proteggere la memoria conidivisa
	std::mutex mtx;
	std::condition_variable dataIsReady;
	std::condition_variable dataIsSent;
	int dato_inviato = 1;
	int dato_pronto = 0;
	*/

private:
	WSADATA wsaData_Server;
	WORD DllVersion_Server;

};

#endif