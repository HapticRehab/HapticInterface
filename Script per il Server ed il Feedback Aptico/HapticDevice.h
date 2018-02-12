/*Haptic Device è una classe che lavora insieme alla classe Sensoray626 per permettere
di controllare in vari modi il pentalatero aritcolato. Permette di svolegere i problemi
cinematici diretto ed inverso di poszione. Contiene funzione che permettono di posizionare
i bracci a 45 gradi, di seguire un riferimento circolare e di modellare
una molla virtuale a cui viene ancorato l'organo terminale*/

//!!!!!!!TUTTE LE MISURE DI LUNGHEZZA IN QUESTA CLASSE SONO IN METRI!!!!!!!!!//

#ifndef HAPTIC_DEVICE
#define HAPTIC_DEVICE

//Rigidezze delle molle per gli esercizi di reaching e contouring [N/m]
#define LOW_K 20;
#define MEDIUM_K 30;
#define HIGH_K 40;
#define NULL_K 0;

//Rigidezze delle molle per gli esercizi di chasing [N/m]
#define LOW_K_CHASING 7
#define MEDIUM_K_CHASING 8
#define HIGH_K_CHASING 9

#include <Windows.h>
#include "Sensoray626.h"
#include <vector>
#include <sys/timeb.h>
#include <fstream>
#include <chrono>

class HapticDevice
{
public:
	HapticDevice();
	~HapticDevice();

	/*Funzioni generiche usate all'interno di altri metodi della classe*/
	
	//Trova lo 0 meccanico del dispositivo così da avere un sistema di riferimento adeguato
	void setInitialCoordinates(Sensoray626& board,int DAC_channel1,int DAC_channel2) const;							
	//Porta i bracci ad un'angolazione di 45 gradi	//Porta i bracci ad un'angolazione di 45 gradi
	void ControlTo45Degrees(Sensoray626& board);																	
	//Fa la retroazione della posizione e scrive un riferimento di tensione sui DAC
	void anelloDiRetroazione(Sensoray626&board, float Riferimento_Motore1, float Riferimento_Motore2, double gain);	

	/*Funzioni per la risoluzione del problema cinematico diretto ed inverso*/

	//Trova la posizione dell'organo terminale a partire dagli angoli misurati
	float* CinematicaDirettaPosizione(Sensoray626& board);
	//Restituisce gli angoli a cui sono i motori quando l'organo terminale è nella posizion x,y
	void CinematicaInversaPosizione(float x, float y);

	/*Funzioni usate per gli esercizi aptici*/

	//Porta l'organo terminale nella posizione finale desiderata in 5 secondi 
	void initializePosition(Sensoray626& board, float finalX, float finalY);
	//Gestisce il contatto dell'organo terminale con due sfere
	void molla_per_contatto_con_sfere(Sensoray626& board, float x, float y, int SphereID, int averageIndex);
	//Gestisce l'esercizio di contouring
	void molla_per_CONTOURING(Sensoray626& board, float x, float y, int averageIndex);
	//Funzione che gestisce l'esercizio di chasing
	void molla_per_CHASING(Sensoray626& board, float x, float y, float x_molla, float y_molla, int averageIndex);
	//Resetta le variabili usate per la media mobile alla fine dell'esercizio
	void cleanMovingAverage();

	/*variabili usate nel reaching*/

	//Coordinata x del punto in cui è ancorata la molla virtuale per la sfera 1
	double x_centro_molla1 = 0;
	//Coordinata y del punto in cui è ancorata la molla virtuale per la sfera 1
	double y_centro_molla1 = 0.20;
	//Coordinata x del punto in cui è ancorata la molla virtuale per la sfera 2
	double x_centro_molla2 = 0;
	//Coordinata y del punto in cui è ancorata la molla virtuale per la sfera 2
	double y_centro_molla2 = 0.20;
	//Angolazione dei motori usata per calcolare la jacobiana
	float  Theta1_reach, Theta4_reach;
	//Angoli letti dagli encoder, devono essere modificati riportandoli nel sistema di riferimento del manipolatore
	float theta1_reach, theta4_reach;

	/*Variabili usate nel contouring*/

