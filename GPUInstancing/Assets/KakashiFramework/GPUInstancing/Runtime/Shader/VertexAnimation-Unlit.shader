Shader "Kakashi/Vertex Animation - Unlit"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
	}
	
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 300

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "VertexAnimation.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				VERTEX_ANIMATION_IN
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				VERTEX_ANIMATION_OUT
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			VERTEX_ANIMATION_PROPS(_AniTex)
			
			v2f vert (appdata v)
			{
				v2f o;
				VERTEX_ANIMATION_VERT(v.vertex, _AniTex)
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}                
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
	
    Fallback "Legacy Shaders/VertexLit"
}
