Shader "HighlightPlus/Geometry/ComposeOutline" {
	Properties {
		_MainTex ("Texture", any) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_Cull ("Cull Mode", Float) = 2
		_ZTest ("ZTest Mode", Float) = 0
		_Flip ("Flip", Vector) = (0,1,0,1)
		_Debug ("Debug Color", Vector) = (0,0,0,0)
		_OutlineStencilComp ("Stencil Comp", Float) = 6
		_OutlineSharpness ("Outline Sharpness", Float) = 1
		_PatternTex ("Pattern Texture", 2D) = "black" {}
		_PatternData ("Pattern Data", Vector) = (1,0.5,0.1,0)
		_DistortionTex ("Distortion Texture", 2D) = "white" {}
		_DashData ("Dash Data", Vector) = (0.1,0.1,1,0.1)
		_OutlineGradientTex ("Outline Gradient Texture", 2D) = "white" {}
		_OutlineGradientData ("Outline Gradient Data", Vector) = (0.5,1,0,0)
		_Padding ("Padding", Float) = 0
		_Pixelation ("Pixelation", Float) = 0
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