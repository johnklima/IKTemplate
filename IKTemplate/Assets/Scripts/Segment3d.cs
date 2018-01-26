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
    public Vector3 initialRotation;
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
        initialRotation = transform.rotation.eulerAngles;

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

        Quaternion a = transform.rotation;                 //save current local rotation

        Vector3 irot = transform.rotation.eulerAngles;     //get its eulers
        
        transform.LookAt(target);                        //look at target
        Quaternion b = transform.rotation;          //get new local rotation
        


        if (useInterpolation)
        {
            //if the system is in drag mode, we want to crank the interpolation
            //otherwise, the chain is "lazy," it doesnt need to do anything
            float ir = interpRate;
            if (parentSystem.isDragging)
                ir *= 10;


            //convert euler and map to -180,180
            Vector3 euler = b.eulerAngles;
            
            xt = euler.x;
            yt = euler.y;
            zt = euler.z;
            

            
            
            
            if (xt > 180)
                xt -= 180;

            if (yt > 180)
                yt -= 180;

            if (zt > 180)
                zt -= 180;
            
             
             

            //use the initial rotation from which the bone limits are calculated.
            
            angleDifferenceX = xt - initialRotation.x;
            angleDifferenceY = yt - initialRotation.y;
            angleDifferenceZ = zt - initialRotation.z;


            QMaxAngle = Mathf.Abs(Quaternion.Angle(maxQ, b));
            /*
            if (QMaxAngle < 1)
                b = maxQ;

            QMinAngle = Mathf.Abs(Quaternion.Angle(minQ, b));
            if (QMinAngle < 1)
                b = minQ;

            */

            

            if (angleDifferenceX < minRotation.x || angleDifferenceX > maxRotation.x)
                xt = irot.x;
            if (angleDifferenceY < minRotation.y || angleDifferenceY > maxRotation.y)
                yt = irot.y;
            if (angleDifferenceZ < minRotation.z || angleDifferenceZ > maxRotation.z)
                zt = irot.z;
            
            

            /*
            //if ANY constraint value is true, stop and exit - no wrong
            if (angleDifferenceX < minRotation.x || 
                angleDifferenceY < minRotation.y || 
                angleDifferenceZ < minRotation.z || 
                angleDifferenceX > maxRotation.x || 
                angleDifferenceY > maxRotation.y || 
                angleDifferenceZ > maxRotation.z  )
            {
                transform.localRotation = a;                     //set it back to initial        
                Debug.Log("EXIT CONSTRAINT");
                return;
            }
            */

            transform.rotation = a;                     //set it back to initial
            euler.Set(xt, yt, zt);
            b.eulerAngles = euler;


            //spherical interpolate
            float t = Time.deltaTime;
            Quaternion c = Quaternion.Slerp(a, b, t * ir);

            transform.rotation = c;

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
