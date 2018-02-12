#include "stdafx.h"
#include "ApplicationCore.h"

ApplicationCore::ApplicationCore()
{
	SocketListener_ERROR = 0;
	HOLOLENS_SOCKET_DIED = 0;
}

ApplicationCore::~ApplicationCore()
{
}

void ApplicationCore::sender_to_UNITY(HapticSock& server_send_to_UNITY, HapticDevice& robot, Sensoray626& board) {
	PCSTR ip = "127.0.0.1";
	//Si crea una socket listener
	listener_send = server_send_to_UNITY.CreateHapticServer(ip, LOCALPORTSEND);
	//Se ci sono stati problemi nella creazione delle varie socket che ascoltano si termina il programma
	if (server_send_to_UNITY.Failure_Encountered == 1) {
		SocketListener_ERROR = 1;
		return;
	}

	//Attesa del controllo sul MYO
	std::unique_lock<std::mutex>lock(startingMtx);
	while (MYOcheckDone == 0) {
		canIstart.wait(lock);
	}
	lock.unlock();

	//Se non è stato trovato il MYO o si è verificato un problema nella creazione delle socket listener, si chiude il programma
	if (dontStart == 0 && SocketListener_ERROR==0) {

		while (1) {
			//Si accetta una nuova connessione
			server_send_to_UNITY.newConn = accept(listener_send, (SOCKADDR*)&server_send_to_UNITY.addr, &server_send_to_UNITY.addrlen);
			//Gestione degli errori, se la connessione è fallita si ritorna in stato di listen
			if (server_send_to_UNITY.newConn == INVALID_SOCKET) {
				shutdown(server_send_to_UNITY.newConn, SD_BOTH);
				closesocket(server_send_to_UNITY.newConn);
				if (close_ALL == 1) {
					return;
				}
				server_send_to_UNITY.fromClient_connection_closed = 1;
			}
			else {
				std::cout << "Client Connected, PORT:" << LOCALPORTSEND << std::endl;
				server_send_to_UNITY.toClient_connection_closed = 0;
			}
			
			//Parte la funzione per l'invio di dati a UNITY
			if (server_send_to_UNITY.toClient_connection_closed == 0) {
				SEND_DATA(server_send_to_UNITY, board, robot);
			}
		}
	}
	else {
		return;
	}
}

void ApplicationCore::SEND_DATA(HapticSock& server_send_to_UNITY, Sensoray626& board, HapticDevice& robot) {
	float* data;
	char message[256];

	//aspetto che la inizializzazione dell'esercizio sia stata fatta
	std::unique_lock<std::mutex>lock(mtx_init);
	while (init_send == 0) {
		initialization_ready.wait(lock);
	}

	init_send = 0;
	lock.unlock();

	//Parte un ciclo che invia dati a UNITY finchè stop_dialog=0
	while (stop_dialog==0) {
		data = robot.CinematicaDirettaPosizione(board);
		sprintf_s(message, "%.4f[stop]%.4f[stop]", data[0], data[1]);
		server_send_to_UNITY.ServerSend(message);
		if (server_send_to_UNITY.toClient_connection_closed == 1) {
			shutdown(server_send_to_UNITY.newConn, SD_BOTH);
			closesocket(server_send_to_UNITY.newConn);
			stop_dialog = 1;
			return;
		}
	}

	//Se si è usciti dal ciclo senza chiudere la socket newConn si procede a chiuderla
	if (server_send_to_UNITY.toClient_connection_closed == 0) {
		shutdown(server_send_to_UNITY.newConn, SD_BOTH);
		closesocket(server_send_to_UNITY.newConn);
	}
}

void ApplicationCore::findElbowPose(DataCollector& armBand, myo::Hub& hub) {
	//Parte un ciclo che continua ad estrarre dati dal MYO relativi all'orientazione
	//dell'avambraccio
	while (stop_dialog == 0 && HOLOLENS_SOCKET_DIED==0) {
		hub.run(1);
		elbowMovement = armBand.printElbowMovement();
		Sleep(10);
	}
}

void ApplicationCore::findMYO(myo::Hub& hub, myo::Myo*& myo, DataCollector& armBand, HapticDevice robot, Sensoray626 board) {
	std::cout << "Attempting to find a Myo...You have 10 seconds..." << std::endl;
	myo = hub.waitForMyo(10000);

	//Si cerca un MYO per 10 secondi, se non si trova si chiede se si vuole cercarlo ancora
	if (!myo) {
		std::cout << "Unable to find a Myo!" << std::endl;
		int msgBOX = MessageBox(NULL, (LPCWSTR)L"MYO Armaband not Found.\n Connect a MYO armband to Run Excercise.\nCancel Exits the Entire Program.", (LPCWSTR)L"MYO Error", MB_ICONWARNING | MB_RETRYCANCEL);

		switch (msgBOX) {
		case(IDCANCEL): {
			MYOcheckDone = 1;
			dontStart = 1;
			canIstart.notify_all();
			exit(-1);
			break;
		}
		case(IDRETRY): {
			findMYO(hub, myo, armBand,robot,board);
			break;
		}
		}
	}

	//Una volta trovato il MYO si fa partire l'applicazione UNTIY per impostare gli esercizi
	//Se l'applicazione non viene trovato si termina il server
	if ((int)ShellExecute(GetDesktopWindow(), L"open", L"..\\TestBuild\\test.exe", NULL, NULL, SW_SHOW) <= 32) {
		MessageBox(NULL,
			(LPCWSTR)L"Resource not available\n Check UnityFile.exe",
			(LPCWSTR)L"Error in Opening Unity Runnable",
			MB_ICONERROR | MB_OK
		);
		exit(-1);
	}
	else {
		robot.setInitialCoordinates(board, DAC1, DAC2);
	}
}

