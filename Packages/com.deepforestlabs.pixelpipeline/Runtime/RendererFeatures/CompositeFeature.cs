#nullable enable
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DeepForestLabs.PixelPipeline
{
    /// <summary>
    /// Runs on the Composite camera before post-processing. Writes a fullscreen
    /// blit that reads _FarRT and _NearRT (set as global textures by
    /// PixelPipelineController) and lerps between them using the alpha channel
    /// of _NearRT. Point sampling preserves the pixel look.
    /// </summary>
    public class CompositeFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader compositeShader;
        [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;

        private Material material;
        private CompositePass pass;

        public override void Create()
        {
            if (compositeShader == null)
                compositeShader = Shader.Find("DeepForestLabs/PixelPipeline/Composite");

            if (compositeShader != null)
            {
                if (material == null || material.shader != compositeShader)
                {
                    CoreUtils.Destroy(material);
                    material = CoreUtils.CreateEngineMaterial(compositeShader);
                }
            }

            pass = new CompositePass(material)
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

        private class CompositePass : ScriptableRenderPass
        {
            private readonly Material material;

            public CompositePass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("PixelPipeline.Composite");
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

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("PixelPipeline.Composite", out var data))
                {
                    data.material = material;

                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                    builder.AllowGlobalStateModification(true);
                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                    {
                        Blitter.BlitTexture(ctx.cmd, new Vector4(1f, 1f, 0f, 0f), d.material, 0);
                    });
                }
            }
        }
    }
}
#nullable disable
