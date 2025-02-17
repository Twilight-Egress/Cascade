﻿using Cascade.Core.Graphics.GraphicalObjects.SkyEntities;

namespace Cascade.Content.Skies.SkyEntities.TravellingAsteroid
{
    public class TravellingCosmostoneAsteroidSmall : SkyEntity
    {
        private readonly float ShaderTimeMultiplier;

        public TravellingCosmostoneAsteroidSmall(Vector2 position, Vector2 velocity, float scale, float depth, float rotationSpeed, int lifespan)
        {
            Position = position;
            Velocity = velocity;
            Scale = new(scale);
            Depth = depth;
            RotationSpeed = rotationSpeed;
            Lifetime = lifespan;

            Opacity = 0f;
            Frame = Main.rand.Next(3);           
            Rotation = Main.rand.NextFloat(Tau);
            ShaderTimeMultiplier = Main.rand.NextFloat(0.1f, 1.5f) * Main.rand.NextBool().ToDirectionInt();
        }

        public override string AtlasTextureName => "Cascade.EmptyPixel.png";

        public override int MaxVerticalFrames => 3;

        public override void Update()
        {
            int timeToDisappear = Lifetime - 60;

            // Fade in and out.
            if (Time < timeToDisappear)
                Opacity = Clamp(Opacity + 0.1f, 0f, 1f);
            if (Time >= timeToDisappear && Time <= Lifetime)
                Opacity = Clamp(Opacity - 0.1f, 0f, 1f);

            Rotation += RotationSpeed * Velocity.X * 0.006f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D asteroidTexture = ModContent.Request<Texture2D>("Cascade/Content/NPCs/CosmostoneShowers/Asteroids/CosmostoneAsteroidSmall").Value;
            Texture2D glowmask = ModContent.Request<Texture2D>("Cascade/Content/NPCs/CosmostoneShowers/Asteroids/CosmostoneAsteroidSmall_Glowmask").Value;

            Rectangle frameRectangle = asteroidTexture.Frame(1, MaxVerticalFrames, 0, Frame % MaxVerticalFrames);
            Vector2 mainOrigin = frameRectangle.Size() / 2f;
            Color color = Color.Lerp(Color.White, Color.Black, 0.15f + Depth / 10f) * Opacity;

            // Draw the main sprite.
            spriteBatch.Draw(asteroidTexture, GetDrawPositionBasedOnDepth(), frameRectangle, color, Rotation, mainOrigin, Scale / Depth, 0, 0f);

            spriteBatch.PrepareForShaders();
            ManagedShader shader = ShaderManager.GetShader("Cascade.ManaPaletteShader");
            shader.TrySetParameter("flowCompactness", 3.0f);
            shader.TrySetParameter("gradientPrecision", 10f);
            shader.TrySetParameter("timeMultiplier", ShaderTimeMultiplier);
            shader.TrySetParameter("palette", CascadeUtilities.CosmostonePalette);
            shader.TrySetParameter("opacity", Opacity);
            shader.Apply();

            // Draw the glowmask with the shader applied.
            spriteBatch.Draw(glowmask, GetDrawPositionBasedOnDepth(), frameRectangle, Color.White, Rotation, mainOrigin, Scale / Depth, 0, 0f);
            spriteBatch.ResetToDefault();
        }
    }
}
