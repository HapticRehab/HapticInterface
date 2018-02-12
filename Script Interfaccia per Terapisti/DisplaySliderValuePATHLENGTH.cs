using UnityEngine;
using UnityEngine.UI;

//Script per visualizzare il valore selezionato con lo slider per la lunghezza del percorso di contouring
public class DisplaySliderValuePATHLENGTH : MonoBehaviour {

    public Slider slide;

    Text textComponent;

    // Use this for initialization
    void Start () {
        textComponent = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        textComponent.text = (3+slide.value*0.2).ToString() + " cm";
    }
}
