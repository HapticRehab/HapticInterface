using UnityEngine;

//Script per rendere non distruttibile ad ogni cambio scena un GameObject
public class DontDestroy : MonoBehaviour {

    public static DontDestroy playerInstance;
	// Use this for initialization
	void Awake() {
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
