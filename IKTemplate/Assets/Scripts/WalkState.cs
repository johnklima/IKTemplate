using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : StateNode {

    public Vector3 velocity;
        
    //constructor
    public WalkState(RootState root)
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

            //disable the animation
            rootState.playermotion.walk.enabled = false;

            //since a child state is true, return this fact!
            return true;
        }
        
        //get the velocity
        Vector3 velo = rootState.playermotion.velocity;

        //remove the Y gravity component
        velo.Set(velo.x, 0, velo.z);

        if (velo.magnitude > 0.5f)
            p_isInState = true;
        else
            p_isInState = false;

        //force into state for animation testing
        //p_isInState = true;

        if (p_isInState)
        {
            //do something
            Debug.Log("IN WALKING");
            rootState.playermotion.walk.enabled = true;
        }
        else
        {
            //clean up my state
            //disable the animation
            rootState.playermotion.walk.enabled = false;

        }

        return p_isInState;
    }

}
