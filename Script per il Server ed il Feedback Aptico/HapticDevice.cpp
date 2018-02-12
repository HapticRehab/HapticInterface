#include "stdafx.h"
#include "HapticDevice.h"

//Alla creazione dell'istanza della classe si inizializzano gli array necessari alla media mobile
HapticDevice::HapticDevice()
{
	for (int i = 0; i < averageSize; i++) {
		velocit‡_arrayX[i] = 0;
	}

	for (int i = 0; i < averageSize; i++) {
		velocit‡_arrayY[i] = 0;
	}

	start = std::chrono::high_resolution_clock::now();
}

HapticDevice::~HapticDevice()
{
}

void HapticDevice::setInitialCoordinates(Sensoray626& board,int channel1,int channel2) const {
	board.writeToDac(channel1, INITIAL_VOLTAGE1);
	board.writeToDac(channel2, INITIAL_VOLTAGE2);
	Sleep(4000);	//Si aspetta un po di tempo per permettere ai bracci del manipolatore di arrivare ai blocchi meccanici
	board.writeToDac(DAC1, 0);
	board.writeToDac(DAC2, 0);
}

void HapticDevice::anelloDiRetroazione(Sensoray626& board,float Riferimento_Motore1,float Riferimento_Motore2,double gain){
	
	posizione[0] = board.readEncoder(ENC1);
	posizione[1] = board.readEncoder(ENC2);

	errore_posizione[0] = Riferimento_Motore1 - posizione[0];
	errore_posizione[1] = Riferimento_Motore2 - posizione[1];

	uscita_volt[0] = gain*errore_posizione[0];
	uscita_volt[1] = gain*errore_posizione[1];

	board.writeToDac(DAC1, uscita_volt[0]);
	board.writeToDac(DAC2, uscita_volt[1]);
}

void HapticDevice::ControlTo45Degrees(Sensoray626& board){
	setInitialCoordinates(board, DAC1, DAC2);
	int i = 0;
	while (i<100000){
		anelloDiRetroazione(board,RIFERIMENTO_45GRADI,RIFERIMENTO_45GRADI,2);
		i++;
	}
}

void HapticDevice::CinematicaInversaPosizione(float x, float y) {
	float alfa1, alfa2, beta1, beta2;

	alfa1 = acos((pow(L1, 2) + (pow((L0 + x), 2) + pow(y, 2)) - pow(L2, 2)) / (2 * L1*sqrt(pow((L0 + x), 2) + pow(y, 2))));

	alfa2 = acos((pow(L1, 2) + (pow((L0 - x), 2) + pow(y, 2)) - pow(L2, 2)) / (2 * L1*sqrt(pow((L0 - x), 2) + pow(y, 2))));

	beta1 = atan2(y, (L0 + x));

	beta2 = atan2(y, (L0 - x));

	angles[0] = beta1 + alfa1 - (M_PI / 2);
	angles[1] = M_PI - beta2 - alfa2;

}

float* HapticDevice::CinematicaDirettaPosizione(Sensoray626& board) {

	theta1 = board.readEncoder(ENC1);
	theta4 = board.readEncoder(ENC2);

	Theta1 = theta1 + (M_PI / 2);
	Theta4 = theta4;

	x1 = -L0;
	y1 = 0;

	x4 = L0;
	y4 = 0;

	x2 = -L0 + L1*cos(Theta1);
	y2 = L1*sin(Theta1);

	x3 = L0 + L4*cos(Theta4);
	y3 = L4*sin(Theta4);

	L23 = sqrt(pow((x3 - x2), 2) + pow((y3 - y2), 2));

	beta = acos((pow(L23, 2) + pow(L2, 2) - pow(L3, 2)) / (2 * L23*L2));
	epsilon = atan2((y2 - y3), (x2 - x3));
	epsilon2 = atan2((y3 - y2), (x3 - x2));

	Theta3 = 2 * M_PI - (beta - epsilon);
	Theta2 = beta + epsilon2;

	x = x3 + L2*cos(Theta3);
	y = y3 + L2*sin(Theta3);

	coordinate_X_Y[0] = x;
	coordinate_X_Y[1] = y;

	return coordinate_X_Y;
}

