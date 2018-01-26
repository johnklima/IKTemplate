using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//our first "concrete" state
public class  JumpState: StateNode
{

    private enum StateProgessStates { Start, InAir, Land};
    private int stateProgress = 0;
    private float jumptimer = 0;
    private float initialY = 0;
    public  float durationOfJump = 2.0f;
    public float jumpHeight = 4.0f;
    public float jumpDistance = 6.0f;
    

    private Vector3 jumpVector = new Vector3(0, 0, 0);

    //constructor
    public JumpState(RootState root)
    {
        rootState = root;
        m_childStates = new List<StateNode>();
    }


    public override bool advanceTime(float dt)
    {

        //if any child state is true, set my state and return
        //do not continue to process state tree, back out from here
        if (advanceState(dt) == true)
        {
            //if any child state is true, I am false
            p_isInState = false;

            rootState.playermotion.jump.enabled = false;
            rootState.playermotion.isJumping = false;

            //since a child state is true, return this fact!
            return true;
        }

        if ( Input.GetKeyDown(KeyCode.Space)        && 
             rootState.playermotion.energy > 0.0f   && 
             !p_isInState)
        {

            rootState.playermotion.isJumping = true;
            rootState.playermotion.energy -= 0.0f; // subtract none for now
            p_isInState = true;
            stateProgress = (int) StateProgessStates.Start;

            jumpVector *= 0;            //ensure jump vector starts at zero
            initialY = rootState.playermotion.gameObject.transform.position.y;

            Debug.Log("Jump init");

        }

        if (p_isInState)
        {

            if (stateProgress == (int)StateProgessStates.Start)
            {
                //initialize jump
                jumptimer = Time.time;
                stateProgress = (int)StateProgessStates.InAir;
                rootState.playermotion.jump.enabled = true;
                Debug.Log("Jump Start");
            }
            else if (stateProgress == (int)StateProgessStates.InAir)
            {
                //handle motion and animation in air
                Debug.Log("In Air");

                /* 
                 * we can simply use sin of time/duration to arrive at an x,y,z and mult by some amplitude (height)
                 * and some frequency (distance)
                 * first we must scale PI (an angle) by our current time increment. basically we want to
                 * count from 0 to PI over time, to pass to Sine
                */
                float f = Mathf.PI  * ((Time.time - jumptimer) / durationOfJump);
                float h = Mathf.Sin(f) * jumpHeight; //amplitude is height

                //next get the player forward vector
                Vector3 fwd = rootState.playermotion.gameObject.transform.forward;

                //mult x,z by time passed as a function of delta time
                fwd *= dt * jumpDistance;//frequency is distance
                
                // replace Y with 0
                fwd.Set(fwd.x, 0, fwd.z);
                
                // add forward motion to current position
                jumpVector = rootState.playermotion.gameObject.transform.position + fwd;

                float finalH =  initialY + 
                                h + 
                                rootState.playermotion.groundOffset;

                // set y position absolute
                jumpVector.Set(jumpVector.x,  finalH , jumpVector.z);

                //set player position accordingly - this is absolute position
                rootState.playermotion.gameObject.transform.position = jumpVector;

                //ignore physics velocity when in jump - for now just kill it
                rootState.playermotion.velocity *= 0;

                if (Time.time - jumptimer > durationOfJump)
                {
                    stateProgress = (int)StateProgessStates.Land;
                }

                if (rootState.playermotion.gameObject.transform.position.y < rootState.playermotion.terrainHeight)
                {
                    stateProgress = (int)StateProgessStates.Land;
                }
            }
            else if (stateProgress == (int)StateProgessStates.Land)
            {
                //return to the ground
                Debug.Log("Land");
                rootState.playermotion.jump.enabled = false;
                rootState.playermotion.isJumping = false;
                p_isInState = false;
            }

           
        }

        return p_isInState;
    }


}