void ApplicationCore::send_to_HOLOLENS_with_MYO(HapticSock& server_send_to_HOLOLENS, Sensoray626& board, HapticDevice& robot, DataCollector& armBand) {
	PCSTR ip = "147.162.95.114";
	//Si crea una socket listener
	listener_hololens = server_send_to_HOLOLENS.CreateHapticServer(ip, HOLOLENSPORT);
	//Se ci sono stati problemi nella creazione delle varie socket che ascoltano si termina il programma
	if (server_send_to_HOLOLENS.Failure_Encountered == 1) {
		SocketListener_ERROR = 1;
		return;
	}
	
	//Variabile per salvare i dati provenienti dalla soluzione del problema cinematico diretto
	float *data;
	//buffer dove salvare il messaggio ricevuto
	char message[256];
	//Variabili che contengono le coordinate X,Y del cubo da inseguire
	float x_chase, y_chase;

	//Variabili per la gestione dell'armband MYO
	std::thread myo_thread;
	myo::Hub hub("com.unipd.controllo626");
	myo::Myo* myo;

	try {
		//Si cerca il MYO
		findMYO(hub, myo, armBand,robot,board);
		std::cout << "Connected to a Myo armband!" << std::endl << std::endl;
		hub.addListener(&armBand);

		//Si segnala che il MYO è stato trovato
		MYOcheckDone = 1;
		dontStart = 0;
		canIstart.notify_all();

		//Se non ci sono stati errori nella creazione di socket listener, inizia un ciclo per gestire
		//la connessione con gli Hololens
		while (SocketListener_ERROR==0) {

			HololensConnected = 0;
			
			//Si accetta una nuova conessione
			server_send_to_HOLOLENS.newConn = accept(listener_hololens, (SOCKADDR*)&server_send_to_HOLOLENS.addr, &server_send_to_HOLOLENS.addrlen);
			//Se la connessione non è valida si fa in modo di ritornare in stato di listen
			if (server_send_to_HOLOLENS.newConn == INVALID_SOCKET) {
				shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
				closesocket(server_send_to_HOLOLENS.newConn);
				HOLOLENS_SOCKET_DIED = 1;
				server_send_to_HOLOLENS.toClient_connection_closed = 1;
				if (close_ALL == 1) {
					return;
				}
			}
			else {
				std::cout << "Client Connected, PORT:" << HOLOLENSPORT << std::endl;
				server_send_to_HOLOLENS.toClient_connection_closed = 0;
				HololensConnected = 1;
				HOLOLENS_SOCKET_DIED = 0;
			}

			//Fino a quando la connessione rimane valida, si effettua un ciclo per inviare dati all'Hololens
			while (server_send_to_HOLOLENS.toClient_connection_closed == 0) {

				//Si attende la ricezione del messaggio di inizializzazione
				std::unique_lock<std::mutex>lock(mtx_init);
				while (init_holo == 0) {
					initialization_ready.wait(lock);
				}
				init_holo = 0;
				lock.unlock();

				//Se è necessario chiudere il server si termina la thread
				if (close_ALL == 1) {
					shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
					closesocket(server_send_to_HOLOLENS.newConn);
					if (HololensConnected == 0) {
						shutdown(listener_hololens, SD_BOTH);
						closesocket(listener_hololens);
					}
					return;
				}

				//in base all'esercizio scelto si invia un messaggio di inizializzazione della scena all'Hololens
				switch (excercise) {
				case 1:
					sprintf_s(message, "%d[stop]%.4f[stop]%.4f[stop]%.4f[stop]%.4f[stop]%.4f[stop]%d[stop]%d[stop]%d[stop]%d[stop]", excercise, x_molla1, y_molla1, x_molla2, y_molla2, diameter_Spheres, Sphere1Color, Sphere2Color, TimeAllowed, repetitions_required);
					break;
				case 2:
					sprintf_s(message, "%d[stop]%.4f[stop]%.4f[stop]%.4f[stop]%.4f[stop]%d[stop]%d[stop]%d[stop]", excercise, x_centroPercorso, y_centroPercorso, path_dim, path_wid, pathColor, TimeAllowed, repetitions_required);
					break;
				case 3:
					sprintf_s(message, "%d[stop]%.4f[stop]%.4f[stop]%d[stop]%d[stop]%d[stop]", excercise, x_offset_chasing, y_offset_chasing, radius, chaseColor, TimeAllowed);
					break;
				}

				//In base all'esercizio scelto si inviano messaggi diversi all'Hololens
				switch (excercise)
				{
				//Chasing
				case 3:
					//Si invia il messaggio di inizializzazione
					server_send_to_HOLOLENS.ServerSend(message);
					//Se la connessione è terminata si modificano dei flag per segnalare alle altre thread
					//il problema
					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						HololensConnected = 0;
						shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
						closesocket(server_send_to_HOLOLENS.newConn);
						HOLOLENS_SOCKET_DIED = 1;
						break;
					}

					//Si controlla se è necessario chiudere la connessione per problemi avvenuti con 
					//le altre socket
					if (stop_dialog == 1) {
						sprintf_s(message, "%s[stop]", "close");
						server_send_to_HOLOLENS.ServerSend(message);
						if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
							HololensConnected = 0;
							shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
							closesocket(server_send_to_HOLOLENS.newConn);
							HOLOLENS_SOCKET_DIED = 1;
							myo_thread.join();
							break;
						}
					}

					//si fa partire una thread per gestire il MYO
					myo_thread = std::thread(&ApplicationCore::findElbowPose,this, std::ref(armBand), std::ref(hub));

					//Parte un ciclo per inviare dati agli Hololens, fino a quando l'esercizio
					//non è iniziato si inviano le coordinate iniziali del cubo da inseguire
					while (exc_started == 0) {
								
						data = robot.CinematicaDirettaPosizione(board);
						sprintf_s(message, "%.4f[stop]%.4f[stop]%.4f[stop]%.4f[stop]%s[stop]%d[stop]", data[0], data[1], chase_radius* sin(-0.155f), y_offset_chasing + chase_radius*cos(-0.155f), elbowMovement, radius);
						server_send_to_HOLOLENS.ServerSend(message);
						if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
							HololensConnected = 0;
							shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
							closesocket(server_send_to_HOLOLENS.newConn);
							HOLOLENS_SOCKET_DIED = 1;
							myo_thread.join();
							break;
						}

						//Si controlla se è necessario chiudere la connessione per problemi avvenuti con 
						//le altre socket
						if (stop_dialog == 1) {
							sprintf_s(message, "%s[stop]", "close");
							server_send_to_HOLOLENS.ServerSend(message);
							if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
								HololensConnected = 0;
								shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
								closesocket(server_send_to_HOLOLENS.newConn);
								HOLOLENS_SOCKET_DIED = 1;
								myo_thread.join();
								break;
							}
						}
					}
					
					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						break;
					}

					//Parte un ciclo per inviare i dati di posizione dell'organo terminale e del cubo da inseguire all'Hololens
					//Si invia anche il valore del raggio della circonferenza su cui eseguire l'inseguimento
					while (stop_dialog == 0) {
								
							data = robot.CinematicaDirettaPosizione(board);
							//gestione della memoria condivisa
							std::unique_lock<std::mutex>lock(mtx_chasing);
							while (data_to_send == 0) {
								data_ready_to_send.wait(lock);
							}
							x_chase = coordinateX_Y_mollaCHASING[0];
							y_chase = coordinateX_Y_mollaCHASING[1];

							data_to_send = 0;

							data_sent.notify_one();
							lock.unlock();

							sprintf_s(message, "%.4f[stop]%.4f[stop]%.4f[stop]%.4f[stop]%s[stop]%d[stop]", data[0], data[1], x_chase, y_chase, elbowMovement, radius);
							//invio del messaggio agli Hololens
							server_send_to_HOLOLENS.ServerSend(message);
							//Se la connessione è terminata si modificano dei flag per segnalare il problema
							//alle altre thread e si fa in modo che la thread che crea il feedback aptico
							//rimanga in attesa dell'invio di un dato
							if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
								HololensConnected = 0;
								shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
								closesocket(server_send_to_HOLOLENS.newConn);
								HOLOLENS_SOCKET_DIED = 1;
								myo_thread.join();
								data_to_send = 0;
								data_sent.notify_one();
								break;
							}
					}

					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						break;
					}

					sprintf_s(message, "%s[stop]", "close");
					//invio di un messaggio di chiusura agli Hololens
					server_send_to_HOLOLENS.ServerSend(message);
					//Se la connessione è terminata si modificano dei flag per segnalare alle
					//altre thread del problema
					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						HololensConnected = 0;
						shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
						closesocket(server_send_to_HOLOLENS.newConn);
						HOLOLENS_SOCKET_DIED = 1;
						myo_thread.join();
						break;
					}

					myo_thread.join();
					break;

				default:
					//Si invia il messaggio di inizializzazione
					server_send_to_HOLOLENS.ServerSend(message);
					//Se la connessione è terminata si modificano dei flag per segnalare alle altre thread
					//il problema
					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						HololensConnected = 0;
						shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
						closesocket(server_send_to_HOLOLENS.newConn);
						HOLOLENS_SOCKET_DIED = 1;
						break;
					}

					//si fa partire una thread per gestire il MYO
					myo_thread = std::thread(&ApplicationCore::findElbowPose,this, std::ref(armBand), std::ref(hub));

					//Parte un ciclo per inviare i dati di posizione dell'organo terminale e del cubo da inseguire all'Hololens
					//Si invia anche il valore del raggio della circonferenza su cui eseguire l'inseguimento
					while (stop_dialog == 0) {
								
						data = robot.CinematicaDirettaPosizione(board);
						sprintf_s(message, "%.4f[stop]%.4f[stop]%s[stop]%d[stop]", data[0], data[1], elbowMovement, repetitions_count);
						server_send_to_HOLOLENS.ServerSend(message);
						if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
							HololensConnected = 0;
							shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
							closesocket(server_send_to_HOLOLENS.newConn);
							HOLOLENS_SOCKET_DIED = 1;
							myo_thread.join();
							break;
						}	
					}

					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						break;
					}
					
					sprintf_s(message, "%s[stop]", "close");
					//invio di un messaggio di chiusura agli Hololens
					server_send_to_HOLOLENS.ServerSend(message);
					//Se la connessione è terminata si modificano dei flag per segnalare alle
					//altre thread del problema
					if (server_send_to_HOLOLENS.toClient_connection_closed == 1) {
						HololensConnected = 0;
						shutdown(server_send_to_HOLOLENS.newConn, SD_BOTH);
						closesocket(server_send_to_HOLOLENS.newConn);
						HOLOLENS_SOCKET_DIED = 1;
						myo_thread.join();
						break;
					}
					
				
					myo_thread.join();
					break;
				}
			}
		}
	}
	catch (const std::exception& e) {
		std::cerr << "Error: " << e.what() << std::endl;
		std::cerr << "Press enter to continue.";
		return;
	}
	if (HololensConnected == 0) {
		shutdown(listener_hololens, SD_BOTH);
		closesocket(listener_hololens);
	}
}