void HapticDevice::inverseMatrix(float A[2][2]) {
	float determinante = 1 / ((A[0][0] * A[1][1]) - (A[0][1] * A[1][0]));
	matrix_inv_A[0][0] = determinante*A[1][1];
	matrix_inv_A[0][1] = -determinante*A[0][1];
	matrix_inv_A[1][0] = -determinante*A[1][0];
	matrix_inv_A[1][1] = determinante*A[0][0];
}

void HapticDevice::calcolateJacobian(float inv_A[2][2], float B[2][2]) {
	jacobian_[0][0] = inv_A[0][0] * B[0][0] + inv_A[0][1] * B[1][0];
	jacobian_[0][1] = inv_A[0][0] * B[0][1] + inv_A[0][1] * B[1][1];
	jacobian_[1][0] = inv_A[1][0] * B[0][0] + inv_A[1][1] * B[1][0];
	jacobian_[1][1] = inv_A[1][0] * B[0][1] + inv_A[1][1] * B[1][1];
}

void HapticDevice::transposeJacobian()
{
	for (int i = 0; i < 2; i++)
		for (int j = 0; j < 2; j++)
			transposedJacobian_[i][j] = jacobian_[j][i];
}

void HapticDevice::initializePosition(Sensoray626& board, float finalX, float finalY) {
	//guadagno di posizione
	double gain = 12;

	//coordinate cartesiane e angolari dei punti progressivi in cui si deve spostare l'organo terminale
	float futureX, futureY;
	float futureAngles[2];

	//sampling time[s]
	float campionamento = 0.001;

	//tempo totale in cui faccio il movimento[s]
	float total_time = 5;

	//faccio la cinematica diretta per capire dove sono
	float *initialX_Y;
	initialX_Y = CinematicaDirettaPosizione(board);

	//calcolo le distanze dal punto finale in x e y
	float x_distance = finalX - initialX_Y[0];
	float y_distance = finalY - initialX_Y[1];

	//lo step da compiere tra un riferimento e l'altro
	float x_step = x_distance / (total_time / campionamento);
	float y_step = y_distance / (total_time / campionamento);
	//myfile.is_open();

	for (int i = 0; i <= (total_time / campionamento); i++) {

		futureX = initialX_Y[0] + i*x_step;
		futureY = initialX_Y[1] + i*y_step;

		CinematicaInversaPosizione(futureX, futureY);
		futureAngles[0] = angles[0];
		futureAngles[1] = angles[1];

		anelloDiRetroazione(board, futureAngles[0], futureAngles[1], gain);
		Sleep(1);

	}

	//l'organo terminale Ë tenuto in una posizione fissa fino a quando non viene segnalato che l'esercizio deve cominciare
	while (flag_excercise_start == 0) {
		anelloDiRetroazione(board, futureAngles[0], futureAngles[1], gain);
		Sleep(1);
	}

	board.writeToDac(DAC1, 0);
	board.writeToDac(DAC2, 0);
}

