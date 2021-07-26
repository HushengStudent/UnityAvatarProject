// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Avatar/Diffuse" 
{
    Properties 
    {
        _Texture_jiao ("_Texture_jiao", 2D) = "white" {}
        _Texture_shen ("_Texture_shen", 2D) = "white" {}
        _Texture_shou ("_Texture_shou", 2D) = "white" {}
        _Texture_tou ("_Texture_tou", 2D) = "white" {}
    }

    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Texture_jiao;
            float4 _Texture_jiao_ST;

            sampler2D _Texture_shen;
            float4 _Texture_shen_ST;

            sampler2D _Texture_shou;
            float4 _Texture_shou_ST;

            sampler2D _Texture_tou;
            float4 _Texture_tou_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture_jiao);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed jiao_mask = (1 - step(0.5,i.uv.x)) * (1 - step(0.5,i.uv.y));
                fixed2 jiao_uv = fixed2(i.uv.x,i.uv.y)*2;
                fixed4 jiao_color = tex2D(_Texture_jiao, saturate(jiao_uv)) * jiao_mask;

                fixed shen_mask = step(0.5,i.uv.x) * (1 - step(0.5,i.uv.y));
                fixed2 shen_uv = fixed2(i.uv.x-0.5,i.uv.y)*2;
                fixed4 shen_color = tex2D(_Texture_shen, saturate(shen_uv)) * shen_mask;

                fixed shou_mask = (1 - step(0.5,i.uv.x)) * step(0.5,i.uv.y);
                fixed2 shou_uv = fixed2(i.uv.x,i.uv.y-0.5)*2;
                fixed4 shou_color = tex2D(_Texture_shou, saturate(shou_uv)) * shou_mask;

                fixed tou_mask = step(0.5,i.uv.x) * step(0.5,i.uv.y);
                fixed2 tou_uv = fixed2(i.uv.x-0.5,i.uv.y-0.5)*2;
                fixed4 tou_color = tex2D(_Texture_tou, saturate(tou_uv)) * tou_mask;

                return jiao_color + shen_color + shou_color + tou_color;
            }
            ENDCG
        }
    }
}