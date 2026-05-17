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

            public ColorQuantizationPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("PixelPipeline.ColorQuantization");
            }

            private class PassData
            {
                public Material material;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (material == null) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                if (!resourceData.activeColorTexture.IsValid()) return;

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "PixelPipeline.ColorQuantization", out var data, profilingSampler))
                {
                    data.material = material;

                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                    builder.AllowGlobalStateModification(true);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc(static (PassData d, RasterGraphContext ctx) =>
                    {
                        Blitter.BlitTexture(ctx.cmd, new Vector4(1f, 1f, 0f, 0f), d.material, 0);
                    });
                }
            }
        }
    }
}
#nullable disable
