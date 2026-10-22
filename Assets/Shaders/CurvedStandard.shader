Shader "Custom/CurvedStandard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BumpMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows addshadow
        #pragma multi_compile_instancing
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        uniform float _CurveStrength;
        uniform float _CurveStartOffset;
        uniform float4 _CurveOrigin;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
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
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Standard"
}
