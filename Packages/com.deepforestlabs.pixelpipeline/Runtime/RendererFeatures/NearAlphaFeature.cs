#nullable enable
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DeepForestLabs.PixelPipeline
{
    /// <summary>
    /// Runs on the Near camera after opaques+transparents. Reads the camera depth
    /// texture and rewrites only the alpha channel of the Near color RT with a
    /// Bayer-dithered mask based on distance fade parameters. The composite pass
    /// uses this alpha to mix the Near RT over the Far RT without a visible seam.
    /// </summary>
    public class NearAlphaFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader nearAlphaShader;
        [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;

        private Material material;
        private NearAlphaPass pass;

        public override void Create()
        {
            if (nearAlphaShader == null)
                nearAlphaShader = Shader.Find("DeepForestLabs/PixelPipeline/NearAlphaEncode");

            if (nearAlphaShader != null)
            {
                if (material == null || material.shader != nearAlphaShader)
                {
                    CoreUtils.Destroy(material);
                    material = CoreUtils.CreateEngineMaterial(nearAlphaShader);
                }
            }

            pass = new NearAlphaPass(material)
            {
                renderPassEvent = injectionPoint
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
            material = null;
        }

        private class NearAlphaPass : ScriptableRenderPass
        {
            private readonly Material material;

            public NearAlphaPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("PixelPipeline.NearAlphaEncode");
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

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("PixelPipeline.NearAlphaEncode", out var data))
                {
                    data.material = material;

                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                    if (resourceData.cameraDepthTexture.IsValid())
                        builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);

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
