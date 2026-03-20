Shader "TextMeshPro/Sprite" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_CullMode ("Cull Mode", Float) = 0
		_ColorMask ("Color Mask", Float) = 15
		_ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
	//DummyShaderTextExporter
// 	SubShader{
// 		Tags { "RenderType"="Opaque" }
// 		LOD 200
// 		CGPROGRAM
// #pragma surface surf Standard
// #pragma target 3.0

// 		sampler2D _MainTex;
// 		fixed4 _Color;
// 		struct Input
// 		{
// 			float2 uv_MainTex;
// 		};
		
// 		void surf(Input IN, inout SurfaceOutputStandard o)
// 		{
// 			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
// 			o.Albedo = c.rgb;
// 			o.Alpha = c.a;
// 		}
// 		ENDCG
// 	}

	SubShader{
		// 1. Chữ phải nằm trong hàng đợi Transparent để hiển thị đúng độ mượt cạnh
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200

		// Khai báo lại Blend để xóa bỏ các ô vuông đen xung quanh chữ
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// 2. Thêm alpha:fade để hỗ trợ độ trong suốt
		#pragma surface surf Standard alpha:fade
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			
			// 3. Thuật toán SDF nâng cao cho TextMeshPro:
			// Lấy giá trị từ kênh Alpha (SDF lưu trữ khoảng cách trong alpha)
			float distance = tex.a;
			
			// Bạn có thể chỉnh các thông số này để chữ dày hơn hoặc mềm hơn:
			float smoothing = 0.05; // Độ mượt cạnh (Softness)
			float threshold = 0.5;  // Độ dày chữ (Dilate) - Càng thấp chữ càng dày

			float alpha = smoothstep(threshold - smoothing, threshold + smoothing, distance);

			o.Albedo = _Color.rgb; // Sử dụng màu Tint
			o.Alpha = alpha * _Color.a; // Trả về độ trong suốt cho cạnh chữ
		}
		ENDCG
	}

}