Shader "Custom/RoundingCubeShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Roundness("Roundness", Float) = 0
        _XSize("XSize", int) = 2
        _YSize("YSize", int) = 2
        _ZSize("ZSize", int) = 2

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Roundness;
            int _XSize;
            int _YSize;
            int _ZSize;

            v2f vert(appdata v)
            {
                v2f o;
                float3 inner = float3(v.vertex.x, v.vertex.y, v.vertex.z);

                float3 upperLimits = float3(_XSize / 2, _YSize / 2, _ZSize / 2);
                if (inner.x < -upperLimits.x + _Roundness)
                {
                    inner.x = -upperLimits.x + _Roundness;
                }
                else if (inner.x > upperLimits.x - _Roundness)
                {
                    inner.x = upperLimits.x - _Roundness;
                }
                if (inner.y < -upperLimits.y + _Roundness)
                {
                    inner.y = -upperLimits.y + _Roundness;
                }
                else if (inner.y > upperLimits.y - _Roundness)
                {
                    inner.y = upperLimits.y - _Roundness;
                }
                if (inner.z < -upperLimits.z + _Roundness)
                {
                    inner.z = -upperLimits.z + _Roundness;
                }
                else if (inner.z > upperLimits.z - _Roundness)
                {
                    inner.z = upperLimits.z - _Roundness;
                }

                float3 normal = 0;
                if (_Roundness != 0)
                {
                    normal = normalize(v.vertex - inner);
                }

                float4 final = float4(inner + normal * _Roundness, 1);
                o.vertex = UnityObjectToClipPos(final);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                    // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
