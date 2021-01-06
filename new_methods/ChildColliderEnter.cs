using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildColliderEnter : MonoBehaviour
{
    public bool touchFlag1 = false;
    bool touchFlag2;
    public Child2ColliderEnter c2;
    handRSP script;
    GameObject text;  
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        touchFlag2 = c2.touchFlag2;
        text = GameObject.Find("Cube");
        script = text.GetComponent<handRSP>();
    }

    // Update is called once per frame
    void Update()
    {
      if(touchFlag1 && touchFlag2){
        GameObject.Find("Cube").transform.position　= transform.position;
        //GameObject.Find("Cube").GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
      }
    }


  void OnCollisionEnter(Collision collision)
	{
       if(!script.flag3) touchFlag1 = true;
       else touchFlag1 = false;
	}
		
	void OnCollisionExit(Collision collision)
	{
        touchFlag1 = false;
	}

}
