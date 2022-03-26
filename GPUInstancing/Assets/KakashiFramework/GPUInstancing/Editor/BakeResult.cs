using UnityEngine;

namespace KakashiFramework.GPUInstancing
{
    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-12-08 22:10
    //******************************************
    public class BakeResult
    {
        public readonly bool Succ;
        public readonly string Error;
        public readonly GameObject BakedObject;
        public readonly AnimationInfos AnimationInfos;

        public BakeResult(GameObject bakedObj, AnimationInfos animationInfos)
        {
            Succ = true;
            BakedObject = bakedObj;
            AnimationInfos = animationInfos;
        }

    }
}