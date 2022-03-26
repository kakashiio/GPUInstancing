#ifndef VERTEX_ANIMATION
#define VERTEX_ANIMATION

    #ifdef INSTANCING_ON

        #define VERTEX_ANIMATION_IN \
            uint vid : SV_VertexID; \
            UNITY_VERTEX_INPUT_INSTANCE_ID

        #define VERTEX_ANIMATION_OUT \
            UNITY_VERTEX_INPUT_INSTANCE_ID

        #define VERTEX_ANIMATION_SURFACE_VERTEX_INPUT \
            struct appdata_full_instacing { \
                float4 vertex : POSITION; \
                float4 tangent : TANGENT; \
                float3 normal : NORMAL; \
                float4 texcoord : TEXCOORD0; \
                float4 texcoord1 : TEXCOORD1; \
                float4 texcoord2 : TEXCOORD2; \
                float4 texcoord3 : TEXCOORD3; \
                fixed4 color : COLOR; \
                VERTEX_ANIMATION_IN \
            };

        #define VERTEX_ANIMATION_PROPS(aniTex) \
            sampler2D aniTex; \
            float4 aniTex##_TexelSize; \
             \
            uint _VertexCount; \
            int _VertexMax; \
            int _VertexMin; \
             \
            UNITY_INSTANCING_BUFFER_START(Props) \
                UNITY_DEFINE_INSTANCED_PROP(uint, _FrameOffset) \
                UNITY_DEFINE_INSTANCED_PROP(uint, _PixelOffset) \
                UNITY_DEFINE_INSTANCED_PROP(uint, _FrameCount) \
                UNITY_DEFINE_INSTANCED_PROP(uint, _SampleRate) \
            UNITY_INSTANCING_BUFFER_END(Props)

        #define VERTEX_ANIMATION_VERT(objPos, aniTex) \
            UNITY_SETUP_INSTANCE_ID(v); \
            UNITY_TRANSFER_INSTANCE_ID(v, o); \
            VERTEX_ANIMATION_SURFACE_VERT(objPos, aniTex)

        #define VERTEX_ANIMATION_SURFACE_VERT(objPos, aniTex) \
            uint frameOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameOffset); \
            uint pixelOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _PixelOffset); \
            uint frameCount = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameCount); \
            uint sampleRate = UNITY_ACCESS_INSTANCED_PROP(Props, _SampleRate); \
				            \
            uint frame = fmod(frameOffset + ceil(_Time.y * sampleRate), frameCount); \
            uint offset = pixelOffset; \
            \
            uint pixelIndex = offset + v.vid + frame * _VertexCount; \
            uint px = pixelIndex % (uint)aniTex##_TexelSize.z; \
            uint py = pixelIndex * aniTex##_TexelSize.y; \
            float coordX = px * aniTex##_TexelSize.x; \
            float coordY = py * aniTex##_TexelSize.y; \
            \
            objPos = tex2Dlod(aniTex, float4(coordX, coordY, 0, 0)) * (_VertexMax - _VertexMin) + _VertexMin;

    #else

        #define VERTEX_ANIMATION_IN 
        #define VERTEX_ANIMATION_OUT 
        #define VERTEX_ANIMATION_SURFACE_VERTEX_INPUT \
            struct appdata_full_instacing { \
            float4 vertex : POSITION; \
            float4 tangent : TANGENT; \
            float3 normal : NORMAL; \
            float4 texcoord : TEXCOORD0; \
            float4 texcoord1 : TEXCOORD1; \
            float4 texcoord2 : TEXCOORD2; \
            float4 texcoord3 : TEXCOORD3; \
            fixed4 color : COLOR; \
            };
        #define VERTEX_ANIMATION_PROPS(aniTex)
        #define VERTEX_ANIMATION_VERT(objPos, aniTex) \
            objPos = float4(0,0,0,0);
        #define VERTEX_ANIMATION_SURFACE_VERT(objPos, aniTex) \
            objPos = float4(0,0,0,0);

    #endif

#endif