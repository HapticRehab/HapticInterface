using UnityEngine;
using UnityEngine.UI;

//Script per visualizzare il valore selezionato con lo slider per la larghezza del percorso di contouring
public class DisplaySliderValuePATHWIDTH : MonoBehaviour {

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
        textComponent.text = (1.2 + slide.value * 0.2).ToString() + " cm";
    }
}
