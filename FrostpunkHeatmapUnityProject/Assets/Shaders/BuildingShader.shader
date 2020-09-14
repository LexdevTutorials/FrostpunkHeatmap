Shader "Lexdev/CaseStudies/FrostpunkHeatmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Emission ("Emission", Color) = (0.0, 0.0, 0.0, 0.0)
        [Toggle(ADD_SNOW)] _Snow("Add Snow", Float) = 0
        _SnowColor("Snow Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _SnowAngle("SnowAngle", float) = 0.0
    }
    SubShader
    {
        Pass
        {
            //Deferred rendering
            Tags {"LightMode" = "Deferred"}

            HLSLPROGRAM

            //Multiple shader variants for the heatmap and the slight snow effect
            #pragma multi_compile __ RENDER_HEAT
            #pragma shader_feature ADD_SNOW

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //Vertex data
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            //Vertex to fragment struct
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            //GBuffer output
            struct gbuffer
            {
                float4 albedo : SV_Target0;
                float4 specular : SV_Target1;
                float4 normal : SV_Target2;
                float4 emission : SV_Target3;
            };

            //Main Texture
            sampler2D _MainTex;
            float4 _MainTex_ST;

            //Emission
            float4 _Emission;

            //Snow
            float4 _SnowColor;
            float _SnowAngle;

            //Heatmap
            sampler2D _Mask;
            float _MapSize;

            //Simple vertex function, neet world position for heatmap sampling
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            //Fragment function, writes to the GBuffer
            gbuffer frag(v2f i)
            {
                gbuffer o;

                //Different variants for heatmap/normal rendering
                #if !defined(RENDER_HEAT)
                    //Different albedo color when snow effect is used
                    #if defined(ADD_SNOW)
                        float normalDot = dot(i.normal, float3(0.0f, 1.0f, 0.0f));
                        if (normalDot < _SnowAngle)
                            o.albedo = tex2D(_MainTex, i.uv);
                        else
                            o.albedo = _SnowColor;
                    #else
                        o.albedo = tex2D(_MainTex, i.uv);
                    #endif
                    o.specular = float4(0.0f, 0.0f, 0.0f, 0.0f); //No specular in our case
                    o.normal = float4(i.normal * 0.5f + 0.5f, 0.0f); //Normal needs to be remaped
                    o.emission = _Emission;
                #else
                    float4 mask = tex2D(_Mask, i.worldPos.xz / _MapSize + 0.5f);
                    o.albedo = float4(mask.rgb, 1.0f); //albedo is simply the mask value at the given world position
                    o.specular = float4(0.0f, 0.0f, 0.0f, 0.0f); //No specular in the heatmap view
                    o.normal = float4(i.normal * 0.5f + 0.5f, 0.0f);
                    o.emission = float4(0.0f, 0.0f, 0.0f, 1.0f); //No emission in the heatmap view
                #endif

                return o;
            }

            ENDHLSL
        }
    }
}
