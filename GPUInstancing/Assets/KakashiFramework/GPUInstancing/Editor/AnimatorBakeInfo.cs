using System.Collections.Generic;
using UnityEngine;

namespace KakashiFramework.GPUInstancing
{

    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-11-13 00:59
    //******************************************
    public class AnimatorBakeInfo
    {
        public readonly Dictionary<string, AnimationClipInfo> AnimationClipInfos = new Dictionary<string, AnimationClipInfo>();
        public readonly int FPS;
        public readonly GameObject Go;
        public readonly int TotalVertex;
        public readonly int TotalPixel;
        public readonly int MaxTextureSize;
        
        public AnimatorBakeInfo(GameObject go, int sampleFPS = 30, int maxTextureSize = 2048) : this(go.GetComponent<Animator>(), sampleFPS, maxTextureSize)
        {
        }

        public AnimatorBakeInfo(Animator animator, int sampleFPS = 30, int maxTextureSize = 2048)
        {
            FPS = sampleFPS;
            Go = animator.gameObject;
            TotalVertex = Go.GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMesh.vertexCount;

            var animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            var stateMachine = animatorController.layers[0].stateMachine;
            var childAnimatorStates = stateMachine.states;
            int totalPixel = 0;
            for (int i = 0; i < childAnimatorStates.Length; i++)
            {
                var state = childAnimatorStates[i].state;
                var clip = state.motion as AnimationClip;
                
                int frameCount = Mathf.CeilToInt(clip.length * sampleFPS);
                int pixelCount = frameCount * TotalVertex;
                totalPixel += pixelCount;
                AnimationClipInfos.Add(state.name, new AnimationClipInfo(state.name, clip, frameCount, pixelCount, sampleFPS));
            }

            MaxTextureSize = maxTextureSize;
            TotalPixel = totalPixel;
        }
    }
}