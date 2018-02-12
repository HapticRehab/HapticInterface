//Questo file riporta le implementazioni delle funzioni definite nell'header file Sensoray626.h
#include "stdafx.h"
#include "Sensoray626.h"

Sensoray626::Sensoray626()
{
	closed_Flag = 0;
}

Sensoray626::~Sensoray626()
{
}

int Sensoray626::startBoard(){
	closed_Flag = 0;
	//Si gestiscono i fallimenti dell'apertura della .dll
	switch (S626_DLLOpen()) {   // Open the API. MUST CALL THIS FIRST!
	case ERR_LOAD_DLL:
		printf("can't open s626.dll\n");	// check dll path
		return -1;
	case ERR_FUNCADDR:
		printf("dll function(s) not found\n");  // check dll version
		S626_DLLClose();    // Close the API.
		return -1;
	default:  // if dll was successfully opened ...
		S626_OpenBoard(0, 0, NULL, THREAD_PRIORITY_NORMAL);
		if (S626_GetErrors(0)) {
			printf("errors opening board\n");
			S626_DLLClose();
			closed_Flag = 1;
			return closed_Flag;
		}
		return 0;
	}
}

int Sensoray626::closeBoard() const{
	//Se la board non era già stata chiusa, si può chiudere
	if (closed_Flag != 1) {
		S626_CloseBoard(0);
		S626_DLLClose();
	}
	return 0;
}

//Scrive sul Dac un valore tra -8191 e 8191 che corrisponde ad una tensione di uscita tra -10 e 10 V. IL PARAMETRO VOLT INDICA I VOLT DESIDERATI IN USCITA
//I canali vanno da 0 a 3
void Sensoray626::writeToDac(int channel, double volt) const{
	if (volt > 3.5||volt<-3.5){
		if (volt > 0){
			volt = 3.5;
		}
		else{
			volt = 3.5;
		}
	}
	
	int value = VoltConversion*volt;
	if (value<=8191 && value >=-8191){
		S626_WriteDAC(0, channel, value);
	}
	else{
		S626_WriteDAC(0, channel, 0);
		std::cout << "Volt out of bounds: -10V < volt < 10V" << std::endl;
	}
}

void Sensoray626::resetDac() const{
	for (int i = 0; i < 4; i++){
		writeToDac(i, 0);
	}
}

void Sensoray626::setEncoder() const{
		for (int i = 0; i < 6; i++){
			S626_CounterModeSet(0, i,
				(LOADSRC_INDX << BF_LOADSRC) |
				(INDXSRC_HARD << BF_INDXSRC) |
				(INDXPOL_POS << BF_INDXPOL) |
				(CLKSRC_COUNTER << BF_CLKSRC) |
				(CLKPOL_POS << BF_CLKPOL) |
				(CLKMULT_4X << BF_CLKMULT) |
				(CLKENAB_ALWAYS << BF_CLKENAB));
			S626_CounterPreload(0, i, 0);
			S626_CounterSoftIndex(0, i);
			S626_CounterLoadTrigSet(0, i, 1);
			S626_CounterLatchSourceSet(0, i, LATCHSRC_AB_READ);
			S626_CounterEnableSet(0, i, CLKENAB_ALWAYS);
			S626_CounterIntSourceSet(0, i, INTSRC_BOTH);
		}
}

float Sensoray626::readEncoder(int channel){
	conteggio[channel] = S626_CounterReadLatch(0, channel);
	posizione[channel] = conteggio[channel] * RadiantConversion;
	return posizione[channel];
}