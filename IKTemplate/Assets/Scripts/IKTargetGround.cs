using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTargetGround : MonoBehaviour {


    private static Vector3 rayoffset = new Vector3(0, 3, 0);
    private static int GROUND_LAYER = 1 << 8;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame

	void Update () {
        float h = 1;
        RaycastHit hit;
        int layerMask = GROUND_LAYER;

        Vector3 raycastPoint = transform.position;
        raycastPoint += rayoffset;

        
        if (Physics.Raycast(raycastPoint, -Vector3.up, out hit, 100, layerMask))
        {

            h = hit.point.y;

            if(transform.position.y < h)
            {
                float y = transform.position.y;
                
                raycastPoint.Set(transform.position.x, h, transform.position.z);
                transform.position = raycastPoint;

                

            }
            

        }




    }
}
