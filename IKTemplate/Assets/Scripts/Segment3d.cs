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
    public int  modLostAngles = 1;

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
        //update its children (from toe to hips)
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

        //remove additional rotations prior to constraint calculations        
        transform.Rotate(Vector3.left, -extraX, Space.Self);
        transform.Rotate(Vector3.up, -extraY, Space.Self);
        transform.Rotate(Vector3.forward, -extraZ, Space.Self);



        Quaternion a = transform.localRotation;                 //save current local rotation       

        Quaternion b = FindLookAt(target);                      //look at the target point

        //if (parent)
        //    b = parent.gameObject.transform.localRotation * b;

        if (useInterpolation)
        {
            //if the system is in drag mode, we want to crank the interpolation
            //otherwise, the chain is "lazy," it doesnt need to do anything
            float ir = interpRate;
            if (parentSystem.isDragging)
                ir *= 10;


            //set rotation back to start position
            transform.localRotation = a;

            //spherical interpolate
            float t = Time.deltaTime;
            Quaternion c = Quaternion.Slerp(a, b, t * ir);
            

            transform.localRotation = c;


        }
        else
        {
            transform.rotation = b;
        }

        //apply additional rotations on axis useful for tweaking and can indeed be animated        
        transform.Rotate(Vector3.left, extraX , Space.Self);
        transform.Rotate(Vector3.up, extraY , Space.Self);
        transform.Rotate(Vector3.forward, extraZ , Space.Self);
        

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
    public void reachCCD(Vector3 target)
    {


    }
    public void reachAlt(Vector3 target)
    {
        pointAt(target);
        updateSegment();

        length = transform.GetChild(0).localScale.z;
        if (child)
        {

            child.reachAlt(target);
            //updateSegmentAndChildren();
        }

    }

    Quaternion FindLookAt(Vector3 target)
    {

        Quaternion b = Quaternion.identity;

        if (useConstraints)
        {
            //compose 3 individual angles, x , y, z on each plane
            Quaternion prot = Quaternion.identity;
            if (parent)
                prot = parent.gameObject.transform.localRotation;

            //start by total reset of rotations
            transform.rotation = Quaternion.identity ;
            
            //X
            
            Vector3 targetDir = target - transform.position;
            Vector3 currentDir = transform.forward;  


            //flatten to my plane of interest y,z
            currentDir.Set(0, currentDir.y, currentDir.z);
            targetDir.Set(0, targetDir.y, targetDir.z);
            xt = Vector3.SignedAngle(targetDir, currentDir, Vector3.left) ;
            
            Quaternion qa = Quaternion.AngleAxis(xt, transform.right);
            Quaternion qx =  qa;


            //clamp X
            //initial rotation is the "rigging" rotation, we add parent x to allow
            //child to rotate "plus" the rig,maxmin, and parent rotation

            float max = initialRotation.x + maxRotation.x;
            float min = initialRotation.x + minRotation.x;
           
            if (xt > max)
            {
                qx = Quaternion.Euler(max, 0, 0);
                angleDifferenceX = xt - max;
            }               
            else if (xt < min)
            {
                qx = Quaternion.Euler(min, 0, 0);
                angleDifferenceX = xt - min;
            }

            xt = Mathf.Clamp(xt,min, max);


            //qx is now a valid rotation on the x axis

            //Y

            max = initialRotation.y + maxRotation.y;
            min = initialRotation.y + minRotation.y;
            
            targetDir = target - transform.position;
            currentDir = transform.forward; 

            //flatten to my plane of interest x,z
            currentDir.Set(currentDir.x, 0, currentDir.z);
            targetDir.Set(targetDir.x, 0, targetDir.z);
            yt = Vector3.SignedAngle(targetDir, currentDir, Vector3.down);
            qa = Quaternion.AngleAxis(yt, transform.up);
            Quaternion qy = qa;

            
            //clamp Y
            //initial rotation is the "rigging" rotation
            if (yt > max)
            {
                qy = Quaternion.Euler(0, max, 0);
                angleDifferenceY = yt - max;
            }
            else if (yt < min)
            {
                qy = Quaternion.Euler(0, min, 0);
                angleDifferenceY = yt - min;
            }

            yt = Mathf.Clamp(yt, min, max);


            //Z

            max = initialRotation.z + maxRotation.z;
            min = initialRotation.z + minRotation.z;

            //first we DO apply the x,y rotation, Z is just a twist
            transform.localRotation = Quaternion.Euler(xt, yt, 0);
                        
            targetDir = target - transform.position;
            currentDir = transform.forward;

            //flatten to my plane of interest x,y
            currentDir.Set(currentDir.x, 0, currentDir.z);
            targetDir.Set(targetDir.x, 0, targetDir.z);
            zt = Vector3.SignedAngle(targetDir, currentDir, Vector3.down);
            qa = Quaternion.AngleAxis(zt, transform.forward);
            Quaternion qz = qa;
            
            //clamp Z
            //initial rotation is the "rigging" rotation
            if (zt > max)
            {
                qz = Quaternion.Euler(0, 0, max);
                angleDifferenceZ = zt - max;
            }
            else if (zt < min)
            {
                qz = Quaternion.Euler(0, 0, min);
                angleDifferenceZ = zt - min;
            }

            zt = Mathf.Clamp(zt, min, max);



            return Quaternion.Euler(xt, yt, zt);
            
        }
        else
            return Quaternion.LookRotation(Vector3.Normalize(target - transform.position));

        

    }




    Quaternion Qclamp(Quaternion b)
    {
        /*
        * CONSTRAINTS:
        * the idea here is to get each x,y,z rotation component as a direction, and a "twist" around that
        * direction, comparing it to min/max values for each axis, and clamping the rotation to prevent
        * it from exceeding what would be considered "normal" human motion.
        * 
        */
        //if (useConstraints)
        {
            //we are looking for an axis and an angle
            float ang;              //the unstrained angle
            float ang2;             //the constrained angle
            float cx = 0;           //the lost angle from child constraint (if any)
            float cy = 0;
            float cz = 0;

            //our child may have clamped so if we have one, get its missing rotations
            if (child)
            {
                cx = child.angleDifferenceX * modLostAngles;
                cy = child.angleDifferenceY * modLostAngles;
                cz = child.angleDifferenceZ * modLostAngles;

            }


            Vector3 axis;
            //we will accumulate 3 rotations, one per axis
            Quaternion qx, qy, qz;

            //clamp on X axis - the most important axis for a human
            b.ToAngleAxis(out ang, out axis);               //what is our target rotation
            float ix = initialRotation.x;       //what is our "start" rotation, neutral pose
            axis.y = 0;                                     //remove y and z from the direction, just the x please
            axis.z = 0;
            axis.Normalize();                               //make sure it is a "unit" vector

            //clamp our current angle to range min and max from our neutral pose e.g. thigh can rotate
            //+-60 deg on x from the initial rotation
            ang2 = Mathf.Clamp(ang + cx, ix + minRotation.x, ix + maxRotation.x);
            //how much have we clamped?
            angleDifferenceX = ang2 - ang;

            //and convert to a quat rotation for this axis
            qx = Quaternion.AngleAxis(ang2, axis);

            //clamp on Y axis - etc...
            b.ToAngleAxis(out ang, out axis);
            float iy = initialRotation.y;
            axis.x = 0;
            axis.z = 0;
            axis.Normalize();
            ang2 = Mathf.Clamp(ang + cy, iy + minRotation.y, iy + maxRotation.y);
            angleDifferenceY = ang2 - ang;

            qy = Quaternion.AngleAxis(ang2, axis);

            //clamp on z axis
            b.ToAngleAxis(out ang, out axis);
            float iz = initialRotation.z;
            axis.x = 0;
            axis.y = 0;
            axis.Normalize();
            ang2 = Mathf.Clamp(ang + cz, iz + minRotation.z, iz + maxRotation.z);
            angleDifferenceZ = ang2 - ang;

            qz = Quaternion.AngleAxis(ang2, axis);

            //order here is critical, quats are not mathematically commutive
            //this is our final angle for this joint
            b = qx * qz * qy;

            return b;

        }



    }
}
