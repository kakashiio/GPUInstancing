Shader "Kakashi/Vertex Animation - Diffuse"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows vertex:vert
        #pragma multi_compile_instancing
		#include "VertexAnimation.cginc"

        sampler2D _MainTex;
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

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
