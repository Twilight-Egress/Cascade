﻿using Cascade.Core.Graphics.GraphicalObjects.SkyEntities;

namespace Cascade.Content.Skies.SkyEntities.StationaryAsteroids
{
    public class StationarySilicateAsteroidSmall : SkyEntity
    {
        public StationarySilicateAsteroidSmall(Vector2 position, float scale, float depth, float rotationSpeed, int lifespan)
        {
            Position = position;
            Scale = new(scale);
            Depth = depth;
            RotationSpeed = rotationSpeed;
            Lifetime = lifespan;

            Opacity = 0f;
            Frame = Main.rand.Next(6);
            Rotation = Main.rand.NextFloat(Tau);
            RotationDirection = Main.rand.NextBool().ToDirectionInt();
        }

        public override string AtlasTextureName => "Cascade.EmptyPixel.png";

        public override int MaxVerticalFrames => 6;

        public override void Update()
        {
            int timeToDisappear = Lifetime - 60;

            // Fade in and out.
            if (Time < timeToDisappear)
                Opacity = Clamp(Opacity + 0.1f, 0f, 1f);
            if (Time >= timeToDisappear && Time <= Lifetime)
                Opacity = Clamp(Opacity - 0.1f, 0f, 1f);

            Rotation += RotationSpeed * RotationDirection;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D asteroid = ModContent.Request<Texture2D>("Cascade/Content/NPCs/CosmostoneShowers/Asteroids/SilicateAsteroidSmall").Value;

            Rectangle frameRectangle = asteroid.Frame(1, MaxVerticalFrames, 0, Frame % MaxVerticalFrames);
            Vector2 mainOrigin = frameRectangle.Size() / 2f;
            Color color = Color.Lerp(Color.White, Color.Black, 0.15f + Depth / 10f) * Opacity;

            spriteBatch.Draw(asteroid, GetDrawPositionBasedOnDepth(), frameRectangle, color, Rotation, mainOrigin, Scale / Depth, 0, 0f);
        }
    }
}