void ApplicationCore::receive_from_UNITY_and_HANDLE_DATA(HapticSock& server_receive_from_UNITY, Sensoray626& board, HapticDevice& robot) {
	PCSTR ip = "127.0.0.1";
	//Si crea una socket listener
	listener_handle = server_receive_from_UNITY.CreateHapticServer(ip, LOCALPORTRECEIVE);
	//Se ci sono stati problemi nella creazione delle varie socket che ascoltano si termina il programma
	if (server_receive_from_UNITY.Failure_Encountered == 1) {
		SocketListener_ERROR = 1;
		return;
	}

	//Attesa del controllo sul MYO
	std::unique_lock<std::mutex>lock(startingMtx);
	while (MYOcheckDone == 0) {
		canIstart.wait(lock);
	}
	lock.unlock();

	//Se non è stato trovato il MYO o si è verificato un problema nella creazione delle socket listener, si chiude il programma
	if (dontStart == 0 && SocketListener_ERROR==0) {
		while (1) {
			//Si accetta una nuova connessione
			server_receive_from_UNITY.newConn = accept(listener_handle, (SOCKADDR*)&server_receive_from_UNITY.addr, &server_receive_from_UNITY.addrlen);
			//Gestione degli errori, se la connessione è fallita si ritorna in stato di listen
			if (server_receive_from_UNITY.newConn == INVALID_SOCKET) {
				shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
				closesocket(server_receive_from_UNITY.newConn);
				if (close_ALL == 1) {
					return;
				}
				server_receive_from_UNITY.fromClient_connection_closed = 1;
			}
			else {
				std::cout << "Client Connected, PORT:" << LOCALPORTRECEIVE << std::endl;
				server_receive_from_UNITY.fromClient_connection_closed = 0;
			}

			//Parte la funzione per la gestione degli esercizi
			if (server_receive_from_UNITY.fromClient_connection_closed == 0) {
				HANDLE_DATA(server_receive_from_UNITY, board, robot);
			}
		}
	}
	else {
		return;
	}
}