void HapticDevice::molla_per_contatto_con_sfere(Sensoray626& board, float x, float y, int SphereID, int averageIndex) {
	//lettura degli encoder e generazione della jacobiana
	theta1_reach = board.readEncoder(ENC1);
	theta4_reach = board.readEncoder(ENC2);

	Theta1_reach = theta1_reach + (M_PI / 2);
	Theta4_reach = theta4_reach;

	matrix_A[0][0] = y*cos(Theta1_reach) - (x + L0)*sin(Theta1_reach);
	matrix_A[0][1] = 0;
	matrix_A[1][0] = 0;
	matrix_A[1][1] = y*cos(Theta4_reach) + (L0 - x)*sin(Theta4_reach);

	matrix_B[0][0] = x + L0 - L1*cos(Theta1_reach);
	matrix_B[0][1] = y - L1*sin(Theta1_reach);
	matrix_B[1][0] = x - L0 - L1*cos(Theta4_reach);
	matrix_B[1][1] = y - L1*sin(Theta4_reach);

	inverseMatrix(matrix_A);
	calcolateJacobian(matrix_inv_A, matrix_B);
	transposeJacobian();

	//parte per il damping, con media mobile
	finish = std::chrono::high_resolution_clock::now();

	time_between_samples = (std::chrono::duration_cast<std::chrono::microseconds>(finish - start).count())*pow(10, (-6));

	velocit‡X = (x - x_old_sample) / time_between_samples;

	if (velocit‡X > 30 || velocit‡X<-30) {
		velocit‡X = 0;
	}

	somma_elementi_velocit‡_arrayX -= velocit‡_arrayX[averageIndex];

	velocit‡_arrayX[averageIndex] = velocit‡X;

	somma_elementi_velocit‡_arrayX += velocit‡_arrayX[averageIndex];

	velocit‡_mediaX = somma_elementi_velocit‡_arrayX / averageSize;

	velocit‡Y = (y - y_old_sample) / time_between_samples;

	if (velocit‡Y > 30 || velocit‡Y<-30) {
		velocit‡Y = 0;
	}

	somma_elementi_velocit‡_arrayY -= velocit‡_arrayY[averageIndex];

	velocit‡_arrayY[averageIndex] = velocit‡Y;

	somma_elementi_velocit‡_arrayY += velocit‡_arrayY[averageIndex];

	velocit‡_mediaY = somma_elementi_velocit‡_arrayY / averageSize;

	x_old_sample = x;
	y_old_sample = y;

	velocit‡_mediaX = (((int)(velocit‡_mediaX * 100)) / 100.0);
	velocit‡_mediaY = (((int)(velocit‡_mediaY * 100)) / 100.0);

	start = std::chrono::high_resolution_clock::now();

	//Si deve capire che sfera si sta toccando prima di generare la forza
	switch (SphereID)
	{

	case(1)://Sfera 1

		distanza_centro = sqrt(pow((x - x_centro_molla1), 2) + pow((y - y_centro_molla1), 2));
		delta = distanza_centro - L_molla;

		//Se la molla Ë compressa
		if (delta < 0) {
			Forza = -k_molla*delta;

			pendenza = atan2((y - y_centro_molla1), (x - x_centro_molla1));
			F_x = Forza*cos(pendenza);
			F_y = Forza*sin(pendenza);

			F_damping_X = -0.05*velocit‡_mediaX;
			F_damping_Y = -0.05*velocit‡_mediaY;

			F_x = F_x + F_damping_X;
			F_y = F_y + F_damping_Y;

			Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
			Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

			current[0] = Torque[0] / Kt;
			current[1] = Torque[1] / Kt;

			for (int i = 0; i < 2; i++) {
				if (current[i] > 6 || current[i] < -6) {
					std::cout << "SATURA" << std::endl;
					if (current[i] > 0) {
						current[i] = 6;
					}
					else {
						current[i] = -6;
					}
				}
			}

			voltage[0] = current[0] * 0.5;
			voltage[1] = current[1] * 0.5;

			board.writeToDac(DAC1, voltage[0]);
			board.writeToDac(DAC2, voltage[1]);
		}
		else {
			board.writeToDac(DAC1, 0);
			board.writeToDac(DAC2, 0);
		}
		break;

	case(2): //Sfera 2

		distanza_centro = sqrt(pow((x - x_centro_molla2), 2) + pow((y - y_centro_molla2), 2));
		delta = distanza_centro - L_molla;

		//Se la molla Ë compressa
		if (delta < 0) {
			Forza = -k_molla*delta;

			pendenza = atan2((y - y_centro_molla2), (x - x_centro_molla2));
			F_x = Forza*cos(pendenza);
			F_y = Forza*sin(pendenza);

			F_damping_X = -0.05*velocit‡_mediaX;
			F_damping_Y = -0.05*velocit‡_mediaY;

			F_x = F_x + F_damping_X;
			F_y = F_y + F_damping_Y;

			Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
			Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

			current[0] = Torque[0] / Kt;
			current[1] = Torque[1] / Kt;

			for (int i = 0; i < 2; i++) {
				if (current[i] > 6 || current[i] < -6) {
					std::cout << "SATURA" << std::endl;
					if (current[i] > 0) {
						current[i] = 6;
					}
					else {
						current[i] = -6;
					}
				}
			}

			voltage[0] = current[0] * 0.5;
			voltage[1] = current[1] * 0.5;

			board.writeToDac(DAC1, voltage[0]);
			board.writeToDac(DAC2, voltage[1]);
		}
		else {
			board.writeToDac(DAC1, 0);
			board.writeToDac(DAC2, 0);
		}
		break;
	}
}

