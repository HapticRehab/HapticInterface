using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    //Funzione che carica la scena del menu partendo da quella dell'esercizio. Si modifica un flag per
    //indicare che è già stato svolto un esercizio ed è la seconda volta che si carica il menu.
    //In questo modo si può mostrare direttamente la schermata di impostazione dell'esercizio
    public void LoadByIndex(int sceneIndex)
    {
        MenuInitializer.flag_second_call_of_the_scene = 1;
        SceneManager.LoadScene(sceneIndex);
    }
}