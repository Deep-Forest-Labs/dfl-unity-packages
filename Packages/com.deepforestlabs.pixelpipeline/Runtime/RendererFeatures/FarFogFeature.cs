#nullable enable
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace DeepForestLabs.PixelPipeline
{
    /// <summary>
    /// Runs on the Far camera after transparents. Reads the camera depth texture
    /// and the current far color, applies depth-based color desaturation/tint (D)
    /// and Bayer-dithered distance fog (A). Each sub-effect is independently
    /// togglable via global float uniforms.
    /// </summary>
    public class FarFogFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader farFogShader;
        [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;

        private Material material;
        private FarFogPass pass;

        public override void Create()
        {
            if (farFogShader == null)
                farFogShader = Shader.Find("DeepForestLabs/PixelPipeline/FarFog");

            if (farFogShader != null)
            {
                if (material == null || material.shader != farFogShader)
                {
                    CoreUtils.Destroy(material);
                    material = CoreUtils.CreateEngineMaterial(farFogShader);
                }
            }

            pass = new FarFogPass(material)
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

        private class FarFogPass : ScriptableRenderPass
        {
            private readonly Material material;

            public FarFogPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("PixelPipeline.FarFog");
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
                    "PixelPipeline.FarFog", out var data, profilingSampler))
                {
                    data.material = material;

                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);

                    if (resourceData.cameraDepthTexture.IsValid())
                        builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);

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
