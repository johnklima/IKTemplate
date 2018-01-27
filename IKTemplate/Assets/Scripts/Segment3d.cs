using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Segment3d : MonoBehaviour
{
    public Vector3 Apos = new Vector3(0, 0, 0);
    public Vector3 Bpos = new Vector3(0, 0, 0);

    protected float length = 0;

    public Segment3d parent = null;
    public Segment3d child = null;

    public float interpRate = 3;

    private IKSystem3d parentSystem;    
    private float twist;

    public bool useInterpolation = true;
    public float extraX = 0;
    public float extraY = 0;
    public float extraZ = 0;

    public Vector3 maxRotation;
    public Vector3 minRotation;
    public Quaternion initialRotation;
    public float angleDifferenceX ;
    public float angleDifferenceY ;
    public float angleDifferenceZ;
    public float xt, yt, zt;

    //lets try a quat based solution
    public Quaternion maxQ;
    public Quaternion minQ;
    public float QMaxAngle;
    public float QMinAngle;

    void Awake()
    {
        //aquire the length of this segment - the dummy geometry will always be child zero
        length = transform.GetChild(0).localScale.z;
        parentSystem = transform.GetComponentInParent<IKSystem3d>();
        initialRotation = transform.localRotation;

    }

    public void updateSegmentAndChildren()
    {
        length = transform.GetChild(0).localScale.z;


        updateSegment();

        //update its children
        if (child)
            child.updateSegmentAndChildren();

        
    }

    public void updateSegment()
    {
               
        if (parent)
        {
            Apos = parent.Bpos;         //could also use parent endpoint...
            transform.position = Apos;  //move me to Bpos (parent endpoint)
        }
        else
        {
            //Apos is always my position
            Apos = transform.position;
        }

        //Bpos is always where the endpoint will be, as calculated from length 
        calculateBpos();
    }

    void calculateBpos()
    {   
        Bpos = Apos + transform.forward * length;
    }
    
    public void pointAt(Vector3 target)
    {

        maxQ = Quaternion.Euler(maxRotation);
        minQ = Quaternion.Euler(minRotation);

        Quaternion a = transform.localRotation;                 //save current local rotation       

        transform.LookAt(target);

        Quaternion b = transform.localRotation;

        transform.localRotation = a;


        float ang;
        Vector3 axis;

        Quaternion qx, qy, qz;

        //clamp on X axis
        b.ToAngleAxis(out ang, out axis);
        float ix = initialRotation.eulerAngles.x;
        axis.y = 0;
        axis.z = 0;
        axis.Normalize();
        ang = Mathf.Clamp(ang, ix + minRotation.x, ix + maxRotation.x);

        qx = Quaternion.AngleAxis(ang, axis);

        //clamp on Y axis
        b.ToAngleAxis(out ang, out axis);
        float iy = initialRotation.eulerAngles.y;
        axis.x = 0;
        axis.z = 0;
        axis.Normalize();
        ang = Mathf.Clamp(ang, iy + minRotation.y, iy + maxRotation.y);

        qy = Quaternion.AngleAxis(ang, axis);

        //clamp on z axis
        b.ToAngleAxis(out ang, out axis);
        float iz = initialRotation.eulerAngles.z;
        axis.x = 0;
        axis.y = 0;
        axis.Normalize();
        ang = Mathf.Clamp(ang, iz + minRotation.z, iz + maxRotation.z);

        qz = Quaternion.AngleAxis(ang, axis);

        b = qy * qz * qx;




        if (useInterpolation)
        {
            //if the system is in drag mode, we want to crank the interpolation
            //otherwise, the chain is "lazy," it doesnt need to do anything
            float ir = interpRate;
            if (parentSystem.isDragging)
                ir *= 10;

           
            //spherical interpolate
            float t = Time.deltaTime;
            Quaternion c = Quaternion.Slerp(a, b, t * ir);
            
            transform.localRotation = c;


        }
        else
        {
            transform.rotation = b;
        }



        //additional rotations on axis
        /*
        transform.Rotate(Vector3.left, extraX * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.forward, extraY * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.up, extraZ * Time.deltaTime, Space.Self);
        */

    }


    public void drag(Vector3 target)
    {
        pointAt(target);
        transform.position = target - transform.forward * length;

        if (parent)
            parent.drag(transform.position);


    }

    public void reach(Vector3 target)
    {
        drag(target);
        updateSegment();
    }
}
