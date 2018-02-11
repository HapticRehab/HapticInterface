using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnTimer : MonoBehaviour {

    public float time; //dopo quanto tempo cancello la traccia della posizione dell'organo terminale

	// Use this for initialization
	void Start () {
        Destroy(gameObject, time);	
	}
	
}
