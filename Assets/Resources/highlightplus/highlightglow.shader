Shader "HighlightPlus/Geometry/Glow" {
	Properties {
		_MainTex ("Texture", any) = "white" {}
		_Glow2 ("Glow2", Vector) = (0.01,1,0.5,0)
		_Color ("Color", Vector) = (1,1,1,1)
		_Cull ("Cull Mode", Float) = 2
		_ConstantWidth ("Constant Width", Float) = 1
		_MinimumWidth ("Minimum Width", Float) = 0
		_GlowZTest ("ZTest", Float) = 4
		_GlowStencilOp ("Stencil Operation", Float) = 0
		_CutOff ("CutOff", Float) = 0.5
		_GlowStencilComp ("Stencil Comp", Float) = 6
		_NoiseTex ("Noise Tex", 2D) = "white" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}