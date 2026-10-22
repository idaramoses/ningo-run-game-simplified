Shader "Custom/CurvedStandardAlpha"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert alpha:fade fullforwardshadows
        #pragma multi_compile_instancing
        #pragma target 3.0

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
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
            float distZ = (worldPos.z - _CurveOrigin.z) - _CurveStartOffset;

            if (distZ > 0.0)
            {
                worldPos.y -= distZ * distZ * _CurveStrength;
            }

            v.vertex = mul(unity_WorldToObject, worldPos);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a * _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/Cutout/Diffuse"
}
