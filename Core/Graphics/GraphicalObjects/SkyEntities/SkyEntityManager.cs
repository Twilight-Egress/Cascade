﻿using ReLogic.Threading;

namespace Cascade.Core.Graphics.GraphicalObjects.SkyEntities
{
    public class SkyEntityManager : ModSystem
    {
        #region Fields
        internal static List<SkyEntity> ActiveSkyEntities = [];
        #endregion

        #region Overrides
        #region Loading and Unloading
        public override void OnModLoad()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            On_SkyManager.DrawDepthRange += DrawSkyEntities_BeforeCustomSkies;
            On_SkyManager.DrawDepthRange += DrawSkyEntities_AfterCustomSkies;
        }

        public override void OnModUnload()
        {
            ActiveSkyEntities = null;

            On_SkyManager.DrawDepthRange -= DrawSkyEntities_BeforeCustomSkies;
            On_SkyManager.DrawDepthRange -= DrawSkyEntities_AfterCustomSkies;
        }

        public override void OnWorldUnload() => ActiveSkyEntities.Clear();

        #endregion

        #region Updating
        public override void PostUpdateEverything()
        {
            FastParallel.For(0, ActiveSkyEntities.Count, (int x, int y, object context) =>
            {
                for (int i = x; i < y; i++)
                {
                    ActiveSkyEntities[i].Time++;
                    ActiveSkyEntities[i].Position += ActiveSkyEntities[i].Velocity;
                    ActiveSkyEntities[i].Update();
                }
            });

            ActiveSkyEntities.RemoveAll(skyEntity => skyEntity.Time >= skyEntity.Lifetime && skyEntity.DieWithLifespan);
        }
        #endregion
        #endregion

        #region Public Static Methods
        public static bool IsSpecificSkyEntityActive<T>() where T : SkyEntity => ActiveSkyEntities.ContainsType(typeof(T));

        public static int CountActiveSkyEntities<T>() where T : SkyEntity
        {
            int count = 0;
            foreach (SkyEntity sky in ActiveSkyEntities)
            {
                if (sky.GetType() == typeof(T))
                    count++;
            }

            return count;
        }

        public static int CountActiveSkyEntities(params Type[] types)
        {
            int count = 0;
            foreach (SkyEntity sky in ActiveSkyEntities)
            {
                if (types.ContainsType(sky.GetType()))
                    count++;
            }

            return count;
        }
        #endregion

        #region Private Methods
        private void DrawSkyEntities_BeforeCustomSkies(On_SkyManager.orig_DrawDepthRange orig, SkyManager self, SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            spriteBatch.End();
            DrawSkyEntities(SkyEntityDrawContext.BeforeCustomSkies, spriteBatch, minDepth, maxDepth);
            spriteBatch.ResetToDefault(false);

            orig.Invoke(self, spriteBatch, minDepth, maxDepth);
        }

        private void DrawSkyEntities_AfterCustomSkies(On_SkyManager.orig_DrawDepthRange orig, SkyManager self, SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            orig.Invoke(self, spriteBatch, minDepth, maxDepth);

            spriteBatch.End();
            DrawSkyEntities(SkyEntityDrawContext.AfterCustomSkies, spriteBatch, minDepth, maxDepth);
            spriteBatch.ResetToDefault(false);
        }

        private static void DrawSkyEntities(SkyEntityDrawContext drawContext, SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            // Get a list of sky entities based on their draw context.
            List<SkyEntity> drawCollection = ActiveSkyEntities.Where(s => s.DrawContext == drawContext).ToList();
            if (drawCollection.Count <= 0)
                return;

            // Prepare for screen culling.
            RasterizerState screenCullState = CascadeUtilities.PrepareScissorRectangleState();

            spriteBatch.Begin(SpriteSortMode.Deferred, drawCollection.First().BlendState, Main.DefaultSamplerState, DepthStencilState.None, screenCullState, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (SkyEntity skyEntity in drawCollection)
            {
                if (skyEntity.Depth > minDepth && skyEntity.Depth <= maxDepth)
                    skyEntity.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        private static void DrawSkyEntities(SkyEntityDrawContext drawContext, SpriteBatch spriteBatch)
            => DrawSkyEntities(drawContext, spriteBatch, float.MinValue, float.MaxValue);
        #endregion
    }
}
