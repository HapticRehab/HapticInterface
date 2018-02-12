using UnityEngine;
using UnityEngine.UI;

//Script per visualizzare il valore selezionato con un generico slider
public class DisplaySliderValue : MonoBehaviour {

    public Slider repetitionNumber;

    Text textComponent;

    void Start()
    {
        textComponent = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update () {
        textComponent.text = repetitionNumber.value.ToString();
	}
}
