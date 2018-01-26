using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTargetLegRight : MonoBehaviour {


    Vector3 deltaT = new Vector3();
    Vector3 curT = new Vector3();
    float localTime = 0;
    
    // Use this for initialization
    void Start ()
    {
        deltaT = transform.localPosition;
        localTime = 0;
    }
	
	// Update is called once per frame
	void Update () {

        float z = Mathf.Sin(localTime) * 2f;
        curT.Set(transform.localPosition.x, transform.localPosition.y, deltaT.z + z);

        transform.localPosition = curT;

        //accumulate our own local time to this object
        localTime += Time.deltaTime;
	}
}
