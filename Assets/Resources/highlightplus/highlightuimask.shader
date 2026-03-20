Shader "HighlightPlus/UI/Mask" {
	Properties {
		_MainTex ("Texture", any) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_CutOff ("CutOff", Float) = 0.5
		_Stencil ("Stencil ID", Float) = 14
		_StencilWriteMask ("Stencil Write Mask", Float) = 14
		_StencilReadMask ("Stencil Read Mask", Float) = 14
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