void HapticDevice::molla_per_CONTOURING(Sensoray626& board, float x, float y, int averageIndex) {
	//lettura degli encoder e generazione della jacobiana
	theta1_cont = board.readEncoder(ENC1);
	theta4_cont = board.readEncoder(ENC2);

	Theta1_cont = theta1_cont + (M_PI / 2);
	Theta4_cont = theta4_cont;

	matrix_A[0][0] = y*cos(Theta1_cont) - (x + L0)*sin(Theta1_cont);
	matrix_A[0][1] = 0;
	matrix_A[1][0] = 0;
	matrix_A[1][1] = y*cos(Theta4_cont) + (L0 - x)*sin(Theta4_cont);

	matrix_B[0][0] = x + L0 - L1*cos(Theta1_cont);
	matrix_B[0][1] = y - L1*sin(Theta1_cont);
	matrix_B[1][0] = x - L0 - L1*cos(Theta4_cont);
	matrix_B[1][1] = y - L1*sin(Theta4_cont);

	inverseMatrix(matrix_A);
	calcolateJacobian(matrix_inv_A, matrix_B);
	transposeJacobian();

	//parte per il damping, con media mobile
	finish = std::chrono::high_resolution_clock::now();

	time_between_samples = (std::chrono::duration_cast<std::chrono::microseconds>(finish - start).count())*pow(10, (-6));

	velocit‡X = (x - x_old_sample) / time_between_samples;

	if (velocit‡X > 30 || velocit‡X<-30) {
		velocit‡X = 0;
	}

	somma_elementi_velocit‡_arrayX -= velocit‡_arrayX[averageIndex];

	velocit‡_arrayX[averageIndex] = velocit‡X;

	somma_elementi_velocit‡_arrayX += velocit‡_arrayX[averageIndex];

	velocit‡_mediaX = somma_elementi_velocit‡_arrayX / averageSize;

	velocit‡Y = (y - y_old_sample) / time_between_samples;

	if (velocit‡Y > 30 || velocit‡Y<-30) {
		velocit‡Y = 0;
	}

	somma_elementi_velocit‡_arrayY -= velocit‡_arrayY[averageIndex];

	velocit‡_arrayY[averageIndex] = velocit‡Y;

	somma_elementi_velocit‡_arrayY += velocit‡_arrayY[averageIndex];

	velocit‡_mediaY = somma_elementi_velocit‡_arrayY / averageSize;

	x_old_sample = x;
	y_old_sample = y;

	velocit‡_mediaX = (((int)(velocit‡_mediaX * 100)) / 100.0);
	velocit‡_mediaY = (((int)(velocit‡_mediaY * 100)) / 100.0);

	start = std::chrono::high_resolution_clock::now();

	distanza_centro = sqrt(pow((x - x_centro_percorso), 2) + pow((y - y_centro_percorso), 2));


	//Se l'organo terminale si trova nella parte interna del percorso allora si deve usare la molla pi˘ interna 
	if (distanza_centro < limit) {

		delta = distanza_centro - L_molla_interna_contouring;
		//Se la molla Ë compressa
		if (delta < 0) {
			Forza = -k_molla*delta;

			pendenza = atan2((y - y_centro_percorso), (x - x_centro_percorso));
			F_x = Forza*cos(pendenza);
			F_y = Forza*sin(pendenza);

			F_damping_X = -0.05*velocit‡_mediaX;
			F_damping_Y = -0.05*velocit‡_mediaY;

			F_x = F_x + F_damping_X;
			F_y = F_y + F_damping_Y;

			Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
			Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

			current[0] = Torque[0] / Kt;
			current[1] = Torque[1] / Kt;

			for (int i = 0; i < 2; i++) {
				if (current[i] > 6 || current[i] < -6) {
					if (current[i] > 0) {
						current[i] = 6;
					}
					else {
						current[i] = -6;
					}
				}
			}

			voltage[0] = current[0] * 0.5;
			voltage[1] = current[1] * 0.5;

			board.writeToDac(DAC1, voltage[0]);
			board.writeToDac(DAC2, voltage[1]);
		}
		else {
			
			board.writeToDac(DAC1, 0);
			board.writeToDac(DAC2, 0);
		}
	}
	else {	//l'organo terminale si trova nella parte esterna del percorso

		delta = distanza_centro - L_molla_esterna_contouring;
		//Se la molla Ë estesa
		if (delta > 0) {
			Forza = -k_molla*delta;

			pendenza = atan2((y - y_centro_percorso), (x - x_centro_percorso));
			F_x = Forza*cos(pendenza);
			F_y = Forza*sin(pendenza);

			F_damping_X = -0.05*velocit‡_mediaX;
			F_damping_Y = -0.05*velocit‡_mediaY;

			F_x = F_x + F_damping_X;
			F_y = F_y + F_damping_Y;

			Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
			Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

			current[0] = Torque[0] / Kt;
			current[1] = Torque[1] / Kt;

			for (int i = 0; i < 2; i++) {
				if (current[i] > 6 || current[i] < -6) {
					if (current[i] > 0) {
						current[i] = 6;
					}
					else {
						current[i] = -6;
					}
				}
			}

			voltage[0] = current[0] * 0.5;
			voltage[1] = current[1] * 0.5;

			board.writeToDac(DAC1, voltage[0]);
			board.writeToDac(DAC2, voltage[1]);
		}
		else {
			
			board.writeToDac(DAC1, 0);
			board.writeToDac(DAC2, 0);
		}
	}
}

