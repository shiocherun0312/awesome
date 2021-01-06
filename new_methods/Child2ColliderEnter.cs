using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Child2ColliderEnter : MonoBehaviour
{
    public bool touchFlag2 = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
	{
        touchFlag2 = true;
		//Debug.Log("123");
	}
		
	void OnCollisionExit(Collision collision)
	{
        touchFlag2 = false;
		//Debug.Log("321");
	}

}
