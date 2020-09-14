Shader "Lexdev/UI/Blur"
{
    Properties
    {
		_Radius("Blur radius", Range(0, 20)) = 1
		_Color("Color", COLOR) = (1,1,1,1)
    }
    SubShader
    {
		Tags{ "Queue" = "Transparent"}
		HLSLINCLUDE
			#define SampleGrabTexture(posX, posY) tex2Dproj( _GrabTexture, float4(i.screenPos.x + _GrabTexture_TexelSize.x * posX, i.screenPos.y + _GrabTexture_TexelSize.y * posY, i.screenPos.z, i.screenPos.w))
		ENDHLSL

		GrabPass {}
        Pass
        {
            HLSLPROGRAM
				
            #pragma vertex vert
            #pragma fragment frag
				
            #include "UnityCG.cginc"
 
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
 
            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
            };

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			int _Radius;

			sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.screenPos.y = 1.0f - o.screenPos.y;
				o.uv = v.uv;
                return o;
            }
 
			half4 frag(v2f i) : SV_TARGET
			{
				if (tex2D(_MainTex, i.uv).a < 0.5f)
					discard;

				half4 result = SampleGrabTexture(0, 0);
				for (int range = 1; range <= _Radius; range++)
				{
					result += SampleGrabTexture(0, range);
					result += SampleGrabTexture(0, -range);
				}
				result /= _Radius * 2 + 1;

				return result;
            }

            ENDHLSL
        }
			
		GrabPass {}
		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float4 screenPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
			};

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			int _Radius;

			sampler2D _MainTex;
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.screenPos.y = 1.0f - o.screenPos.y;
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			half4 frag(v2f i) : SV_TARGET
			{
				if (tex2D(_MainTex, i.uv).a < 0.5f)
					discard;

				half4 result = SampleGrabTexture(0, 0);
				for (int range = 1; range <= _Radius; range++)
				{
					result += SampleGrabTexture(range, 0);
					result += SampleGrabTexture(-range, 0);
				}
				result /= _Radius * 2 + 1;

				_Color *= i.color;
				return half4(_Color.a * _Color.rgb + (1 - _Color.a) * result.rgb, 1.0f);
			}

			ENDHLSL
		}
    }
}