void HapticDevice::molla_per_CHASING(Sensoray626& board, float x, float y, float x_molla, float y_molla, int averageIndex) {
	//lettura degli encoder e generazione della jacobiana
	theta1_chas = board.readEncoder(ENC1);
	theta4_chas = board.readEncoder(ENC2);

	Theta1_chas = theta1_chas + (M_PI / 2);
	Theta4_chas = theta4_chas;

	matrix_A[0][0] = y*cos(Theta1_chas) - (x + L0)*sin(Theta1_chas);
	matrix_A[0][1] = 0;
	matrix_A[1][0] = 0;
	matrix_A[1][1] = y*cos(Theta4_chas) + (L0 - x)*sin(Theta4_chas);

	matrix_B[0][0] = x + L0 - L1*cos(Theta1_chas);
	matrix_B[0][1] = y - L1*sin(Theta1_chas);
	matrix_B[1][0] = x - L0 - L1*cos(Theta4_chas);
	matrix_B[1][1] = y - L1*sin(Theta4_chas);

	inverseMatrix(matrix_A);
	calcolateJacobian(matrix_inv_A, matrix_B);
	transposeJacobian();

	//parte per la molla
	distanza_centro = sqrt(pow((x - x_molla), 2) + pow((y - y_molla), 2));
	delta = distanza_centro - L_molla_chasing;

	//parte per il damping, con media mobile
	finish = std::chrono::high_resolution_clock::now();

	time_between_samples = (std::chrono::duration_cast<std::chrono::microseconds>(finish - start).count())*pow(10, (-6));

	velocit‡X = (x - x_old_sample) / time_between_samples;

	if (velocit‡X > 30 || velocit‡X<-30) {
		velocit‡X = 0;
	}

	somma_elementi_velocit‡_arrayX -= velocit‡_arrayX[averageIndex];

	velocit‡_arrayX[averageIndex] = velocit‡X;

	somma_elementi_velocit‡_arrayX += velocit‡_arrayX[averageIndex];

	velocit‡_mediaX = somma_elementi_velocit‡_arrayX / averageSize;

	velocit‡Y = (y - y_old_sample) / time_between_samples;

	if (velocit‡Y > 30 || velocit‡Y<-30) {
		velocit‡Y = 0;
	}

	somma_elementi_velocit‡_arrayY -= velocit‡_arrayY[averageIndex];

	velocit‡_arrayY[averageIndex] = velocit‡Y;

	somma_elementi_velocit‡_arrayY += velocit‡_arrayY[averageIndex];

	velocit‡_mediaY = somma_elementi_velocit‡_arrayY / averageSize;

	x_old_sample = x;
	y_old_sample = y;

	velocit‡_mediaX = (((int)(velocit‡_mediaX * 100)) / 100.0);
	velocit‡_mediaY = (((int)(velocit‡_mediaY * 100)) / 100.0);

	start = std::chrono::high_resolution_clock::now();

	//applicazione della forza
	//Se la molla Ë estesa
	if (delta > 0) {
		Forza = -k_molla_chasing*delta;

		pendenza = atan2((y - y_molla), (x - x_molla));
		F_x = Forza*cos(pendenza);
		F_y = Forza*sin(pendenza);

		F_damping_X = -0.05*velocit‡_mediaX;
		F_damping_Y = -0.05*velocit‡_mediaY;

		F_x = F_x + F_damping_X;
		F_y = F_y + F_damping_Y;

		Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
		Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

		current[0] = Torque[0] / Kt;
		current[1] = Torque[1] / Kt;

		for (int i = 0; i < 2; i++) {
			if (current[i] > 6 || current[i] < -6) {
				if (current[i] > 0) {
					current[i] = 6;
				}
				else {
					current[i] = -6;
				}
			}
		}

		voltage[0] = current[0] * 0.5;
		voltage[1] = current[1] * 0.5;

		board.writeToDac(DAC1, voltage[0]);
		board.writeToDac(DAC2, voltage[1]);
	}
	else {
		board.writeToDac(DAC1, 0);
		board.writeToDac(DAC2, 0);
	}
}

