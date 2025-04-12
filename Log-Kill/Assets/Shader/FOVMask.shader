Shader "Custom/FOVMask"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        ColorMask 0 // 컬러 출력 없음
        ZWrite Off

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass 
        {
            Cull Off
        }
    }
}
