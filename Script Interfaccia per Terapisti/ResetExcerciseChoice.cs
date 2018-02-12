using UnityEngine;
using UnityEngine.UI;

//Nel caso in cui si prema il pulsante "Back" nel menu di setup, viene riportato a 0 il valore del dropdown per la scelta dell'esercizio
//0 corrisponde a nessun esercizio selezionato
public class ResetExcerciseChoice : MonoBehaviour {

    public Dropdown exChoice;

    public void ResetChoice()
    {
        exChoice.value = 0;
    }
	
}
