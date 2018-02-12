//Sensoray626 è una classe che permette di lavorare con la scheda Sensoray626. In particolare permette: la sua inizializzazione, la sua chiusura, di scrivere sui DAC, di resettare i DAC,
//di impostare i contatori e di leggere gli Encoder.  

#ifndef SENSORAY_626
#define SENSORAY_626

#define _USE_MATH_DEFINES

#define DAC1 0
#define DAC2 1
#define ENC1 0
#define ENC2 2

#include <math.h>
#include <Windows.h>
#include <iostream>
#include "win626.h"

class Sensoray626
{
public:
	Sensoray626();										//Costruttore
	~Sensoray626();										//Distruttore
	int startBoard();									//Inizializza la scheda Sensoray e apre la .dll
	int closeBoard() const;								//Chiude la scheda e la .dll
	void writeToDac(int channel, double volt) const;	//Scrive un valore tra -8191 e 8191 (-10 e +10 V) suo dei quattro(0,1,2,3) canali del DAC.
	void resetDac() const;								//Resetta tutti i DAC della scheda affinchè abbiano un output nullo
	void setEncoder() const;							//Inizializza gli TUTTI gliEncoder 
	float readEncoder(int channel);						//Legge il valore dell'Encoder relativo al canale "channel" (channel va da 0 a 5)

	int closed_Flag;									//Flag che indica se l'apertura della scheda o della .dll sia andata a buon fine
	
private:
	const double VoltConversion=819.1;					//Parametro che indica il rapporto di conversione tra interi e volt necessario a scrivere un valore corretto nel DAC		
	const float RadiantConversion = (2 * M_PI / 4000);	//Parametro che permette di convertire il valore letto dagli encoder, che è un conteggio, in radianti 
	DWORD conteggio[6];									//Tiene traccia del conteggio dell'Encoder sul canale channel
	float posizione[6];									//Tiene traccia del valore in radianti misurato dell'Encoder sul canale channel
	
};

#endif
