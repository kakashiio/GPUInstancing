using UnityEngine;

namespace KakashiFramework.GPUInstancing
{
    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-12-15 23:04
    //******************************************
    public class GPUInstancingAnimation : MonoBehaviour
    {
        private static int FRAME_COUNT = Shader.PropertyToID("_FrameCount");
        private static int VERTEX_COUNT = Shader.PropertyToID("_VertexCount");
        private static int FRAME_OFFSET = Shader.PropertyToID("_FrameOffset");
        private static int SAMPLE_RATE = Shader.PropertyToID("_SampleRate");
        private static int VERTEX_MIN = Shader.PropertyToID("_VertexMin");
        private static int VERTEX_MAX = Shader.PropertyToID("_VertexMax");
        private static int ANI_TEX = Shader.PropertyToID("_AniTex");

        public AnimationInfos AnimationInfos;
        public Material Material;

        private static MaterialPropertyBlock _MaterialPropertyBlock;
        
        private void Awake()
        {
            Play(AnimationInfos.AllAnimationInfo[0].AnimationName);
        }

        public void Play(string aniName)
        {
            if (_MaterialPropertyBlock == null)
            {
                _MaterialPropertyBlock = new MaterialPropertyBlock();
            }
            
            AnimationInfo animationInfo = _GetAnimationInfo(aniName);
            
            var mat = Material;
            mat.SetInt(VERTEX_COUNT, AnimationInfos.VertexCount);
            mat.SetInt(VERTEX_MIN, animationInfo.VertexMin);
            mat.SetInt(VERTEX_MAX, animationInfo.VertexMax);
            
            _MaterialPropertyBlock.SetInt(FRAME_COUNT, animationInfo.FrameCount);
            _MaterialPropertyBlock.SetInt(SAMPLE_RATE, animationInfo.FrameRate);
            _MaterialPropertyBlock.SetInt(FRAME_OFFSET, 0);
            _MaterialPropertyBlock.SetTexture(ANI_TEX, animationInfo.Texture);
            
            GetComponentInChildren<Renderer>().SetPropertyBlock(_MaterialPropertyBlock);
        }

        private AnimationInfo _GetAnimationInfo(string aniName)
        {
            foreach (var animationInfo in AnimationInfos.AllAnimationInfo)
            {
                if (string.Equals(aniName, animationInfo.AnimationName))
                {
                    return animationInfo;
                }
            }
            return null;
        }
    }
}