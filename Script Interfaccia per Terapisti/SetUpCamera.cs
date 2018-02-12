using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Funzione per posizionare la telecamera a seconda dell'esercizio che si sta svolgendo
public class SetUpCamera : MonoBehaviour {

    private Camera cam;

	// Use this for initialization
	void Awake () {

        cam = gameObject.GetComponent<Camera>();
        if (SaveStateAndLoadScene._excercise!= 1)
        {
            cam.transform.position = new Vector3(0, 1, 0.25f);
            cam.orthographicSize = 0.15f;
        }		
	}

}
