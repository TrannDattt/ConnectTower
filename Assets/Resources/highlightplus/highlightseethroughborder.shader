Shader "HighlightPlus/Geometry/SeeThroughBorder" {
	Properties {
		_MainTex ("Texture", any) = "white" {}
		_SeeThroughBorderColor ("Outline Color", Vector) = (0,0,0,1)
		_Color ("Color", Vector) = (1,1,1,1)
		_CutOff ("CutOff", Float) = 0.5
		_SeeThroughBorderWidth ("Outline Offset", Float) = 0.01
		_SeeThroughBorderConstantWidth ("Constant Width", Float) = 1
		_SeeThroughStencilRef ("Stencil Ref", Float) = 2
		_SeeThroughStencilComp ("Stencil Comp", Float) = 5
		_SeeThroughDepthOffset ("Depth Offset", Float) = 0
		_SeeThroughMaxDepth ("Max Depth", Float) = 0
		_SeeThroughStencilPassOp ("Stencil Pass Operation", Float) = 0
		_Cull ("Cull Mode", Float) = 2
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