void HapticDevice::cleanMovingAverage() {

	x_old_sample = 0;
	y_old_sample = 0;

	somma_elementi_velocit‡_arrayX = 0;
	somma_elementi_velocit‡_arrayY = 0;

	for (int i = 0; i < averageSize; i++) {
		velocit‡_arrayX[i] = 0;
	}

	for (int i = 0; i < averageSize; i++) {
		velocit‡_arrayY[i] = 0;
	}
}

/*FUNZIONI DEMO*/

//Funzioni per far seguire al manipolatore una traiettoria circolare
void HapticDevice::GenerazioneCerchio(double raggio){
	//Si crea un riferimento circolare e i punti di riferimento vengono posti in due vector
	period=50;
	freq_circle= ((2 * M_PI) / period);
	Samples_per_period = (2 * M_PI) / (freq_circle*sampling_time);
	x_offset_circle = 0;
	y_offset_circle = 0.2653;

	for (double i = 0; i < Samples_per_period; i ++){
		x_Cerchio.push_back(x_offset_circle+raggio*sin(freq_circle*i*sampling_time));
		y_Cerchio.push_back(y_offset_circle+raggio*cos(freq_circle*i*sampling_time));
	}
}

void HapticDevice::CinematicaInversaPosizionePerCerchio(){
	float xf, yf,alfa1,alfa2,beta1,beta2;
	for (int i = 0; i < x_Cerchio.size(); i++){
		xf = x_Cerchio.at(i);
		yf = y_Cerchio.at(i);

		alfa1 = acos((pow(L1,2) + (pow((L0 + xf),2) + pow(yf, 2)) - pow(L2,2)) / (2 * L1*sqrt(pow((L0 + xf), 2) + pow(yf,2))));

		alfa2 = acos((pow(L1,2) + (pow((L0 - xf),2) + pow(yf,2)) - pow(L2, 2)) / (2 * L1*sqrt(pow((L0 - xf), 2) + pow(yf,2))));

		beta1 = atan2(yf , (L0 + xf));

		beta2 = atan2(yf , (L0 - xf));

		Theta1_Cerchio.push_back(beta1 + alfa1-(M_PI/2));
		Theta2_Cerchio.push_back(M_PI - beta2 - alfa2);
	}
}

void HapticDevice::ControlCerchio(Sensoray626& board){
	struct timeval tvBefore, tvAfter;
	struct _timeb timebuffer;
	std::ofstream myfile("example.txt");

	while (1){
		ControlTo45Degrees(board);
		int i=0;
		myfile.is_open();
		while (1){
			
			i = (i % Theta1_Cerchio.size());
			if (i == 0)
			{
				_ftime_s(&timebuffer);
				tvBefore.tv_sec = timebuffer.time;
				tvBefore.tv_usec = timebuffer.millitm;
				myfile << "START " << tvBefore.tv_sec << " " << tvBefore.tv_usec << std::endl;
			}

			anelloDiRetroazione(board, Theta1_Cerchio.at(i), Theta2_Cerchio.at(i),2);			
			i++;
			
			Sleep(5);
			
			if (i == Theta1_Cerchio.size())
			{
				_ftime_s(&timebuffer);
				tvAfter.tv_sec = timebuffer.time;
				tvAfter.tv_usec = timebuffer.millitm;
				myfile << "STOP " << tvAfter.tv_sec << " " << tvAfter.tv_usec << std::endl;
			}
		}
	}
}

