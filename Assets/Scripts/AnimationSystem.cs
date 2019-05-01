using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//only for on and off animation
using System;
public class AnimationSystem : MonoBehaviour {

    [Header("state setting")]
    public int frameRate = 24;
    public bool isLoope = true;
    public static bool isUseAntiLogic = false;

    [Header("Animation clip setting")]
    public List<AnimationModel> animationClips;

    [Header("Simulation GameObject")]
    public List<GameObject> Light;
    private bool[] LightStates;

    IEnumerator _AnimationController()
    {
        yield return new WaitForSeconds(1 / frameRate);
    }

}

public enum AnimationType
{
    nothing,
    FullLight,
    FullDark,

}


[System.Serializable]
public class StateSetting
{
    //default true
    public bool State1 = !AnimationSystem.isUseAntiLogic;
    //default false
    public bool State2 = AnimationSystem.isUseAntiLogic;
    //public bool isUseAntiLogic = false;
}

[System.Serializable]
public class AnimationModel
{
    
    public AnimationType type = AnimationType.FullLight;
    public float AnimationClipTime = 1;
    public AnimationType StartState;
    public AnimationType EndingState;
    public AnimationPrototype m_animation;
    public AnimationPrototype AnimationSetting
    {
        get
        {
            if(m_animation == null)
            {
                switch (type)
                {
                    case AnimationType.FullDark:
                        m_animation = new FullDarkAnimation();
                        break;
                    case AnimationType.FullLight:
                        m_animation = new FullLightAnimation();
                        break;
                }
            }
            m_animation.StartState = StartState;
            m_animation.EndingState = EndingState;
            return m_animation;
        }
    }

}
public class AnimationPrototype
{
    protected float timer = -0.00001f;
    public float timeLength;
    public AnimationType StartState;
    public AnimationType EndingState;
    public bool isPlaying
    {
        get{
            if((int)timer / timeLength <= 0 || (int)timer / timeLength >= 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    public virtual void InitializeAnimation(float _timeLenth,ref bool[] _states){}
    public virtual void UpdateAnimation(ref bool[] _states){}
    public virtual void EndAnimation(ref bool[] _states){}
}

public class FullLightAnimation : AnimationPrototype
{
    StateSetting state = new StateSetting();
    public override void InitializeAnimation(float _timeLenth,ref bool[] _states)
    {
        timer = -0.00001f;
        timeLength = _timeLenth;
        //StateSetting state = new StateSetting();
        StateSetting(StartState, ref _states);
    }
    public override void UpdateAnimation(ref bool[] _states)
    {
        timer += Time.deltaTime;
        if(timer/timeLength > 1)
        {
            EndAnimation(ref _states);
        }
    }
    public override void EndAnimation(ref bool[] _states)
    {
        timer = -0.00001f;
        StateSetting(EndingState,ref _states);
    }
    void StateSetting(AnimationType type,ref bool[] _states)
    {
        switch (StartState)
        {
            case AnimationType.FullDark:

                for (int i = 0; i < _states.Length; i++)
                {
                    _states[i] = state.State2;
                }
                break;

            case AnimationType.FullLight:
                for (int i = 0; i < _states.Length; i++)
                {
                    _states[i] = state.State1;
                }
                break;

            case AnimationType.nothing:
                break;
        }
    }

}

public class FullDarkAnimation : AnimationPrototype
{
    
}
