using UnityEngine;

public class QuitOnClick : MonoBehaviour
{
    //Funzione per chiudere l'applicazione
    public void Quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit ();
    #endif
    }

}