#include "stdafx.h"
#include "HapticSocket.h"


HapticSock::HapticSock()
{
	Failure_Encountered = 0;
}

HapticSock::~HapticSock()
{
}

SOCKET HapticSock::CreateHapticServer(PCSTR IpAddress, u_short Port) {
	/*Nel caso in cui ci sia una qualsiasi errore nella generazione
	della socket, questa funzione returna il valore INVALID_SOCKET*/

	DllVersion_Server = MAKEWORD(2, 1);
	//Gestione degli errori
	if (WSAStartup(DllVersion_Server, &wsaData_Server) != 0) {
		std::cout << "Winsock startup failed" << std::endl;
		Failure_Encountered = 1;
		return INVALID_SOCKET;
	}

	addrlen = sizeof(addr);
	inet_pton(AF_INET, IpAddress, &addr.sin_addr.s_addr);
	addr.sin_port = htons(Port);
	addr.sin_family = AF_INET;

	sListen = socket(AF_INET, SOCK_STREAM, NULL);
	//Gestione degli errori
	if (sListen == INVALID_SOCKET) {
		printf("socket failed with error: %ld\n", WSAGetLastError());
		WSACleanup();
		Failure_Encountered = 1;
		return INVALID_SOCKET;
	}


	int iResult = bind(sListen, (SOCKADDR*)&addr, sizeof(addr));
	//Gestione degli errori
	if (iResult == SOCKET_ERROR) {
		printf("bind failed with error: %d\n", WSAGetLastError());
		closesocket(sListen);
		WSACleanup();
		Failure_Encountered = 1;
		return INVALID_SOCKET;
	}

	std::cout << "Listening" << std::endl;
	iResult = listen(sListen, SOMAXCONN);
	//Gestione degli errori
	if (iResult == SOCKET_ERROR) {
		printf("listen failed with error: %d\n", WSAGetLastError());
		closesocket(sListen);
		WSACleanup();
		Failure_Encountered = 1;
		return INVALID_SOCKET;
	}

	return sListen;
}

void HapticSock::ServerReceive() {
	int bytes_received = 0;
	//Si controlla la riuscita della ricezione
	bytes_received = recv(newConn, buffer_receive, sizeof(buffer_receive), NULL);
	if (bytes_received <= 0) {
		fromClient_connection_closed = 1;
		shutdown(newConn, SD_BOTH);
		closesocket(newConn);
	}
}

void HapticSock::ServerSend(char message[]) {

	int iResult = send(newConn, message, sizeof(buffer_send), NULL);
	//Si controlla la riuscita dell'invio
	if (iResult == SOCKET_ERROR) {
		wprintf(L"send failed with error: %d\n", WSAGetLastError());
		toClient_connection_closed = 1;
		shutdown(newConn, SD_BOTH);
		closesocket(newConn);
	}

}

void HapticSock::CreateServer(PCSTR IpAddress, u_short Port) {
	//Funzione genera una socket da mettere in listen ed attende una connessione
	//NON VIENE UTILIZZATA NELL'INTERFACCIA APTICA
	DllVersion_Server = MAKEWORD(2, 1);
	if (WSAStartup(DllVersion_Server, &wsaData_Server) != 0) {
		std::cout << "Winsock startup failed" << std::endl;
		exit(1);
	}

	addrlen = sizeof(addr);
	inet_pton(AF_INET, IpAddress, &addr.sin_addr.s_addr);
	addr.sin_port = htons(Port);
	addr.sin_family = AF_INET;

	sListen = socket(AF_INET, SOCK_STREAM, NULL);
	bind(sListen, (SOCKADDR*)&addr, sizeof(addr));
	std::cout << "Listening" << std::endl;
	listen(sListen, SOMAXCONN);

	newConn = accept(sListen, (SOCKADDR*)&addr, &addrlen);
	if (newConn == 0) {
		std::cout << "Failed Connection to Client" << std::endl;
	}
	else {
		std::cout << "Client Connected, PORT:" << Port << std::endl;
	}
}

/*Funzioni utilizzate per creare un Client

void HapticSock::CreateClient(PCSTR IpAddress, u_short Port){
	
	DllVersion_Client = MAKEWORD(2, 1);
	if (WSAStartup(DllVersion_Client, &wsaData_Client) != 0){
		std::cout << "Winsock startup failed" << std::endl;
		exit(1);
	}

	addrlen_Client = sizeof(addr_Client);
	inet_pton(AF_INET, IpAddress, &addr_Client.sin_addr.s_addr);
	addr_Client.sin_port = htons(Port);
	addr_Client.sin_family = AF_INET;

	Connect = socket(AF_INET, SOCK_STREAM, NULL);
	if (connect(Connect, (SOCKADDR*)&addr_Client, sizeof(addr_Client)) != 0){
		std::cout << "Failed to Connect" << std::endl;
		exit(1);
	}
	//std::cout << " Connected" << std::endl;
}

void HapticSock::ClientSend(char message[]){
	int iResult=send(Connect, message, sizeof(buffer_send), NULL);
}

void HapticSock::ClientReceive() {
	int iResult = recv(Connect, buffer_receive, sizeof(buffer_receive), NULL);
}
*/

/*Funzioni Demo usate per delle prove
//funzione che crea un server e invia dati quando sono disponibili
void HapticSock::Server_CreateAndKeepSending(char (*message)[256]) {
	
	//CreateServer();
	
	while(1){
		std::unique_lock<std::mutex>lock(mtx);
		while (dato_pronto == 0) {
			dataIsReady.wait(lock);
		}
		ServerSend(*message);
		dato_inviato = 1;
		dato_pronto = 0;
		Sleep(250);
		lock.unlock();
		dataIsSent.notify_one();
	}
}

//funzione fatta per provare a mandare dati
void HapticSock::generate(char(*message)[256]) {
	int k = 0;
	for (int i = 0; i < 101; i++) {
		std::unique_lock<std::mutex>lock(mtx);
		while (dato_inviato == 0) {
			dataIsSent.wait(lock);
		}
		sprintf_s(*message, "%d", k);
		dato_pronto = 1;
		dato_inviato = 0;
		k++;
		lock.unlock();
		dataIsReady.notify_one();
	}
}
*/	
