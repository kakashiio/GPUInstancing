using System;
using System.Collections.Generic;
using UnityEngine;

namespace KakashiFramework.GPUInstancing
{
    //******************************************
    //  
    //
    // @Author: Kakashi
    // @Email: junhong.cai@kunlun-inc.com
    // @Date: 2021-12-08 22:07
    //******************************************
    public class AnimationBaker
    {
        public const int DEFAULT_MAX_TEXTURE_SIZE = 2048;
        public const int DEFAULT_SAMPLE_FPS = 30;
        public const string DEFAULT_MESH_OBJ_NAME = "surface";

        public static BakeResult Bake(GameObject go, int sampleFPS = DEFAULT_SAMPLE_FPS, int maxTextureSize = DEFAULT_MAX_TEXTURE_SIZE)
        {
            var animatorBakeInfo = new AnimatorBakeInfo(go,  sampleFPS, maxTextureSize);
            var bakedObj = CreateObjectWithMeshRenderer(animatorBakeInfo.Go);
            AnimationInfos animationInfos = BakeAnimationToTexture(animatorBakeInfo);
            return new BakeResult(bakedObj, animationInfos);
        }

        public static AnimationInfos BakeAnimationToTexture(AnimatorBakeInfo animatorBakeInfo)
        {
            var go = animatorBakeInfo.Go;
            var skinnedMeshRenderer = go.GetComponentInChildren<SkinnedMeshRenderer>(true);

            int vertexMin;
            int vertexMax;
            _CalculateVertexMinAndMax(go, skinnedMeshRenderer, animatorBakeInfo.AnimationClipInfos.Values, out vertexMin, out vertexMax);
            
            var animationClipInfos = new List<AnimationClipInfo>(animatorBakeInfo.AnimationClipInfos.Values);
            animationClipInfos.Sort();
            int remainPixelCount = animatorBakeInfo.TotalPixel;
            int maxPixelPerTexture = animatorBakeInfo.MaxTextureSize * animatorBakeInfo.MaxTextureSize;
            
            List<AnimationInfo> animationInfoList = new List<AnimationInfo>();

            while (remainPixelCount > 0)
            {
                int textureSize = remainPixelCount >= maxPixelPerTexture ? animatorBakeInfo.MaxTextureSize : Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(remainPixelCount)));
                var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false, true);
                int remainPixelInThisTexture = textureSize * textureSize;
                int pixelIndex = 0;
                for (int i = 0; i < animationClipInfos.Count; i++)
                {
                    if (animationClipInfos[i].TotalPixel > remainPixelInThisTexture)
                    {
                        continue;
                    }

                    AnimationInfo animationInfo = _BakeAnimationClip(go, skinnedMeshRenderer, animationClipInfos[i], texture, textureSize, animatorBakeInfo.FPS, vertexMin, vertexMax, pixelIndex);
                    int pixelCount = animationInfo.FrameCount * animatorBakeInfo.TotalVertex;
                    pixelIndex += pixelCount;
                    remainPixelInThisTexture -= pixelCount;
                    remainPixelCount -= pixelCount;
                    animationInfoList.Add(animationInfo);
                    animationClipInfos.RemoveAt(i);
                    i--;
                }
                texture.Apply(false);
            }
            
            AnimationInfos animationInfos = ScriptableObject.CreateInstance<AnimationInfos>();
            animationInfos.VertexCount = animatorBakeInfo.TotalVertex;
            animationInfos.SetAnimationInfo(animationInfoList.ToArray());
            return animationInfos;
        }

        private static AnimationInfo _BakeAnimationClip(GameObject go, SkinnedMeshRenderer skinnedMeshRenderer, AnimationClipInfo animationClipInfo, Texture2D texture, int size, int fps, int vertexMin, int vertexMax, int pixelIndex)
        {
            int frameCount = animationClipInfo.TotalFrame;
            var mesh = new Mesh();
            
            int vertexDiff = vertexMax - vertexMin;
            Func<float, float> cal = (v) => (v - vertexMin) / vertexDiff;
            
            int currentPixelIndex = pixelIndex;
            for (int i = 0; i < frameCount; i++)
            {
                float t = i * 1f / fps;
                animationClipInfo.Clip.SampleAnimation(go, t);
                mesh.Clear(false);
                skinnedMeshRenderer.BakeMesh(mesh);
                    
                var vertices = mesh.vertices;
                for (int v = 0; v < vertices.Length; v++)
                {
                    var vertex = vertices[v];
                    Color c = new Color(cal(vertex.x), cal(vertex.y), cal(vertex.z));
    
                    int x = currentPixelIndex % size;
                    int y = currentPixelIndex / size;
                    texture.SetPixel(x, y, c);
                    currentPixelIndex++;
                }
            }
 
            return new AnimationInfo(animationClipInfo.Name, texture, fps, animationClipInfo.TotalFrame, vertexMin, vertexMax, pixelIndex);
        }

        public static GameObject CreateObjectWithMeshRenderer(GameObject srcGo, string meshObjName = DEFAULT_MESH_OBJ_NAME)
        {
            GameObject go = new GameObject(srcGo.name);
            var meshGo = new GameObject(meshObjName);
            var meshGoTransform = meshGo.transform;
            meshGoTransform.SetParent(go.transform);

            var renderer = srcGo.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var meshFilter = meshGo.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = BakeMesh(renderer);
            meshGo.AddComponent<MeshRenderer>();
            return go;
        }

        public static Mesh BakeMesh(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(mesh);
            return mesh;
        }

        // Pre calculate the min and max vertex position
        private static void _CalculateVertexMinAndMax(GameObject go, SkinnedMeshRenderer skinnedMeshRenderer, IEnumerable<AnimationClipInfo> animationClipInfos, out int vertexMin, out int vertexMax)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            Mesh preCalMesh = new Mesh();
            foreach (var animationClipInfo in animationClipInfos)
            {
                float spf = 1f / animationClipInfo.SampleFPS;
                int totalFrame = animationClipInfo.TotalFrame;
            
                for (int f = 0; f < totalFrame; f++)
                {
                    animationClipInfo.Clip.SampleAnimation(go, f * spf);

                    skinnedMeshRenderer.BakeMesh(preCalMesh);
                    for (int v = 0; v < preCalMesh.vertexCount; v++)
                    {
                        var vertex = preCalMesh.vertices[v];
                        min = Mathf.FloorToInt(Mathf.Min(min, vertex.x, vertex.y, vertex.z));
                        max = Mathf.CeilToInt(Mathf.Max(max, vertex.x, vertex.y, vertex.z));
                    }
                }
            }

            vertexMin = min;
            vertexMax = max;
        }
    }
}