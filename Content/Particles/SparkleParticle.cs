﻿namespace Cascade.Content.Particles
{
    public class SparkleParticle : CasParticle
    {
        public bool AdditiveBlending;

        public int VisualStyle;

        public Color BloomColor;

        public float BloomScale;

        private AtlasTexture StarTextureToDraw;

        public override string AtlasTextureName => "Cascade.Sparkle.png";

        public override BlendState BlendState => AdditiveBlending ? BlendState.Additive : BlendState.AlphaBlend;

        public SparkleParticle(Vector2 position, Vector2 velocity, Color drawColor, Color bloomColor, float scale, int lifeTime, float rotationSpeed = 0f, float bloomScale = 1f, bool additiveBlending = true, int? visualStyle = null)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = drawColor;
            BloomColor = bloomColor;
            Scale = new(scale);
            Lifetime = lifeTime;
            RotationSpeed = rotationSpeed;
            BloomScale = bloomScale;
            AdditiveBlending = additiveBlending;
            VisualStyle = visualStyle ?? Main.rand.NextBool().ToDirectionInt();
        }

        public override void Update()
        {
            // Fade in and out.
            Opacity = Sin(LifetimeRatio * Pi);

            // Slow down and spin.
            Velocity *= 0.98f;
            Rotation += RotationSpeed * Velocity.X.DirectionalSign();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            StarTextureToDraw = VisualStyle == -1 ? Texture : AtlasManager.GetTexture("Cascade.Sparkle2.png");
            AtlasTexture bloomTexture = AtlasManager.GetTexture("Cascade.BloomFlare.png");

            spriteBatch.Draw(bloomTexture, Position - Main.screenPosition, null, BloomColor * Opacity * 0.5f, Rotation, null, Scale / 2f);
            spriteBatch.Draw(StarTextureToDraw, Position - Main.screenPosition, null, DrawColor * Opacity * 0.5f, Rotation, null, Scale * 0.75f);
            spriteBatch.Draw(StarTextureToDraw, Position - Main.screenPosition, null, DrawColor * Opacity, Rotation, null, Scale);
        }
    }
}
