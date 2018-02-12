using UnityEngine;
using UnityEngine.UI;


//script per vedere il valore del periodo del chasing selezionato tramite uno slider. Nella scena in cui imposto l'esercizio
public class DisplaySliderValueCHASESPEED : MonoBehaviour {

    public Slider slide;

    private Text textComponent;

    // Use this for initialization
    void Start()
    {
        textComponent = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        textComponent.text = (4 + slide.value * 0.2).ToString() + " s";
    }
}
