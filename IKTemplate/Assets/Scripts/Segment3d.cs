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
    public bool useConstraints = true;
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

        //update its children (from toe to hips)
        if (child)
            child.updateSegmentAndChildren();

        updateSegment();

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

        transform.LookAt(target);                               //look at the target point

        Quaternion b = transform.localRotation;                 //get that new rotation

        transform.localRotation = a;                            //set the rotation back



        /*
         * CONSTRAINTS:
         * the idea here is to get each x,y,z rotation component as a direction, and a "twist" around that
         * direction, comparing it to min/max values for each axis, and clamping the rotation to prevent
         * it from exceeding what would be considered "normal" human motion.
         * 
         */
        if (useConstraints)
        {
            //we are looking for an axis and an angle
            float ang;
            Vector3 axis;
            //we will accumulate 3 rotations, one per axis
            Quaternion qx, qy, qz;

            //clamp on X axis - the most important axis for a human
            b.ToAngleAxis(out ang, out axis);               //what is our target rotation
            float ix = initialRotation.eulerAngles.x;       //what is our "start" rotation, neutral pose
            axis.y = 0;                                     //remove y and z from the direction, just the x please
            axis.z = 0;
            axis.Normalize();                               //make sure it is a "unit" vector

            //clamp our current angle to range min and max from our neutral pose e.g. thigh can rotate
            //+-60 deg on x from the initial rotation
            ang = Mathf.Clamp(ang, ix + minRotation.x, ix + maxRotation.x);
            //and convert to a quat rotation
            qx = Quaternion.AngleAxis(ang, axis);

            //clamp on Y axis - etc...
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

            //order here is critical, quats are not mathematically commutive
            b = qx * qz * qy;
        }



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

        //additional rotations on axis useful for tweaking and can indeed be animated        
        transform.Rotate(Vector3.left, extraX * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.forward, extraY * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.up, extraZ * Time.deltaTime, Space.Self);
        

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
