Shader "Custom/WarpVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PinchCenter ("Pinch Center", Vector) = (0.5, 0.5, 0, 0)
        _PinchStrength ("Pinch Strength", Float) = 0.5
        _PinchRadius ("Pinch Radius", Float) = 0.5
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
            float4 _PinchCenter;
            float _PinchStrength;
            float _PinchRadius;
            float _AspectRatio;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.texcoord;
                float2 center = _PinchCenter.xy;
                float2 adjustedCenter = center / float2(_AspectRatio, 1.0);

                float dist = distance(uv, adjustedCenter);
                if (_PinchRadius > 0.0 && _PinchStrength > 0.0 && dist < _PinchRadius)
                {
                    float factor = pow((_PinchRadius - dist) / _PinchRadius, _PinchStrength);
                    uv = adjustedCenter + (uv - adjustedCenter) * (1.0 - factor);
                }

                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}