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
        public const int DEFAULT_SAMPLE_FPS = 30;
        public const string DEFAULT_MESH_OBJ_NAME = "surface";

        public static BakeResult Bake(GameObject go, int sampleFPS = DEFAULT_SAMPLE_FPS)
        {
            var animatorBakeInfo = new AnimatorBakeInfo(go, sampleFPS);
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
            
            List<AnimationInfo> animationInfoList = new List<AnimationInfo>();
            foreach (var animationClipInfo in animatorBakeInfo.AnimationClipInfos.Values)
            {
                int textureSize = Mathf.NextPowerOfTwo(Mathf.CeilToInt(Mathf.Sqrt(animationClipInfo.TotalPixel)));
                var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false, true);
                AnimationInfo animationInfo = _BakeAnimationClip(go, skinnedMeshRenderer, animationClipInfo, texture, textureSize, animatorBakeInfo.FPS, vertexMin, vertexMax);
                animationInfoList.Add(animationInfo);
                texture.Apply(false);
            }
            
            AnimationInfos animationInfos = ScriptableObject.CreateInstance<AnimationInfos>();
            animationInfos.SetAnimationInfo(animationInfoList.ToArray());
            return animationInfos;
        }

        private static AnimationInfo _BakeAnimationClip(GameObject go, SkinnedMeshRenderer skinnedMeshRenderer, AnimationClipInfo animationClipInfo, Texture2D texture, int size, int fps, int vertexMin, int vertexMax)
        {
            int frameCount = animationClipInfo.TotalFrame;
            var mesh = new Mesh();
            int vertextCount = 0;
            
            int vertexDiff = vertexMax - vertexMin;
            Func<float, float> cal = (v) => (v - vertexMin) / vertexDiff;
            
            vertextCount += skinnedMeshRenderer.sharedMesh.vertexCount;
            int pixelIndex = 0;
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
    
                    int x = pixelIndex % size;
                    int y = pixelIndex / size;
                    texture.SetPixel(x, y, c);
                    pixelIndex++;
                }
            }
 
            return new AnimationInfo(animationClipInfo.Name, texture, fps, animationClipInfo.TotalFrame, vertextCount, vertexMin, vertexMax);
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