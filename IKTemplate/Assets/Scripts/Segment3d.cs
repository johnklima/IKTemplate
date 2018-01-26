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

        //update its children
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

        //quat look does the same as lookat in the end, but I don't need to set the rotation back
        Vector3 relativePos = target - transform.position;
        Quaternion b = Quaternion.LookRotation(relativePos);

        //to get a sense of space, use direction vectors compared to cardinals
        transform.localRotation = initialRotation;  //set transform to initial
        Vector3 afwd = transform.forward;           //get vector
                                                    //afwd.Set(afwd.x, 0, afwd.z);              //ignore (project to plane)
                                                    //afwd.Normalize();                         //normalize

        angleDifferenceX = initialRotation.eulerAngles.x ;
        angleDifferenceY = initialRotation.eulerAngles.y ;
        angleDifferenceZ = initialRotation.eulerAngles.z ;        

        angleDifferenceX -= a.eulerAngles.x % 180.0f;         

        angleDifferenceY -= a.eulerAngles.y % 180.0f;
        
        angleDifferenceZ -= a.eulerAngles.z % 180.0f;

        transform.localRotation = a;                //set transform to new rotation
        Vector3 bfwd = transform.forward;           //do the same
                                                    //bfwd.Set(bfwd.x, 0, bfwd.z);
                                                    //bfwd.Normalize();

        transform.localRotation = initialRotation;  //etc...
        Vector3 argt = transform.right;
        //argt.Set(argt.x, 0, 0);
        //argt.Normalize();
        
        transform.localRotation = a;
        Vector3 brgt = transform.right;
        //brgt.Set(brgt.x, 0, 0);
        //brgt.Normalize();
        
        transform.localRotation = initialRotation;
        Vector3 aup = transform.up;
        //aup.Set(0, aup.y, 0);
        //aup.Normalize();

        transform.localRotation = a;
        Vector3 bup = transform.up;
        //bup.Set(0, bup.y, 0);
        //bup.Normalize();

        //angleDifferenceX = Vector3.SignedAngle(afwd, bfwd, transform.right);
        //angleDifferenceY = Vector3.SignedAngle(aup, bup, transform.forward);
        //angleDifferenceZ = Vector3.SignedAngle(argt, brgt, transform.up);


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
