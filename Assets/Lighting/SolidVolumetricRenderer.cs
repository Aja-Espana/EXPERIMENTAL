using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class SolidVolumetricRenderer : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public bool enabled = true;
        public Color globalTint = Color.white;
        public float maxRange = 20f;
    }

    public Settings settings = new Settings();
    public Material volumetricMaterial;

    class VolumetricPass : ScriptableRenderPass
    {
        public Material material;
        public Settings settings;

        // Container to pass frame data into the execution lambda safely
        private class PassData
        {
            public Material material;
            public Color globalTint;
            public float maxRange;
            public int lightCount;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!settings.enabled || material == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SolidVolumetrics", out var passData))
            {
                // Count active spotlights on scene
                Light[] activeLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
                int lightCount = 0;
                foreach (var light in activeLights)
                {
                    if (light.enabled && light.type == LightType.Spot)
                        lightCount++;
                }

                // Populate pass data
                passData.material = material;
                passData.globalTint = settings.globalTint;
                passData.maxRange = settings.maxRange;
                passData.lightCount = lightCount;

                // Bind active target color texture to render pass
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // Define execution step
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    data.material.SetColor("_GlobalTint", data.globalTint);
                    data.material.SetFloat("_MaxRange", data.maxRange);
                    data.material.SetInt("_LightCount", data.lightCount);

                    context.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1);
                });
            }
        }
    }

    VolumetricPass m_Pass;

    public override void Create()
    {
        m_Pass = new VolumetricPass();
        m_Pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (volumetricMaterial == null) return;

        m_Pass.material = volumetricMaterial;
        m_Pass.settings = settings;
        renderer.EnqueuePass(m_Pass);
    }
}