void ApplicationCore::HANDLE_DATA(HapticSock& server_receive_from_UNITY, Sensoray626& board, HapticDevice& robot) {
	char* numbers = NULL;
	char* context = NULL;
	//il separatore posto nel messaggio a dividere i parametri ricevuti 
	char separators[] = "[stop]";
	//messaggio ricevuto attraverso la socket
	char* message_received;
	//le coordinate dell'organo terminale ricevute
	float coordinateX_Y[2];

	//ID della sfera con cui si è in contatto
	int SphereID;
	//Rigidezza della molla utilizzata
	int stiffness;
	//thread per inizializzare la posizione del manipolatore
	std::thread init_pos;
	//indice necessario all'esecuzione della media mobile per il calcolo della velocità
	int averageIndex = 0;

	//si azzerano i flag utilizzati per segnalare che è i parametri per l'inizializzazione
	//sono stati ricevuti
	init_send = 0;
	init_holo = 0;

	//Devo capire che esercizio fare, ricevo un messaggio di inizializzazione
	server_receive_from_UNITY.ServerReceive();
	//Se è fallita la ricezione si chiude la socket e si risveglia la thread che invia a UNITY per farla terminare.
	//La thread che gestisce gli Hololens può rimanere in attesa dell'inizializzazione
	if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
		shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
		closesocket(server_receive_from_UNITY.newConn);
		stop_dialog = 1;
		init_send = 1;
		init_holo = 0;
		initialization_ready.notify_all();
		return;
	}

	message_received = server_receive_from_UNITY.buffer_receive;

	numbers = strtok_s(message_received, separators, &context);
	excercise = std::stof(numbers);

	//A seconda dell'esercizio scelto si salvano diversi parametri
	switch (excercise) {
	case 1:
		numbers = strtok_s(NULL, separators, &context);
		robot.L_molla = (std::stof(numbers) / 2) + 0.005;  //0.005 è il raggio dell'organo terminale
		diameter_Spheres = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		robot.x_centro_molla1 = std::stof(numbers);
		x_molla1 = robot.x_centro_molla1;

		numbers = strtok_s(NULL, separators, &context);
		robot.y_centro_molla1 = std::stof(numbers);
		y_molla1 = robot.y_centro_molla1;

		numbers = strtok_s(NULL, separators, &context);
		robot.x_centro_molla2 = std::stof(numbers);
		x_molla2 = robot.x_centro_molla2;

		numbers = strtok_s(NULL, separators, &context);
		robot.y_centro_molla2 = std::stof(numbers);
		y_molla2 = robot.y_centro_molla2;

		numbers = strtok_s(NULL, separators, &context);
		stiffness = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		Sphere1Color = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		Sphere2Color = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		TimeAllowed = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		repetitions_required = std::stof(numbers);

		//Si modificano dei flag e si segnala che è possibile inizializzare l'esercizio
		stop_dialog = 0;
		init_send = 1;
		init_holo = 1;
		initialization_ready.notify_all();

		//Parte una thread che sposta l'organo terminale in una posizione desiderata
		init_pos = std::thread(&HapticDevice::initializePosition, &robot, std::ref(board), 0, 0.25);

		//Si riceve un messaggio che termina la thread init_pos
		server_receive_from_UNITY.ServerReceive();
		//Se la socket è terminata si modifica il flag stop_dialog per far terminare la 
		//thread che invia dati a UNITY e lo socket legata agli Hololens torna in stato di attesa di
		//dell'inizializzazione
		if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
			shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
			closesocket(server_receive_from_UNITY.newConn);
			stop_dialog = 1;
			robot.flag_excercise_start = 1;
			init_pos.join();
			return;
		}

		message_received = server_receive_from_UNITY.buffer_receive;

		numbers = strtok_s(message_received, separators, &context);

		if (std::stof(numbers) == 1) {
			robot.flag_excercise_start = 1;
		}

		//Si sceglie la rigidezza della molla virtuale
		switch (stiffness)
		{
		case (1):
			robot.k_molla = LOW_K;
			break;
		case(2):
			robot.k_molla = MEDIUM_K;
			break;
		case(3):
			robot.k_molla = HIGH_K;
			break;
		case(4):
			robot.k_molla = NULL_K;
		default:
			robot.k_molla = 0;
			std::cout << "Wrong K" << std::endl;
			break;
		}
		break;



	//Contouring
	case 2:

		numbers = strtok_s(NULL, separators, &context);
		robot.pathDimension = std::stof(numbers);
		path_dim = robot.pathDimension;
		robot.L_molla_interna_contouring = robot.pathDimension + 0.005;

		numbers = strtok_s(NULL, separators, &context);
		robot.pathWidth = std::stof(numbers);
		path_wid = robot.pathWidth;
		robot.L_molla_esterna_contouring = robot.pathDimension + (robot.pathWidth) - 0.005;

		robot.limit = robot.pathDimension + (robot.pathWidth / 2);

		numbers = strtok_s(NULL, separators, &context);
		robot.x_centro_percorso = std::stof(numbers);
		x_centroPercorso = robot.x_centro_percorso;

		numbers = strtok_s(NULL, separators, &context);
		robot.y_centro_percorso = std::stof(numbers);
		y_centroPercorso = robot.y_centro_percorso;

		numbers = strtok_s(NULL, separators, &context);
		stiffness = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		pathColor = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		TimeAllowed = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		repetitions_required = std::stof(numbers);

		//Si modificano dei flag e si segnala che è possibile inizializzare l'esercizio
		stop_dialog = 0;
		init_send = 1;
		init_holo = 1;
		initialization_ready.notify_all();

		//Parte una thread che sposta l'organo terminale in una posizione desiderata
		init_pos = std::thread(&HapticDevice::initializePosition, &robot, std::ref(board), 0 - (robot.pathDimension + (robot.pathWidth / 2)), 0.24);

		//Si riceve un messaggio che termina la thread init_pos
		server_receive_from_UNITY.ServerReceive();
		//Se la socket è terminata si modifica il flag stop_dialog per far terminare la 
		//thread che invia dati a UNITY e lo socket legata agli Hololens torna in stato di attesa di
		//dell'inizializzazione
		if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
			shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
			closesocket(server_receive_from_UNITY.newConn);
			stop_dialog = 1;
			robot.flag_excercise_start = 1;
			init_pos.join();
			return;
		}

		message_received = server_receive_from_UNITY.buffer_receive;
		numbers = strtok_s(message_received, separators, &context);

		if (std::stof(numbers) == 1) {
			robot.flag_excercise_start = 1;
		}

		//Si sceglie la rigidezza della molla virtuale
		switch (stiffness)
		{
		case (1):
			robot.k_molla = LOW_K;
			break;
		case(2):
			robot.k_molla = MEDIUM_K;
			break;
		case(3):
			robot.k_molla = HIGH_K;
			break;
		case(4):
			robot.k_molla = NULL_K;
		default:
			robot.k_molla = 0;
			std::cout << "Wrong K" << std::endl;
			break;
		}

		break;

	//Chasing
	case 3:

		numbers = strtok_s(NULL, separators, &context);
		radius = std::stof(numbers);
		switch (radius) {
		case 1:
			robot.radius_chasing = 0.04;
			chase_radius = 0.04f;
			break;
		case 2:
			robot.radius_chasing = 0.06;
			chase_radius = 0.06f;
			break;
		case 3:
			robot.radius_chasing = 0.08;
			chase_radius = 0.08f;
			break;
		case 4:
			robot.radius_chasing = 0.10;
			chase_radius = 0.1f;
			break;
		}
		numbers = strtok_s(NULL, separators, &context);
		robot.period_chasing = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		robot.x_offset_circle_chasing = std::stof(numbers);
		x_offset_chasing = robot.x_offset_circle_chasing;


		numbers = strtok_s(NULL, separators, &context);
		robot.y_offset_circle_chasing = std::stof(numbers);
		y_offset_chasing = robot.y_offset_circle_chasing;

		numbers = strtok_s(NULL, separators, &context);
		stiffness = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		chaseColor = std::stof(numbers);

		numbers = strtok_s(NULL, separators, &context);
		TimeAllowed = std::stof(numbers);

		//Si modificano dei flag e si segnala che è possibile inizializzare l'esercizio
		stop_dialog = 0;
		init_send = 1;
		init_holo = 1;
		initialization_ready.notify_all();

		//Parte una thread che sposta l'organo terminale in una posizione desiderata
		init_pos = std::thread(&HapticDevice::initializePosition, &robot, std::ref(board), robot.x_offset_circle_chasing + robot.radius_chasing*sin((2 * M_PI / robot.period_chasing)*0.001 - 0.155f), robot.y_offset_circle_chasing + robot.radius_chasing*cos((2 * M_PI / robot.period_chasing)*0.001 - 0.155f));

		//Si riceve un messaggio che termina la thread init_pos
		server_receive_from_UNITY.ServerReceive();
		//Se la socket è terminata si modifica il flag stop_dialog per far terminare la 
		//thread che invia dati a UNITY e lo socket legata agli Hololens torna in stato di attesa di
		//dell'inizializzazione
		if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
			shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
			closesocket(server_receive_from_UNITY.newConn);
			stop_dialog = 1;
			robot.flag_excercise_start = 1;
			init_pos.join();
			return;
		}

		message_received = server_receive_from_UNITY.buffer_receive;

		numbers = strtok_s(message_received, separators, &context);

		if (std::stof(numbers) == 1) {
			robot.flag_excercise_start = 1;
			exc_started = 1;
		}

		//Si sceglie la rigidezza della molla virtuale
		switch (stiffness)
		{
		case (1):
			robot.k_molla_chasing = LOW_K_CHASING;
			break;
		case(2):
			robot.k_molla_chasing = MEDIUM_K_CHASING;
			break;
		case(3):
			robot.k_molla_chasing = HIGH_K_CHASING;
			break;
		case(4):
			robot.k_molla_chasing = NULL_K;
		default:
			robot.k_molla_chasing = 0;
			std::cout << "Wrong K" << std::endl;
			break;
		}

		break;
	}

	switch (excercise)
	{
	case(1): //REACHING

		//Parte il cronometro per il calcolo della velocità
		robot.start = std::chrono::high_resolution_clock::now();
		
		//Parte il ciclo che controlla l'esercizio fino a quando la connessione con UNITY viene chiusa oppure 
		//una tra le socket che inviano dati a UNITY o agli Hololens si chiude in modo inaspettato
		while (server_receive_from_UNITY.fromClient_connection_closed == 0 && stop_dialog == 0) {
			
			server_receive_from_UNITY.ServerReceive();
			//Se la connessione è terminata si modifica il flag stop_dialog per terminare la thread che invia
			//dati a UNITY e riportare quella che invia dati agli hololens in stato di attesa
			if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
				shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
				closesocket(server_receive_from_UNITY.newConn);
				stop_dialog = 1;
				break;
			}
			
			message_received = server_receive_from_UNITY.buffer_receive;
			numbers = strtok_s(message_received, separators, &context);
			SphereID = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			coordinateX_Y[0] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			coordinateX_Y[1] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			repetitions_count = std::stof(numbers);

			//Si genera il feedback aptico
			robot.molla_per_contatto_con_sfere(board, coordinateX_Y[0], coordinateX_Y[1], SphereID, averageIndex);
			averageIndex = (averageIndex + 1) % robot.averageSize;
		}
	
		board.resetDac();
		robot.cleanMovingAverage();

		break;

	case(2)://CONTOURING

		//Parte il cronometro per il calcolo della velocità
		robot.start = std::chrono::high_resolution_clock::now();

		//Parte il ciclo che controlla l'esercizio fino a quando la connessione con UNITY viene chiusa oppure 
		//una tra le socket che inviano dati a UNITY o agli Hololens si chiude in modo inaspettato
		while (server_receive_from_UNITY.fromClient_connection_closed == 0 && stop_dialog == 0) {

			server_receive_from_UNITY.ServerReceive();
			//Se la connessione è terminata si modifica il flag stop_dialog per terminare la thread che invia
			//dati a UNITY e riportare quella che invia dati agli hololens in stato di attesa
			if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
				shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
				closesocket(server_receive_from_UNITY.newConn);
				stop_dialog = 1;
				break;
			}
			
			message_received = server_receive_from_UNITY.buffer_receive;
			numbers = strtok_s(message_received, separators, &context);
			coordinateX_Y[0] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			coordinateX_Y[1] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			repetitions_count = std::stof(numbers);

			//Si genera il feedback aptico
			robot.molla_per_CONTOURING(board, coordinateX_Y[0], coordinateX_Y[1], averageIndex);
			averageIndex = (averageIndex + 1) % robot.averageSize;
			
		}

		board.resetDac();
		robot.cleanMovingAverage();

		break;

	case(3)://CHASING

		//Parte il cronometro per il calcolo della velocità
		robot.start = std::chrono::high_resolution_clock::now();

		//Parte il ciclo che controlla l'esercizio fino a quando la connessione con UNITY viene chiusa oppure 
		//una tra le socket che inviano dati a UNITY o agli Hololens si chiude in modo inaspettato
		while (server_receive_from_UNITY.fromClient_connection_closed == 0 && stop_dialog == 0) {
			
			server_receive_from_UNITY.ServerReceive();
			//Se la connessione è terminata si modifica il flag stop_dialog per terminare la thread che invia
			//dati a UNITY e riportare quella che invia dati agli hololens in stato di attesa.
			//In questo esercizio è necessario gestire la memoria condivisa e far sì che la thread che invia dati all'Hololens
			//non rimanga in stato di attesa di un nuovo dato da inviare, creando una deadlock
			if (server_receive_from_UNITY.fromClient_connection_closed == 1) {
				shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
				closesocket(server_receive_from_UNITY.newConn);
				stop_dialog = 1;
				data_to_send = 1;
				data_ready_to_send.notify_one();
				break;
			}
			
			message_received = server_receive_from_UNITY.buffer_receive;
			numbers = strtok_s(message_received, separators, &context);
			coordinateX_Y[0] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			coordinateX_Y[1] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);

			//Si gestisce la memoria condivisa
			std::unique_lock<std::mutex>lock(mtx_chasing);
			if (HOLOLENS_SOCKET_DIED == 0) {
				while (data_to_send == 1) {
					data_sent.wait(lock);
				}
			}

			coordinateX_Y_mollaCHASING[0] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			coordinateX_Y_mollaCHASING[1] = std::stof(numbers);
			numbers = strtok_s(NULL, separators, &context);
			radius = std::stof(numbers);

			if (HOLOLENS_SOCKET_DIED == 0) {
				data_to_send = 1;
				data_ready_to_send.notify_one();
			}
			lock.unlock();
			
			//Si genera il feedback aptico
			robot.molla_per_CHASING(board, coordinateX_Y[0], coordinateX_Y[1], coordinateX_Y_mollaCHASING[0], coordinateX_Y_mollaCHASING[1], averageIndex);
			averageIndex = (averageIndex + 1) % robot.averageSize;
		}
		
		board.resetDac();
		robot.cleanMovingAverage();

		break;

	default:
		MessageBox(NULL,
			(LPCWSTR)L"The Operation For Excercise\n Selection Went Wrong.\n Internal Error.",
			(LPCWSTR)L"Error in Selecting Excercise",
			MB_ICONERROR | MB_OK
		);
		break;
	}

	//Si cambiano dei flag per permettera la chiusura della connessione che invia dati ad UNITY
	//e di quella che invia dati agli Hololens
	stop_dialog = 1;
	if (excercise == 3) {
		exc_started = 0;
		data_to_send = 1;
		data_ready_to_send.notify_one();
	}
	
	if (server_receive_from_UNITY.fromClient_connection_closed == 0) {
		shutdown(server_receive_from_UNITY.newConn, SD_BOTH);
		closesocket(server_receive_from_UNITY.newConn);
	}
	init_pos.join();
	robot.flag_excercise_start = 0;
	repetitions_count = 0;
}

