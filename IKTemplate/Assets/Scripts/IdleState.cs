using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//our first "concrete" state
public class IdleState : StateNode
{

    //constructor
    public IdleState(RootState root)
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

            rootState.playermotion.idle.enabled = false;

            //since a child state is true, return this fact!
            return true;
        }

        //lets just say I am true, which in fact I always am if none of my children are true
        //as IDLE is the first state under root
        p_isInState = true;
        if (p_isInState)
        {
            //do something
            Debug.Log("IN IDLE");
            rootState.playermotion.idle.enabled = true;
        }

        return p_isInState;
    }


}
