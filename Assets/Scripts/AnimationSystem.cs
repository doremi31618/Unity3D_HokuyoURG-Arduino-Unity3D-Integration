using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//only for on and off animation
using System;
using System.Linq;
public class AnimationSystem : MonoBehaviour {

    [Header("state setting")]
    public int frameRate = 24;
    public bool isLoop = true;
    public bool isUseGlobalAntiLogic = false;

    [Header("Animation clip setting")]
    public List<AnimationModel> animationClips;

    [Header("Simulation GameObject")]
    public List<GameObject> Light;
    private bool[] LightStates;

    public AnimationType nowPlaying;
    private void Start()
    {
        StartCoroutine(LightAnimator());
    }
    IEnumerator LightAnimator()
    {
        int playingIndex = 0;
        nowPlaying = animationClips[playingIndex].type;
        LightStates = new bool[Light.Count];
        while(isLoop)
        {
            //check what stat now it is 
            animationClips[playingIndex].AnimationSetting.stateSelector();
            switch(animationClips[playingIndex].m_animation.nowState)
            {
                case AnimationPrototype.animationState.start:
                    animationClips[playingIndex].AnimationSetting.InitializeAnimation(ref LightStates);
                    break;
                case AnimationPrototype.animationState.update:
                    animationClips[playingIndex].m_animation.UpdateAnimation(ref LightStates);
                    break;
                case AnimationPrototype.animationState.end:
                    animationClips[playingIndex].m_animation.EndAnimation(ref LightStates);
                    playingIndex += 1;
                    if (playingIndex >= animationClips.Count)
                    {
                        playingIndex = 0;
                    }
                    nowPlaying = animationClips[playingIndex].type;
                    break;
            }

            for (int i = 0; i < Light.Count;i++)
            {
                if(Light[i].GetComponent<MeshRenderer>().enabled != LightStates[i] ^ isUseGlobalAntiLogic )
                    Light[i].GetComponent<MeshRenderer>().enabled= LightStates[i] ^ isUseGlobalAntiLogic;
            }
            yield return new WaitForSeconds(1 / frameRate);
        }
    }
}

public enum AnimationType
{
    nothing,
    FullLight,
    FullDark,
    LightingUpFromStartToEnd,
    EqualDifferenceSeriesAnimation
}


[System.Serializable]
public static class LogicStateSetting
{
    //default true
    public static bool State1 = true;
    //default false
    public static bool State2 = false;
    //public bool isUseAntiLogic = false;
}

[System.Serializable]
public class AnimationModel
{
    
    public AnimationType type = AnimationType.FullLight;
    public float AnimationClipTime = 1;
    public AnimationType StartState = AnimationType.nothing;
    public AnimationType EndingState = AnimationType.nothing;
    public AnimationPrototype m_animation;
    public bool antiLogic = false;
    public bool antiOrder = false;
    [Tooltip("Only use in LightingUpFromStartToEnd")] public int maxIntervalLight = 3;
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
                    case AnimationType.LightingUpFromStartToEnd:
                        m_animation = new LightingUpFromStartToEnd();
                        m_animation.maxIntervalNumber = maxIntervalLight;
                        break;
                    case AnimationType.EqualDifferenceSeriesAnimation:
                        m_animation = new EqualDifferenceSeriesAnimation();
                        break;
                }
                m_animation.StartState = StartState;
                m_animation.EndingState = EndingState;
                m_animation.timeLength = AnimationClipTime;
                m_animation.isUseAntiLogic = antiLogic;
                m_animation.isUseAntiOrder = antiOrder;
            }

            return m_animation;
        }
    }

}
public class AnimationPrototype
{
    protected float timer = 0;
    public float timeLength;
    public AnimationType StartState;
    public AnimationType EndingState;
    public animationState nowState;
    public int maxIntervalNumber = 3;
    //protected StateSetting state;
    [Tooltip("Control direction use")]public bool isUseAntiOrder;
    [Tooltip("Control logic use")]public bool isUseAntiLogic;
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
    public enum animationState
    {
        start,
        update,
        end
    }
    public virtual void InitializeAnimation(ref bool[] _states){
        timer += Time.deltaTime;
        StateSetting(StartState, ref _states);
    }
    public virtual void UpdateAnimation(ref bool[] _states){timer += Time.deltaTime;}
    public virtual void EndAnimation(ref bool[] _states){
        timer = 0;
        StateSetting(EndingState, ref _states);
    }
    public virtual void stateSelector(){
        if (timer == 0)
            nowState = animationState.start;
        else if (timer > 0 && timer < timeLength)
            nowState = animationState.update;
        else if (timer > timeLength)
            nowState = animationState.end;
    }
    public virtual void StateSetting(AnimationType type, ref bool[] _states)
    {
        switch (type)
        {
            case AnimationType.FullDark:

                for (int i = 0; i < _states.Length; i++)
                {
                    _states[i] = (LogicStateSetting.State2) ;
                }
                break;

            case AnimationType.FullLight:
                for (int i = 0; i < _states.Length; i++)
                {
                    _states[i] = LogicStateSetting.State1;
                }
                break;

            case AnimationType.nothing:
                break;
            case AnimationType.LightingUpFromStartToEnd:
                break;
        }
    }

}