//Questa funzione contiene delle socket che dialogano con UNITY per informare l'applicazione
//se gli Hololens sono connessi e per gestire la chiusura del server. Se una di queste socket termina
//in modo inaspettato il server viene terminato poichè non si è più in grado di gestire la chiusura del programma 
//ed il controllo dello stato degli Hololens.
void ApplicationCore::CHECK_IF_HOLOLENS_CONNECTED() {
	HapticSock send_to_UNITY_if_HOLOLENS_connected;
	PCSTR ip = "127.0.0.1";
	//Si crea una socket listener
	listener_checkHololens = send_to_UNITY_if_HOLOLENS_connected.CreateHapticServer(ip, CHECKHOLOLENSPORT);
	//Se ci sono stati problemi nella creazione delle varie socket che ascoltano si termina il programma
	if (send_to_UNITY_if_HOLOLENS_connected.Failure_Encountered == 1) {
		SocketListener_ERROR = 1;
		return;
	}

	char message[256];
	char* msg;

	//Attesa del controllo sul MYO
	std::unique_lock<std::mutex>lock(startingMtx);
	while (MYOcheckDone == 0) {
		canIstart.wait(lock);
	}
	lock.unlock();

	//Se non è stato trovato il MYO o si è verificato un problema nella creazione delle socket listener, si chiude il programma
	if (dontStart == 0 && SocketListener_ERROR==0) {

		while (1) {
			//Si accetta una nuova connessione
			send_to_UNITY_if_HOLOLENS_connected.newConn = accept(listener_checkHololens, (SOCKADDR*)&send_to_UNITY_if_HOLOLENS_connected.addr, &send_to_UNITY_if_HOLOLENS_connected.addrlen);
			//Gestione degli errori
			if (send_to_UNITY_if_HOLOLENS_connected.newConn == 0) {
				std::cout << "Failed Connection to Client" << std::endl;
				close_all_listener_sockets();
			}
			else {
				std::cout << "Client Connected, PORT:" << CHECKHOLOLENSPORT << std::endl;
			}

			send_to_UNITY_if_HOLOLENS_connected.toClient_connection_closed = 0;

			//Parte un ciclo per il controllo dello stato della connessione con gli Hololens
			while (send_to_UNITY_if_HOLOLENS_connected.toClient_connection_closed == 0) {

				if (HololensConnected == 0) {
					msg = "no";
				}
				else {
					msg = "si";
				}

				//Si invia il messaggio a UNITY
				sprintf_s(message, "%s[stop]", msg);
				send_to_UNITY_if_HOLOLENS_connected.ServerSend(message);
				Sleep(1000);
				
				//Se la socket non è più attiva, si termina il server
				if (send_to_UNITY_if_HOLOLENS_connected.toClient_connection_closed == 1) {

					close_all_listener_sockets();

					return;
				}
			}
		}
	}
	else {
		return;
	}
}

