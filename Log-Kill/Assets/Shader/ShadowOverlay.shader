Shader "Custom/ShadowOverlay"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0, 0, 0, 0.7)
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Keep
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
