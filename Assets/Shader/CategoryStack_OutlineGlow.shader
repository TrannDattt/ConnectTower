Shader "CategoryStack/OutlineGlow" {
	Properties {
		[HDR] _GlowColor ("Glow Color", Vector) = (1,0.8,0,1)
		_GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.5
		_GlowWidth ("Glow Width", Range(0, 0.2)) = 0.05
		_Softness ("Softness", Range(0.01, 1)) = 0.5
		_DepthBias ("Depth Bias", Range(-1, 1)) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
}