	//Cooridnata x del centro del percorso usato nel contouring
	double x_centro_percorso = 0;
	//Cooridnata y del centro del percorso usato nel contouring
	double y_centro_percorso = 0.25;
	//dimensione della parete cilindrica interna del percorso di contouring
	double pathDimension;
	//larghezza del percorso, distanza tra le pareti cilindriche
	double pathWidth;
	//limit mi permette di capire se si deve considerare la molla che rappresenta il bordo interno
	//o quella che rappresenta quello esterno del contouring. In sostanza si divide il percorso in 2 parti uguali, se si
	//è nella parte esterna si usa la molla della parete esterna, se si è nella parte interna, si usa 
	//la molla della parete interna
	double limit;
	//lunghezza della molla che rappresenta la parete cilindrica interna
	double L_molla_interna_contouring;
	//lunghezza della molla che rappresenta la parete cilindrica esterna
	double L_molla_esterna_contouring;
	//Angoli dei motori usati per calcolare la jacobiana
	float  Theta1_cont, Theta4_cont;
	//Angoli letti dagli encoder, devono essere modificati riportandoli nel sistema di riferimento del manipolatore
	float theta1_cont, theta4_cont;

	/*Variabili usate sia per il reaching che per il contouring*/

	//Costante elastica della molla virtuale
	double k_molla = 30;
	//Coefficiente di attrito viscoso
	double b_molla = 0.3;
	//Lunghezza a riposo della molla virtuales
	double L_molla = 0.005;

	/*variabili usate per il damping*/

	//differenza temporale tra due campioni di posizione. Ottenuta da finish-start
	float time_between_samples;
	//variabile usata per tener traccia dell'istante in cui si ferma 
	//il cronometro che misura il tempo passato tra 2 campioni
	std::chrono::high_resolution_clock::time_point finish;
	//variabile usata per tener traccia dell'istante in cui si fa partire
	//il cronometro che misura il tempo passato tra 2 campioni
	std::chrono::high_resolution_clock::time_point start;
	//numero di campioni usati per la media mobile
	static const int averageSize = 20;
	//Forze di attrito viscoso
	float F_damping_X, F_damping_Y;
	//variabile per conservare la x per confrontarla con la x del campione successivo e calcolare la velocità
	float x_old_sample = 0;
	//variabile per conservare la y per confrontarla con la y del campione successivo e calcolare la velocità
	float y_old_sample = 0;
	//velocità calcolate come (campione-campione_precedente)/time_between_samples
	double velocitàX, velocitàY;
	//velocità sull'asse X uscente dal processo di media mobile
	double velocità_mediaX = 0;
	//velocità sull'asse Y uscente dal processo di media mobile
	double velocità_mediaY = 0;
	//array che contiene i valori di velocità sull'asse X utilizzati nella media mobile
	float velocità_arrayX[averageSize];
	//array che contiene i valori di velocità sull'asse Y utilizzati nella media mobile
	float velocità_arrayY[averageSize];
	//somma di tutti gli elementi contenuti nell'array velocità_arrayX
	float somma_elementi_velocità_arrayX = 0;
	//somma di tutti gli elementi contenuti nell'array velocità_arrayY
	float somma_elementi_velocità_arrayY = 0;

	/*Variabili per il chasing*/

	//Angoli dei motori usati per calcolare la jacobiana
	float  Theta1_chas, Theta4_chas;
	//Angoli letti dagli encoder, devono essere modificati riportandoli nel sistema di riferimento del manipolatore
	float theta1_chas, theta4_chas;
	//coordinata x del centro della circonferenza su cui si effettua il chasing
	double x_offset_circle_chasing;
	//coordinata y del centro della circonferenza su cui si effettua il chasing
	double y_offset_circle_chasing;
	//raggio della circonferenza su cui si effettua il chasing
	double radius_chasing;
	//tempo impiegato per compiere un giro della circonferenza
	double period_chasing;
	//Lunghezza della molla usata per il chasing
	double L_molla_chasing = 0.005;
	//k della molla virtuale usata per il chasing
	double k_molla_chasing = 2;

	/*Flag per l'inizializzazione della posizione*/
	
	//quando diventa 1 inidica che l'esercizio deve cominciare e lascia all'utente la possibilità di muovere l'organo terminale
	int flag_excercise_start = 0;

	/////!!!!Funzioni di prova!!!!/////

	/*Funzioni per far seguire all'organo terminale del manipolatore una traiettoria circolare
	funzionano da sole NON servono per l'interfaccia aptica*/

	//Genera le coordinate x,y che bisogna seguire per compiere un movimento circolare
	void GenerazioneCerchio(double raggio);
	//Trasforma un riferimento circolare in (x,y) in uno in (theta1,theta4) per controllare i motori
	void CinematicaInversaPosizionePerCerchio();														
	//Controlla il manipolatore in modo che l'organo terminale segua il cerchio generato
	void ControlCerchio(Sensoray626& board);																		

	/*variabili usate per percorrere una circonferenza con velocità e raggio variabile.
	il manipolatore segue un riferimento variabile su una circonferenza*/

