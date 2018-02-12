using UnityEngine;
using UnityEngine.UI;

//Script per visualizzare il valore della durata dell'esercizio selezionato con uno slider
public class DisplaySliderTIME : MonoBehaviour {

    public Slider slide;

    Text textComponent;

    // Use this for initialization
    void Start()
    {
        textComponent = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        textComponent.text = slide.value.ToString() + " s";
    }
}
