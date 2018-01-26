using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JohnWalking : AnimationScript
{

    //we need to accumulate time so when the anim is disabled. it continues from where it left off
    //TODO: ultimately we want to transition between states and animations
    private float accumTime = 0;

    public Transform target;

    // Use this for initialization
    void Start () {
		
    }
	
    // Update is called once per frame
    void Update ()
    {

        //move the target ball back and forth
        accumTime += Time.deltaTime;
        float z = Mathf.Sin(accumTime * 4.0f);

        target.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
		
    }
}
