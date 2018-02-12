using UnityEngine;

public class DontDestroyCanvas : MonoBehaviour {

    public static DontDestroyCanvas playerInstance;
    
    //Si rende non distruttibile al cambio di scena il canvas che contiene il text per visualizzare lo stato di connessione dell'Hololens
    void Awake()
    {
        DontDestroyOnLoad(this);

        if (playerInstance == null)
        {
            playerInstance = this;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }
}
