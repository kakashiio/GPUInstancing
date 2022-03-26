using System.Collections.Generic;
using System.IO;
using KakashiFramework.GPUInstancing;
using UnityEditor;
using UnityEngine;
using AnimationInfo = KakashiFramework.GPUInstancing.AnimationInfo;

public class BakeAnimationMenu
{
    [MenuItem("Kakashi/GPU Instancing")]
    public static void BakeGPUInstancing()
    {
        string srcPrefabAssetDirPath = "Assets/KakashiFramework-Demo/GPUInstancing/Prefab";
        string dstAssetPath = "Assets/KakashiFramework-Demo/GPUInstancing/Baked/{0}/";
        
        var guids = AssetDatabase.FindAssets("t:prefab", new [] { srcPrefabAssetDirPath });
        foreach (var guid in guids)
        {
            var srcPrefabAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            _Bake(srcPrefabAssetPath, dstAssetPath);
        }
    }

    private static void _Bake(string srcPrefabAssetPath, string dstAssetPath)
    {
        var prefabName = Path.GetFileNameWithoutExtension(srcPrefabAssetPath);
        
        dstAssetPath = string.Format(dstAssetPath, prefabName);
        string bakedBaseData = dstAssetPath.Substring("Assets/".Length);
        string bakedBaseAssetPath = dstAssetPath;

        _CreateFolderIfNeed(bakedBaseAssetPath);

        string meshAssetPath = bakedBaseAssetPath + "mesh{0}.mesh";
        string animationInfoName = "animation_infos.asset";
        string matTplPath = "mat{0}.mat";
        string prefabTplPath = "{0}.prefab";
        string textureTplPath = "{0}.png";
        string shaderName = "Kakashi/Vertex Animation - Diffuse";
        
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(srcPrefabAssetPath);
        var bakeResult = AnimationBaker.Bake(prefab);
        if (!bakeResult.Succ)
        {
            Debug.LogError("Fail to bake : " + bakeResult.Error);
            return;
        }

        _SaveTextures(bakedBaseData + textureTplPath, bakeResult.AnimationInfos.AllAnimationInfo);
        
        Material material = new Material(Shader.Find(shaderName));
        material.enableInstancing = true;
        
        var bakedObject = bakeResult.BakedObject;
        _InitInstancingComponent(bakedObject, bakeResult.AnimationInfos, material);
        _SaveMeshesAndSetRenderers(meshAssetPath, bakedObject, material);
        
        _CreateAsset(bakedBaseAssetPath + animationInfoName, bakeResult.AnimationInfos);
        _CreateAsset(bakedBaseAssetPath + string.Format(matTplPath, 0), material);
        PrefabUtility.SaveAsPrefabAsset(bakedObject, bakedBaseAssetPath + string.Format(prefabTplPath, prefab.name));
        GameObject.DestroyImmediate(bakedObject);
    }

    private static void _InitInstancingComponent(GameObject bakedObject, AnimationInfos animationInfos, Material material)
    {
        GPUInstancingAnimation gpuInstancingAnimation = bakedObject.AddComponent<GPUInstancingAnimation>();
        gpuInstancingAnimation.AnimationInfos = animationInfos;
        gpuInstancingAnimation.Material = material;
    }

    private static void _SaveMeshesAndSetRenderers(string meshAssetPath, GameObject bakedObject, Material material)
    {
        int meshIndex = 0;
        foreach (var renderer in bakedObject.GetComponentsInChildren<Renderer>())
        {
            renderer.material = material;
            var meshFilter = renderer.GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;
            _CreateAsset(string.Format(meshAssetPath, meshIndex), mesh);
            meshIndex++;
        }
    }

    private static void _SaveTextures(string textureTplPath, AnimationInfo[] allAnimationInfo)
    {
        Dictionary<Texture2D, List<AnimationInfo>> textures = new Dictionary<Texture2D, List<AnimationInfo>>();
        foreach (var animationInfo in allAnimationInfo)
        {
            List<AnimationInfo> animationInfos;
            if (textures.ContainsKey(animationInfo.Texture))
            {
                animationInfos = textures[animationInfo.Texture];
            }
            else
            {
                animationInfos = new List<AnimationInfo>();
                textures.Add(animationInfo.Texture, animationInfos);
            }

            animationInfos.Add(animationInfo);
        }

        int index = 0;
        foreach (var texture2Info in textures)
        {
            var texturePath = string.Format(textureTplPath, index);
            
            var textureFullPath = Path.Combine(Application.dataPath, texturePath);
            var texture = texture2Info.Key;
            var pngData = texture.EncodeToPNG();
            File.WriteAllBytes(textureFullPath, pngData);
            GameObject.DestroyImmediate(texture);
            AssetDatabase.Refresh();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            var textureAssetPath = "Assets/" + texturePath;
            
            TextureImporter textureImporter = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.mipmapEnabled = false;
            textureImporter.sRGBTexture = false;
            textureImporter.SaveAndReimport();
            
            var texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            foreach (var info in texture2Info.Value)
            {
                info.Texture = texture2D;
            }
            index++;
        }
    }

    private static void _CreateFolderIfNeed(string assetPath)
    {
        if (assetPath.EndsWith("/"))
        {
            assetPath = assetPath.Substring(0, assetPath.Length - 1);
        }

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        var parentAndChildIndex = assetPath.LastIndexOf('/');
        var parent = assetPath.Substring(0, parentAndChildIndex);
        _CreateFolderIfNeed(parent);
        
        var child = assetPath.Substring(parentAndChildIndex + 1);
        AssetDatabase.CreateFolder(parent, child);

        AssetDatabase.SaveAssets();
    }

    private static void _CreateAsset<T>(string assetPath, T asset) where T : Object
    {
        if (AssetDatabase.LoadAssetAtPath<T>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
        AssetDatabase.CreateAsset(asset, assetPath);
    }
}
