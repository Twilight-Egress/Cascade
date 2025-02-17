﻿using Cascade.Core.Graphics;

namespace Cascade.Content.Particles
{
    public class LightningArcParticle : CasParticle
    {
        public float PointDisplacementVariance;

        public float JaggednessNumerator;

        public bool UseSmoothening;

        public bool Initialized;

        public bool AdditiveBlending;

        public Vector2 EndPosition;

        public List<Vector2> LightningPoints;

        public override string AtlasTextureName => "Cascade.EmptyPixel.png";

        public LightningArcParticle(Vector2 basePosition, Vector2 endPosition, float pointDisplacementVariance, float jaggednessNumerator, float scale, Color color, int lifespan, bool useSmoothening = false, bool additiveBlending = true)
        {
            Position = basePosition;
            EndPosition = endPosition;
            PointDisplacementVariance = pointDisplacementVariance;
            JaggednessNumerator = jaggednessNumerator;
            Lifetime = lifespan;
            Scale = new(scale, scale);
            DrawColor = color;
            UseSmoothening = useSmoothening;
            AdditiveBlending = additiveBlending;
        }

        public override void Update()
        {
            if (!Initialized)
            {
                Initialized = true;
                LightningPoints = CascadeUtilities.CreateLightningBoltPoints(Position, EndPosition, PointDisplacementVariance, JaggednessNumerator);
            }
        }

        public float LightningWidthFunction(float trailLengthInterpolant) => Scale.X * Utils.GetLerpValue(1f, 0f, trailLengthInterpolant, true) * Lerp(1f, 0f, LifetimeRatio);

        public Color LightningColorFunction(float trailLengthInterpolant) => DrawColor;

        public override void Draw(SpriteBatch spriteBatch)
        {
            ShaderManager.TryGetShader("Luminance.StandardPrimitiveShader", out ManagedShader smoothTrail);
            smoothTrail.SetTexture(CascadeTextureRegistry.ThinGlowStreak, 1, SamplerState.LinearWrap);

            PrimitiveSettings settings = new(LightningWidthFunction, LightningColorFunction, null, false, false, smoothTrail);
            PrimitiveRenderer.RenderTrail(LightningPoints, settings, LightningPoints.Count);

            spriteBatch.ExitShaderRegion();
        }
    }
}
