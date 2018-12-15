using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uniduino.Examples;

public class TriggerEnter : MonoBehaviour {
    public UniduinoTestPanel _testArduino;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("Enter");
            StartCoroutine(_testArduino.RelayControll());

            //GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

}
