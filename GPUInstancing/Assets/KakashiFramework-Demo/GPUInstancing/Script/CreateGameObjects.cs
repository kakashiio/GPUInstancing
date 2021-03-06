using System;
using System.Collections.Generic;
using KakashiFramework.GPUInstancing;
using UnityEngine;

public class CreateGameObjects : MonoBehaviour
{
    public GameObject[] Prefabs;
    public int XCount = 10;
    public int YCount = 10;
    public int ZCount = 10;
    public float Padding = 1;
    
    private Transform[] _Parents;
    private int _ActiveIndex;

    private int _AniIndex;
    private List<GPUInstancingAnimation> _GPUInstancingAnimations = new List<GPUInstancingAnimation>();
    
    void Start()
    {
        _Parents = new Transform[Prefabs.Length];
        for (int i = 0; i < Prefabs.Length; i++)
        {
            var prefab = Prefabs[i];
            
            Animator animator = prefab.GetComponent<Animator>();
            bool instancing = animator == null;
            var parentGo = new GameObject(prefab.name + "(" + (animator ? "Animator" : "Instancing Animation") + ")");
            parentGo.SetActive(i == _ActiveIndex);
            _Parents[i] = parentGo.transform;

            Func<int, int, float> pos = (index, count) => (index - (count - 1) * 0.5f) * Padding;
            for (int x = 0; x < XCount; x++)
            {
                for (int y = 0; y < YCount; y++)
                {
                    for (int z = 0; z < ZCount; z++)
                    {
                        var go = Instantiate(prefab, new Vector3(pos(x, XCount), pos(y, YCount), pos(z, ZCount)), Quaternion.identity, _Parents[i]);
                        if (instancing)
                        {
                            _GPUInstancingAnimations.Add(go.GetComponent<GPUInstancingAnimation>());
                        }
                    }
                }    
            }
        }
    }

    private void OnGUI()
    {
        float margin = 60;
        float padding = 10;
        float width = 300;
        float height = 60;
        
        for (int i = 0; i < _Parents.Length; i++)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            if (i == _ActiveIndex)
            {
                style.normal.textColor = Color.green;
            }

            if (GUI.Button(new Rect(margin, margin + (height + padding) * i, width, height), _Parents[i].name, style))
            {
                _Parents[_ActiveIndex].gameObject.SetActive(false);
                _Parents[i].gameObject.SetActive(true);
                _ActiveIndex = i;
            }
        }

        if (GUILayout.Button("Next"))
        {
            var allAnimationInfo = _GPUInstancingAnimations[0].AnimationInfos.AllAnimationInfo;
            _AniIndex = (_AniIndex + 1) % allAnimationInfo.Length;
            var aniName = allAnimationInfo[_AniIndex].AnimationName;
            foreach (var gpuInstancingAnimation in _GPUInstancingAnimations)
            {
                gpuInstancingAnimation.Play(aniName);
            }
        }
    }
}
