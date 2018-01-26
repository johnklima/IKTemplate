using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JohnIdle : AnimationScript
{

    //we need to accumulate time so when the anim is disabled. it continues from where it left off
    private float accumTime = 0;
    public Transform target;
    
    
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        
        accumTime += Time.deltaTime;        
        float y = Mathf.Sin(accumTime * 4.0f) * 0.5f;

        //Do Something! this will bounce the player
        //target.transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);

    }
}
