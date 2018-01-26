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
    private Vector3 initialRotation;
    private float twist;

    public bool useInterpolation = true;
    public float constrainX = 0;
    public float constrainY = 0;
    public float constrainZ = 0;

    void Awake()
    {
        //aquire the length of this segment - the dummy geometry will always be child zero
        length = transform.GetChild(0).localScale.z;
        parentSystem = transform.GetComponentInParent<IKSystem3d>();
        initialRotation = transform.GetChild(1).rotation.eulerAngles;
        
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
        Quaternion a = transform.localRotation;          //save local rotation
        transform.LookAt(target);                        //look at target
        Quaternion b = transform.localRotation;          //get new local rotation
        

        if (useInterpolation)
        {
            //if the system is in drag mode, we want to crank the interpolation
            //otherwise, the chain is "lazy," it doesnt need to do anything
            float ir = interpRate;
            if (parentSystem.isDragging)
                ir *= 10;

            Vector3 sysright = parentSystem.transform.right;
            Vector3 segright = transform.right;

            Vector3 sysfwd = parentSystem.transform.forward;
            Vector3 segfwd = transform.forward;

            //get an alignment to the parent transform (the ik system itself)
            float aZ = Vector3.SignedAngle(segright, sysright, Vector3.up);

            Vector3 euler = b.eulerAngles;


            float x = euler.x;
            float z = euler.z;
            float y = euler.y;

            float xt = x;
            float yt = y;
            float zt = z;

            //convert to +/- rangable values (not used, maybe later for clamping)
            if (xt > 180)
                xt -= 360;

            if (yt > 180)
                yt -= 360;

            if (zt > 180)
                zt -= 360;

            euler.Set( x ,
                       y,
                       z + aZ );


            b.eulerAngles = euler;

            transform.localRotation = a;                     //set it back to initial

            //spherical interpolate
            float t = Time.deltaTime;
            Quaternion c = Quaternion.Slerp(a, b, t * ir);

            transform.localRotation = c;

        }
        else
        {
            transform.localRotation = b;
        }



        //additional rotations on axis
        /*
        transform.Rotate(Vector3.left, constrainX * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.forward, constrainY * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.up, constrainZ * Time.deltaTime, Space.Self);
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
