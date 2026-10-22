Shader "Custom/CurvedDiffuse"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert addshadow
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        fixed4 _Color;
        uniform float _CurveStrength;
        uniform float _CurveStartOffset;
        uniform float4 _CurveOrigin;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert(inout appdata_full v)
        {
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            // Distance ahead along the track (+Z) relative to player/origin
            float distZ = (worldPos.z - _CurveOrigin.z) - _CurveStartOffset;

            if (distZ > 0.0)
            {
                worldPos.y -= distZ * distZ * _CurveStrength;
            }

            v.vertex = mul(unity_WorldToObject, worldPos);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Mobile/Diffuse"
}
