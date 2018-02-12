//Controllo 626.ccp è il MAIN della console application. Ricordarsi che i DAC usati sono nei canali 0 e 1.
//Gli Encoder utilizzati sono nei canali 0 e 2.

#include "stdafx.h"
#include "HapticSocket.h"
#include <windows.h>
#include "win626.h"
#include <iostream>
#include "Sensoray626.h"
#include "HapticDevice.h"
#include <sstream>
#include <thread>
#include "DataCollector_MYO.h"
#include "ApplicationCore.h"

using std::cout;
using std::endl;
using std::cin;

int main()
{
	
	FreeConsole();
	
	Sensoray626 board;
	HapticDevice robot,robot_to_send_to_UNITY,robot_to_receive_from_UNITY,robot_to_send_to_HOLOLENS;
	HapticSock server;
	HapticSock server_to_SEND_to_UNITY;
	HapticSock server_to_RECEIVE_from_UNITY;
	HapticSock server_to_SEND_to_HOLOLENS;
	DataCollector armBand;
	ApplicationCore application;


	board.startBoard();										//Inizializzo la scheda Sensoray626 e apro la .dll
	board.setEncoder();										//Inizializzo gli Encoder
	board.resetDac();										//Spengo tutti i Dac

	//Gli do una posizione in x e y e lui da ogni punto in cui è ci arriva
	/*
	robot.initializePosition(board, 0, 0.25);
	Sleep(5000);
	robot.initializePosition(board, 0, 0.25);
	*/

	//DEMO 45 GRADI
	
	//robot.ControlTo45Degrees(board);
	
	
	//DEMO MOLLA
	
	//robot.molla(board);
	

	//DEMO CERCHIO
	
	/*
	robot.GenerazioneCerchio(0.08);
	robot.DinamicaInversaPosizionePerCerchio();
	robot.ControlCerchio(board);
	*/

	//robot.chasing(board, 0.08, 10);
	

	/*INTERFACCIA APTICA*/

	MessageBox(NULL,(LPCWSTR)L"WATCH OUT!\nTHE ROBOT IS GOING TO MOVE\nTO SETUP ITS COORDINATES\nSTAY AWAY FROM IT UNTIL IT STOPS.",(LPCWSTR)L"Robot Setting Up",MB_ICONWARNING | MB_OK);

	std::thread send_UNITY(&ApplicationCore::sender_to_UNITY,&application, std::ref(server_to_SEND_to_UNITY), std::ref(robot_to_send_to_UNITY), std::ref(board));
	std::thread receive_UNITY(&ApplicationCore::receive_from_UNITY_and_HANDLE_DATA, &application, std::ref(server_to_RECEIVE_from_UNITY), std::ref(board), std::ref(robot_to_receive_from_UNITY));
	std::thread check_HOLOLENS(&ApplicationCore::CHECK_IF_HOLOLENS_CONNECTED, &application);
	std::thread send_HOLOLENS_MYO_2(&ApplicationCore::send_to_HOLOLENS_with_MYO, &application, std::ref(server_to_SEND_to_HOLOLENS), std::ref(board), std::ref(robot_to_send_to_HOLOLENS), std::ref(armBand));

	send_UNITY.join();
	receive_UNITY.join();
	send_HOLOLENS_MYO_2.join();
	check_HOLOLENS.join();
	
	board.closeBoard();										//Chiudo la scheda Sensoray 626 e chiudo la .dll
	return 0;
}