void ApplicationCore::close_all_listener_sockets() {
	//Si modifica il flag per indicare che è necessario terminare il server
	close_ALL = 1;

	//Si chiudono tutte le socket in stato di listen
	if (listener_handle != INVALID_SOCKET) {
		shutdown(listener_handle, SD_BOTH);
		closesocket(listener_handle);
		listener_handle = INVALID_SOCKET;
	}

	if (listener_send != INVALID_SOCKET){
		shutdown(listener_send, SD_BOTH);
		closesocket(listener_send);
		listener_send = INVALID_SOCKET;
	}

	if (listener_checkHololens != INVALID_SOCKET) {
		shutdown(listener_checkHololens, SD_BOTH);
		closesocket(listener_checkHololens);
		listener_checkHololens = INVALID_SOCKET;
	}

	//Se gli Hololens sono connessi, la thread a loro dedicata potrebbe essere in stato di attesa
	//dei dati di inizializzazione della scena. E' quindi necessario risvegliare la thread prima di chiuderla
	if (HololensConnected == 1) {
		init_holo = 1;
		initialization_ready.notify_all();
	}
	else {
		if(listener_hololens!= INVALID_SOCKET){
			shutdown(listener_hololens, SD_BOTH);
			closesocket(listener_hololens);
			listener_hololens = INVALID_SOCKET;
		}
	}
}

void ApplicationCore::main_wait_for_MYOcheck_done(Sensoray626& board, HapticDevice& robot) {

	std::unique_lock<std::mutex>lock(startingMtx);
	while (MYOcheckDone == 0) {
		canIstart.wait(lock);
	}
	lock.unlock();

	if (dontStart == 0) {
		Sleep(1000);
		if ((int)ShellExecute(GetDesktopWindow(), L"open", L"..\\TestBuild\\test.exe", NULL, NULL, SW_SHOW) <= 32) {
			MessageBox(NULL,
				(LPCWSTR)L"Resource not available\n Check UnityFile.exe",
				(LPCWSTR)L"Error in Opening Unity Runnable",
				MB_ICONERROR | MB_OK
			);
		}
		else {
			robot.setInitialCoordinates(board, DAC1, DAC2);
		}
	}
}