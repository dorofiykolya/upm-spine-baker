// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Unlit/TextureAnimation" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BoundingMax("Bounding Max", Vector) = (0,0,0,0)
		_BoundingMin("Bounding Min", Vector) = (0,0,0,0)
		_NumOfFrames("Number Of Frames", int) = 0
		_Speed("Speed", Float) = 0.33
		_PosTex ("Position Map (RGB)", 2D) = "white" {}
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0
		_TimeOffset ("TimeOffset", Float) = 0
		_VertexColorTex ("VertexColor", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

		Pass
		{
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _PosTex;
			sampler2D _VertexColorTex;
			uniform float4 _BoundingMax;
			uniform float4 _BoundingMin;
			uniform float _Speed;
			uniform int _NumOfFrames;
			uniform half _Cutoff;
			uniform float _TimeOffset;
			uniform half4 _Color;

			struct Vertex
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD;
				float4 uv2 : TEXCOORD1;
			};

			struct Fragment
			{
				float4 uv : TEXCOORD;
				float4 color : COLOR;
				float4 vertex : SV_POSITION; 
			};
			
			Fragment vert(Vertex v)
			{
				Fragment o;

				float time = _Time.g * _Speed + _TimeOffset;
				float frames = _NumOfFrames;
				float frameTime = 1.0 / frames;
				float totalTime = frames * frameTime;

				float count = floor(time / totalTime);
				time = time - totalTime * count;
				float currentFrame = floor(time / frameTime);
				float nextFrame = currentFrame + 1;
				float lerpFrame = time - (currentFrame / frames);
				float ratio = saturate(lerpFrame / frameTime);

				float4 currUV = float4(v.uv2.x, v.uv2.y + currentFrame / frames, 0, 0);
				float4 nextUV = float4(v.uv2.x, v.uv2.y + nextFrame / frames, 0, 0);
				
				float4 currPos = tex2Dlod(_PosTex, currUV);
				float4 nextPos = tex2Dlod(_PosTex, nextUV);

				float4 currColor = tex2Dlod(_VertexColorTex, currUV);
				float4 nextColor = tex2Dlod(_VertexColorTex, nextUV);

				float3 expand = _BoundingMax.rgb - _BoundingMin.rgb;

				float4 offset = lerp(currPos, nextPos, ratio);
				float4 color = lerp(currColor, nextColor, ratio);

				v.vertex.xyz += (_BoundingMin + expand * offset.rgb);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.uv.xy;
				o.uv.z = offset.a;
				o.color = color;
				return o;
			}

			fixed4 frag(Fragment i) : SV_Target
			{
				float4 c = tex2D (_MainTex, i.uv.xy) * i.color;
				clip(c.a - _Cutoff);
				c.a *= i.uv.z;
				c *= _Color;
				return c;
			}
			ENDCG
		}
	}
	FallBack "Unlit/Texture"
}