	//periodo di percorrenza
	int period;
	//Frequenza di percorrenza del cerchio in RAD/S
	float freq_circle;
	//Distanza temporale tra due successivi punti di riferimento
	double sampling_time = 0.005;
	//coordinata x del centro della circonferenza
	double x_offset_circle;
	//coordinata y del centro della circonferenza
	double y_offset_circle;
	//raggio della circonferenza
	float radius;
	//coordinate x e y generate che costituiscono il riferimento da seguire
	float x_circle, y_circle;


	/*Funzioni DEMO per testare molle virtuali o l'esercizio di chasing solo usando il manipolatore.
	Non sono usate per l'interfaccia aptica*/

	//Genera una molla virtuale a cui è attaccato l'organo terminale
	void molla(Sensoray626& board);							
	//Funzione di prova per il chasing
	void chasing(Sensoray626& board, float raggio, float periodo);													

	/*Variabili usate nella funzione della molla semplice*/

	//Coordinata x del punto in cui è ancorata la molla virtuale
	double x_centro_molla = 0;
	//Coordinata y del punto in cui è ancorata la molla virtuale
	double y_centro_molla = 0.20;

private:

	/*Funzioni per calcolare la matrice Jacobiana*/

	//Funzione che traspone la matrice Jacobiana
	void transposeJacobian();	
	//Funzione che inverte una matrice 2x2
	void inverseMatrix(float A[2][2]);		
	//Funzione che calcola la matrice Jacobiana
	void calcolateJacobian(float inv_A[2][2], float B[2][2]);			

	/*Costanti utilizzate*/

	//Voltaggio dato inizialmente al motore 1(usato per impostare il sistema di riferimento)
	const double INITIAL_VOLTAGE1= -0.23;																
	//Voltaggio dato inizialmente al motore 2(usato per impostare il sistema di riferimento)
	const double INITIAL_VOLTAGE2= -0.28;															
	//Riferimento di posizione per portare i bracci a 45 gradi					
	const float RIFERIMENTO_45GRADI = M_PI / 4;																										
	
	//Costante di coppia del motore in Nm/A
	const double Kt = 0.14;																							

	//Dimensioni in metri dei bracci del manipolatore
	const double L0 = 0.085 / 2;
	const double L1 = 0.17;
	const double L2 = 0.218;
	const double L3 = 0.218;
	const double L4 = 0.17;

	//Variabili usate per la cinematica diretta di posizione(x e y sono le coordinate dell'organo terminale, Theta1 e Theta4 gli angoli dei motori)
	//tutte le altre variabili sono angoli e coordinate dei vertici non motorizzati del manipolatorefloat x, y, Theta1, Theta4;
	float x, y, Theta1, Theta4;
	float theta1, theta4,  Theta2, Theta3,  beta, epsilon, epsilon2;
	float x1, x2, x3, x4, y1, y2, y3, y4;
	float L23;
	float coordinate_X_Y[2];

	//variabili per la cinematica inversa di posizione
	//sono gli angoli Theta1  e Theta4 restituiti dalla funzione
	float angles[2];																							

	/*Variabili per la retroazione della posizione*/

	//posizione angolare in radianti letta dagli encoder
	float posizione[2];									
	//differenza tra riferimento e posizione letta
	float errore_posizione[2];																						
	//volt forniti in uscita
	float uscita_volt[2];																						

	/*Variabili per seguire un riferimento circolare creato offline*/

	//Numero di campioni che costituiscono una circonferenza
	float Samples_per_period;
	//coordinate x del cerchio da seguire
	std::vector<float> x_Cerchio;
	//coordinate y del cerchio da seguire
	std::vector<float> y_Cerchio;
	//coordinate theta1 del cerchio da seguire
	std::vector<float> Theta1_Cerchio;
	//coordinate theta4 del cerchio da seguire
	std::vector<float> Theta2_Cerchio;

	/*Variabili usate nel problema con la molla*/

	//matrice Jacobiana
	float jacobian_[2][2];																							
	//Jacobiana trasposta
	float transposedJacobian_[2][2];
	//Matrice parziale per costruire la Jacobiana
	float matrix_A[2][2];
	//Matrice parziale per costruire la Jacobiana
	float matrix_B[2][2];
	//Matrice inversa della matrice A, per costruire la Jacobiana
	float matrix_inv_A[2][2];
	//distanza dell'organo terminale punto in cui è ancorata la molla
	float distanza_centro;
	//allungamento della molla
	float delta;
	//angolazione della molla virtuale. E' ancorata ad uno dei suoi estrime e può ruotare attorno ad esso
	float pendenza;
	//Forza totale generata dalla molla e dalla componente viscosa, componente x della forza, componente y della forza
	float Forza, F_x, F_y;
	//Coppie che devono erogare i motori, correnti da fornire ai motori, tensione da scrivere sui DAC
	float Torque[2], current[2], voltage[2];
};

#endif