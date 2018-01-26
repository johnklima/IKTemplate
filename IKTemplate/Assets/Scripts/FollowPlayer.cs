using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    public Transform player;
    public Transform CameraTargets;                     //list of world positions for camera distance
    public float lookRate = 3.0f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 camtarget = findNearestCameraTarget();

        //force the camera to follow the ship
        float z = player.position.z;
        float x = player.position.x  + 10  ;//camtarget.x;
        float y = player.position.y  + 4 ; //camtarget.y;

        Vector3 campos = new Vector3(x, y, z);

        float t = Time.deltaTime;
        campos = Vector3.Slerp(transform.position, campos, t * lookRate);
        campos.Set(campos.x, campos.y, z);
        transform.position = campos;
        
        pointAt(player.position);

    }
    public void pointAt(Vector3 target)
    {
        Quaternion a = transform.rotation;          //save it
        transform.LookAt(target);                   //look at target
        Quaternion b = transform.rotation;          //get new rotation
        transform.rotation = a;                     //set it back
        
       
        //spherical interpolate
        float t = Time.deltaTime;
        Quaternion c = Quaternion.Slerp(a, b, t * lookRate);

        transform.rotation = c;

    }


    Vector3 findNearestCameraTarget()
    {

        
        float epsilon = 100000.0f;
        Vector3 pos = new Vector3(0,0,0);

        foreach (Transform child in CameraTargets)
        {
            float distance = Vector3.Distance(transform.position, child.position);
            if (distance < epsilon)
            {
                epsilon = distance;
                pos = child.position;
            }
        }        

        return pos;
    }
}
