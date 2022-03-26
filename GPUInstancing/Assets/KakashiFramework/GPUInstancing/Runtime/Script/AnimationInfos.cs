using UnityEngine;

namespace KakashiFramework.GPUInstancing
{

    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-11-14 23:18
    //******************************************
    public class AnimationInfos : ScriptableObject
    {
        public AnimationInfo[] AllAnimationInfo;

        public void SetAnimationInfo(AnimationInfo[] animationInfos)
        {
            AllAnimationInfo = animationInfos;
        }
    }
}