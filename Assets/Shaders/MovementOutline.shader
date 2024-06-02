Shader "Custom/MovementOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderColor ("Border Color", Color) = (1,1,1,1)
        _BorderThickness ("Border Thickness", Float) = 0.1
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BorderColor;
            float _BorderThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture color
                fixed4 col = tex2D(_MainTex, i.uv);

                // Determine the distance to the nearest edge
                float2 distToEdge = min(i.uv, 1 - i.uv);
                float nearestEdgeDist = min(distToEdge.x, distToEdge.y);

                // If within the border thickness, blend with the border color
                if (nearestEdgeDist < _BorderThickness)
                {
                    col = lerp(col, _BorderColor, (1 - nearestEdgeDist / _BorderThickness));
                }

                return col;
            }
            ENDCG
        }
    }
}