//Funzioni per testare una molla virtuale e l'esrcizio di chasing usando il solo manipolatore
void HapticDevice::molla(Sensoray626& board){
	ControlTo45Degrees(board);
	//Si puÚ usare questo file .txt per scrivere dei valori durante l'esecuzione della funzione
	//senza causare rallentamenti che si avrebbero con un semplice cout
	struct _timeb timebuffer;
	std::ofstream myfile("example.txt");
	myfile.is_open();
	std::chrono::high_resolution_clock::time_point finish;
	std::chrono::high_resolution_clock::time_point start;	
	float vel;

	const int averageSize = 20;

	int samples_diff_ZERO_X=0;
	float velocit‡_arrayX[averageSize];
	float velocit‡_mediaX = 0;
	float mediaX=0;

	int samples_diff_ZERO_Y = 0;
	float velocit‡_arrayY[averageSize];
	float velocit‡_mediaY = 0;
	float mediaY=0;
	
	for (int i = 0; i < averageSize; i++) {
		velocit‡_arrayX[i] = 0;
	}

	for (int i = 0; i < averageSize; i++) {
		velocit‡_arrayY[i] = 0;
	}

	start= std::chrono::high_resolution_clock::now();
	int i = 0;
	while (1){
		CinematicaDirettaPosizione(board);
		matrix_A[0][0] = y*cos(Theta1) - (x + L0)*sin(Theta1);
		matrix_A[0][1] = 0;
		matrix_A[1][0] = 0;
		matrix_A[1][1] = y*cos(Theta4) + (L0 - x)*sin(Theta4);

		matrix_B[0][0] = x + L0 - L1*cos(Theta1);
		matrix_B[0][1] = y - L1*sin(Theta1);
		matrix_B[1][0] = x - L0 - L1*cos(Theta4);
		matrix_B[1][1] = y - L1*sin(Theta4);

		inverseMatrix(matrix_A);
		calcolateJacobian(matrix_inv_A, matrix_B);
		transposeJacobian();

		distanza_centro = sqrt(pow((x - x_centro_molla), 2) + pow((y - y_centro_molla), 2));
		delta = distanza_centro - L_molla;
		
		//Calcolo della velocit‡ con la media mobile
		finish = std::chrono::high_resolution_clock::now();
		
		time_between_samples= (std::chrono::duration_cast<std::chrono::microseconds>(finish-start).count())*pow(10,(-6));

		velocit‡X = (x - x_old_sample) / time_between_samples;
		
		if (velocit‡X > 30 || velocit‡X<-30) {
			velocit‡X = 0;
		}

		mediaX -= velocit‡_arrayX[i];
		
		velocit‡_arrayX[i] = velocit‡X;

		mediaX += velocit‡_arrayX[i];

		velocit‡_mediaX = mediaX / averageSize;

		velocit‡Y = (y - y_old_sample) / time_between_samples;

		if (velocit‡Y > 30 || velocit‡Y<-30) {
			velocit‡Y = 0;
		}

		mediaY -= velocit‡_arrayY[i];

		velocit‡_arrayY[i] = velocit‡Y;

		mediaY += velocit‡_arrayY[i];

		velocit‡_mediaY = mediaY / averageSize;

		i = (i + 1) % averageSize;

		//Si tengono solamente due cifre dopo la virgola per diminuire il rumore
		velocit‡_mediaX = (((int)(velocit‡_mediaX * 100)) / 100.0);
		velocit‡_mediaY = (((int)(velocit‡_mediaY * 100)) / 100.0);

		x_old_sample = x;
		y_old_sample = y;

		start = std::chrono::high_resolution_clock::now();

		
		//Generazione delle forze, solo se la molla Ë compressa
		if  (delta <0) {
			Forza = -9*delta;

			pendenza = atan2((y - y_centro_molla), (x - x_centro_molla));
			F_x = Forza*cos(pendenza);
			F_y = Forza*sin(pendenza);

			F_damping_X = -0.05*velocit‡_mediaX;
			F_damping_Y = -0.05*velocit‡_mediaY;

			F_x = F_x + F_damping_X;
			F_y = F_y + F_damping_Y;

			Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
			Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

			current[0] = Torque[0] / Kt;
			current[1] = Torque[1] / Kt;

			for (int i = 0; i < 2; i++) {
				if (current[i] > 6 || current[i] < -6) {
					if (current[i] > 0) {
						current[i] = 6;
					}
					else {
						current[i] = -6;
					}
				}
			}

			voltage[0] = current[0] * 0.5;
			voltage[1] = current[1] * 0.5;

			board.writeToDac(DAC1, voltage[0]);
			board.writeToDac(DAC2, voltage[1]);
		}
		else {
			board.writeToDac(DAC1, 0);
			board.writeToDac(DAC2, 0);
		}
	}
}

