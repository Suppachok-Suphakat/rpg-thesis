Shader "Custom/ShadowEffect"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1) // Shadow color (darker)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        // Blending settings for transparency
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ShadowColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the texture (original sprite)
                half4 col = tex2D(_MainTex, i.texcoord);

                // Only apply shadow effect to non-transparent pixels (preserve alpha)
                half4 shadow = lerp(col, _ShadowColor, 0.5);
                shadow.a = col.a; // Preserve the original alpha (transparency)

                return shadow;
            }
            ENDCG
        }
    }
}
