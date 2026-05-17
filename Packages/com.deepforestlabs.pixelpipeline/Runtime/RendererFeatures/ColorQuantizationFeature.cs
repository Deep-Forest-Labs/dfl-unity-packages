#nullable enable
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DeepForestLabs.PixelPipeline
{
    /// <summary>
    /// Runs on the Composite camera after post-processing. Quantizes the final LDR
    /// image colors using ordered dithering with a 4x4 Bayer matrix. Optionally
    /// excludes viewmodel pixels using the _ViewmodelRT alpha channel.
    /// Dither pattern aligns to the pixel-art grid via _PixelPipelinePixelScale.
    /// </summary>
    public class ColorQuantizationFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader colorQuantizationShader;
        [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

        private Material material;
        private ColorQuantizationPass pass;

        public override void Create()
        {
            if (colorQuantizationShader == null)
                colorQuantizationShader = Shader.Find("DeepForestLabs/PixelPipeline/ColorQuantization");

            if (colorQuantizationShader != null)
            {
                if (material == null || material.shader != colorQuantizationShader)
                {
                    CoreUtils.Destroy(material);
                    material = CoreUtils.CreateEngineMaterial(colorQuantizationShader);
                }
            }

            pass = new ColorQuantizationPass(material)
            {
                renderPassEvent = injectionPoint
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
            material = null;
        }

        private class ColorQuantizationPass : ScriptableRenderPass
        {
            private readonly Material material;
            private static readonly MaterialPropertyBlock s_PropertyBlock = new();

            public ColorQuantizationPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("PixelPipeline.ColorQuantization");
            }

            private class PassData
            {
                public Material material;
                public TextureHandle inputTexture;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                if (!resourceData.activeColorTexture.IsValid()) return;

                var targetDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
                targetDesc.name = "ColorQuantization_TempColor";
                targetDesc.clearBuffer = false;
                var tempCopy = renderGraph.CreateTexture(targetDesc);

                renderGraph.AddBlitPass(resourceData.activeColorTexture, tempCopy,
                    Vector2.one, Vector2.zero, passName: "PixelPipeline.ColorQuantization.CopyColor");

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "PixelPipeline.ColorQuantization", out var data, profilingSampler))
                {
                    data.material = material;
                    data.inputTexture = tempCopy;

                    builder.UseTexture(tempCopy, AccessFlags.Read);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                    builder.AllowGlobalStateModification(true);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc(static (PassData d, RasterGraphContext ctx) =>
                    {
                        s_PropertyBlock.Clear();
                        s_PropertyBlock.SetTexture(Shader.PropertyToID("_BlitTexture"), d.inputTexture);
                        s_PropertyBlock.SetVector(Shader.PropertyToID("_BlitScaleBias"),
                            new Vector4(1f, 1f, 0f, 0f));

                        ctx.cmd.DrawProcedural(Matrix4x4.identity, d.material, 0,
                            MeshTopology.Triangles, 3, 1, s_PropertyBlock);
                    });
                }
            }
        }
    }
}
#nullable disable
