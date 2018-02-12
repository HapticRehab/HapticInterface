using UnityEngine;
using UnityEngine.UI;

//script per vedere il valore del periodo del chasing selezionato tramite uno slider, durante l'esecuzione dell'esercizio
public class DisplaySliderValueCHASESPEEDDURINGEXCERCISE : MonoBehaviour {

    private GameObject slide;
    private Text textComponent;
    private Slider s;

    private void Awake()
    {
        slide = GameObject.Find("ChaseSpeedSlider(duringExcercise)(Clone)");
        s = slide.GetComponent<Slider>();
        textComponent = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        textComponent.text = (4 + s.value * 0.2).ToString() + " s";
    }
}
