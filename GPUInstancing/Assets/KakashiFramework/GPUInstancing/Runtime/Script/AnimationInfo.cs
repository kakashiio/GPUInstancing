using System;
using UnityEngine;

namespace KakashiFramework.GPUInstancing
{

    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-11-14 23:26
    //******************************************
    [Serializable]
    public class AnimationInfo
    {
        public string AnimationName;
        public Texture2D Texture;
        public int FrameRate;
        public int FrameCount;
        public int VertexMax;
        public int VertexMin;
        public int PixelIndex;

        public AnimationInfo()
        {
        }

        public AnimationInfo(string animationName, Texture2D texture, int frameRate, int frameCount, int vertexMin, int vertexMax, int pixelIndex)
        {
            AnimationName = animationName;
            Texture = texture;
            FrameRate = frameRate;
            FrameCount = frameCount;
            VertexMin = vertexMin;
            VertexMax = vertexMax;
            PixelIndex = pixelIndex;
        }

    }
}