public class FullLightAnimation : AnimationPrototype
{
    
    //when timer = 0
    public override void InitializeAnimation(ref bool[] _states)
    {
        timer += Time.deltaTime;
        StateSetting(AnimationType.FullLight, ref _states);
    }
    public override void EndAnimation(ref bool[] _states)
    {
        timer = 0;
    }

}

public class FullDarkAnimation : AnimationPrototype
{
    
    //StateSetting state = new StateSetting();
    public override void InitializeAnimation(ref bool[] _states)
    {
        timer += Time.deltaTime;
        StateSetting(AnimationType.FullDark, ref _states);
    }
    public override void EndAnimation(ref bool[] _states)
    {
        timer = 0;
    }
}
public class LightingUpFromStartToEnd : AnimationPrototype
{
    
    public override void UpdateAnimation(ref bool[] _states)
    {
        timer += Time.deltaTime;
        AnimatiionLogic_1(isUseAntiOrder,maxIntervalNumber, ref _states);
        //Debug.Log("Update lighting up animation" + timer);
    }
    //public override void EndAnimation(ref bool[] _states)
    //{
        
    //}
    void AnimatiionLogic_1(bool _isUseAntiOrder,int maxDarkNumber, ref bool[] _states)
    {
        int dir = _isUseAntiOrder ? -1 : 1;
        int timeIndex = (int)Mathf.Lerp(0, _states.Length, timer / timeLength) * dir;
        int min = (_isUseAntiOrder ? (_states.Length - 1) + maxDarkNumber : 0 - maxDarkNumber) + timeIndex;
        int max = (_isUseAntiOrder ? min - (maxDarkNumber - 1) : min + (maxDarkNumber - 1));
        //Debug.Log("min : " + min + " max : " + max);
        for (int i = 0; i < _states.Length;i++)
        {
            float i_min = Mathf.Abs(i - min);
            float i_max = Mathf.Abs(i - max);
            //Debug.Log("min : " + min + " max : " + max + " i : " + i + " i-max : " + i_max + " i-min : " + i_min);
            if(i_min + i_max < maxDarkNumber)
            {
                _states[i] = LogicStateSetting.State1 ^ isUseAntiLogic;
                //Debug.Log("Lighting up");
            }
            else
            {
                _states[i] = LogicStateSetting.State2 ^ isUseAntiLogic;

                //Debug.Log("Lighting off");
            }
        }
            


    }

}
public class EqualDifferenceSeriesAnimation : AnimationPrototype
{

    public override void UpdateAnimation(ref bool[] _states)
    {
        timer += Time.deltaTime;
        AnimatiionLogic_2(isUseAntiOrder, ref _states);
    }
    void AnimatiionLogic_2(bool _isUseAntiOrder, ref bool[] _states)
    {
        
        int dir = _isUseAntiOrder ? -1 : 1;
        int timeIndex = _isUseAntiOrder ? (int)Mathf.Lerp(_states.Length - 1, 0, timer / timeLength) :
                                          (int)Mathf.Lerp(0, _states.Length - 1, timer / timeLength);

        for (int i = 0; i < _states.Length;i++)
        {
            //normal order
            if (dir > 0)
            {
                if(i <= timeIndex)
                {
                    _states[i] = LogicStateSetting.State1 ^ isUseAntiLogic;

                }else
                {
                    _states[i] = LogicStateSetting.State2 ^ isUseAntiLogic;

                }
            }
            //anti order
            else
            {
                if (i >= timeIndex)
                {
                    _states[i] = LogicStateSetting.State1 ^ isUseAntiLogic;
                }
                else
                {
                    _states[i] = LogicStateSetting.State2 ^ isUseAntiLogic;
                }
            }
        }

    }

}

