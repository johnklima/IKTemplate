using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this one is the "generic" state all others inherit from
public abstract class StateNode {
    
    //the list of child states
    protected List<StateNode> m_childStates;

    //is it currently in this state?
    public bool p_isInState;
    public RootState rootState;


    public void addChildState(StateNode state)
    {

        m_childStates.Add(state);

    }
     
    public abstract bool advanceTime(float dt);

    protected bool advanceState(float dt)
    {
        foreach (var child in m_childStates)
        {
            
            if (child.advanceTime(dt) == true)
            {
                //if any child is true, stop iterating and exit                
                return true;
            }
        }

        //if no child state is true, return false
        return false;
    }

}
