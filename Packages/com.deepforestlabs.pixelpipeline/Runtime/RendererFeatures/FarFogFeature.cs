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
            private static readonly MaterialPropertyBlock s_PropertyBlock = new();

            public FarFogPass(Material material)
            {
                this.material = material;
                profilingSampler = new ProfilingSampler("PixelPipeline.FarFog");
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
                targetDesc.name = "FarFog_TempColor";
                targetDesc.clearBuffer = false;
                var tempCopy = renderGraph.CreateTexture(targetDesc);

                renderGraph.AddBlitPass(resourceData.activeColorTexture, tempCopy,
                    Vector2.one, Vector2.zero, passName: "PixelPipeline.FarFog.CopyColor");

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "PixelPipeline.FarFog", out var data, profilingSampler))
                {
                    data.material = material;
                    data.inputTexture = tempCopy;

                    builder.UseTexture(tempCopy, AccessFlags.Read);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

                    if (resourceData.cameraDepthTexture.IsValid())
                        builder.UseTexture(resourceData.cameraDepthTexture);

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