void HapticDevice::chasing(Sensoray626& board, float raggio, float periodo) {
	struct timeval tvBefore, tvAfter;
	struct _timeb timebuffer;
	std::ofstream myfile("example.txt");
	//Si puÚ usare questo file .txt per scrivere dei valori durante l'esecuzione della funzione
	//senza causare rallentamenti che si avrebbero con un semplice cout
	float alfa1, alfa2, beta1, beta2;
	float* xy;
	float x, y;
	int i = 0;

	ControlTo45Degrees(board);
	myfile.is_open();

	period = periodo;
	freq_circle = ((2 * M_PI) / period);
	Samples_per_period = (2 * M_PI) / (freq_circle*sampling_time);
	radius = raggio;
	x_offset_circle = 0;
	y_offset_circle = 0.25;

	while (1) {
		freq_circle = ((2 * M_PI) / period);
		Samples_per_period = (2 * M_PI) / (freq_circle*sampling_time);
		//Si controlla se il tempo impiegato per fare un giro Ë effettivamente quello impostato
		if (i == 0)
		{
			_ftime_s(&timebuffer);
			tvBefore.tv_sec = timebuffer.time;
			tvBefore.tv_usec = timebuffer.millitm;
			myfile << "START " << tvBefore.tv_sec << " " << tvBefore.tv_usec << std::endl;
		}

		x_circle = x_offset_circle + radius*sin(freq_circle*i*sampling_time);
		y_circle = (y_offset_circle + radius*cos(freq_circle*i*sampling_time));
		if (freq_circle*i*sampling_time > 2 * M_PI) {
			i = 0;
			_ftime_s(&timebuffer);
			tvAfter.tv_sec = timebuffer.time;
			tvAfter.tv_usec = timebuffer.millitm;
			myfile << "STOP " << tvAfter.tv_sec << " " << tvAfter.tv_usec << std::endl;
		}

		xy = CinematicaDirettaPosizione(board);
		x = xy[0];
		y = xy[1];

		theta1 = board.readEncoder(ENC1);
		theta4 = board.readEncoder(ENC2);

		Theta1 = theta1 + (M_PI / 2);
		Theta4 = theta4;

		matrix_A[0][0] = y*cos(Theta1) - (x + L0)*sin(Theta1);
		matrix_A[0][1] = 0;
		matrix_A[1][0] = 0;
		matrix_A[1][1] = y*cos(Theta4) + (L0 - x)*sin(Theta4);

		matrix_B[0][0] = x + L0 - L1*cos(Theta1);
		matrix_B[0][1] = y - L1*sin(Theta1);
		matrix_B[1][0] = x - L0 - L1*cos(Theta4);
		matrix_B[1][1] = y - L1*sin(Theta4);

		inverseMatrix(matrix_A);
		calcolateJacobian(matrix_inv_A, matrix_B);
		transposeJacobian();

		distanza_centro = sqrt(pow((x - x_circle), 2) + pow((y - y_circle), 2));

		delta = distanza_centro - L_molla_chasing;
		if (delta > 0) {
			Forza = -k_molla_chasing*delta;

			pendenza = atan2((y - y_circle), (x - x_circle));
			F_x = Forza*cos(pendenza);
			F_y = Forza*sin(pendenza);

			Torque[0] = transposedJacobian_[0][0] * F_x + transposedJacobian_[0][1] * F_y;
			Torque[1] = transposedJacobian_[1][0] * F_x + transposedJacobian_[1][1] * F_y;

			current[0] = Torque[0] / Kt;
			current[1] = Torque[1] / Kt;

			for (int i = 0; i < 2; i++) {
				if (current[i] > 6 || current[i] < -6) {
					if (current[i] > 0) {
						current[i] = 6;
					}
					else {
						current[i] = -6;
					}
				}
			}

			voltage[0] = current[0] * 0.5;
			voltage[1] = current[1] * 0.5;

			board.writeToDac(DAC1, voltage[0]);
			board.writeToDac(DAC2, voltage[1]);
		}
		else {
			board.writeToDac(DAC1, 0);
			board.writeToDac(DAC2, 0);
		}
		i++;
		Sleep(5);
	}
}




