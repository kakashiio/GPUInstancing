using System;
using UnityEngine;

namespace KakashiFramework.GPUInstancing
{
    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-12-08 22:22
    //******************************************
    public class AnimationClipInfo : IComparable<AnimationClipInfo>
    {
        public readonly AnimationClip Clip;
        public readonly int TotalFrame;
        public readonly int TotalPixel;
        public readonly int SampleFPS;
        public readonly string Name;

        public AnimationClipInfo(string name, AnimationClip clip, int totalFrame, int totalPixel, int sampleFPS)
        {
            Name = name;
            Clip = clip;
            TotalFrame = totalFrame;
            TotalPixel = totalPixel;
            SampleFPS = sampleFPS;
        }

        public int CompareTo(AnimationClipInfo other)
        {
            return -TotalPixel.CompareTo(other.TotalPixel);
        }
    }
}