Shader "Kakashi/Vertex Animation - Standard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MetallicGlossMap ("Metallic", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _OcclusionMap ("Occlusionmap", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
		#include "VertexAnimation.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        sampler2D _OcclusionMap;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
         
        struct Input
        {
            float2 uv_MainTex;
        };

		VERTEX_ANIMATION_PROPS(_AniTex)
        VERTEX_ANIMATION_SURFACE_VERTEX_INPUT
        
        void vert(inout appdata_full_instacing v)
        {
			VERTEX_ANIMATION_SURFACE_VERT(v.vertex, _AniTex)
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
            fixed4 ms = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = ms.r;
            o.Smoothness = _Glossiness * ms.a;
		    o.Occlusion = tex2D(_OcclusionMap, IN.uv_MainTex).r;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
