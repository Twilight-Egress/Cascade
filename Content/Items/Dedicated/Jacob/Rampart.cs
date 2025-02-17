﻿namespace Cascade.Content.Items.Dedicated.Jacob
{
    public class Rampart : ModProjectile, ILocalizedModType
    {
        private ref float Timer => ref Projectile.ai[0];

        private ref float RotationDirection => ref Projectile.ai[1];

        private const int MaxChargeTime = 120;

        private const int AnvilAndSummoningCricleOpacityIndex = 0;

        private const int CosmicAnvilBackglowSpinIndex = 1;

        private const int RampartBackglowOpacityIndex = 2;

        private const int RampartBackglowRadiusIndex = 3;

        private const int RampartBackglowSpinIndex = 4;

        public new string LocalizationCategory => "Projectiles.Magic";

        public override string Texture => "CalamityMod/Items/Accessories/RampartofDeities";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 13;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.timeLeft = 600;
            Projectile.scale = 1.5f;

        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(TwoPi);
            RotationDirection = Main.rand.NextBool().ToDirectionInt();
        }

        public override void AI()
        {
            ref float anvilAndSummoningCricleOpacity = ref Projectile.Cascade().ExtraAI[AnvilAndSummoningCricleOpacityIndex];
            ref float rampartBackglowOpacity = ref Projectile.Cascade().ExtraAI[RampartBackglowOpacityIndex];
            ref float rampartBackglowRadius = ref Projectile.Cascade().ExtraAI[RampartBackglowRadiusIndex];

            if (Timer <= MaxChargeTime)
            {
                Projectile.velocity *= 0.9f;
                Projectile.Opacity = Lerp(0f, 1f, CascadeUtilities.SineEaseInOut(Timer / MaxChargeTime));
                Projectile.rotation += TwoPi / 120f * RotationDirection;

                // Backglow visuals.
                if (Timer <= 100)
                {
                    rampartBackglowOpacity = Lerp(rampartBackglowOpacity, 1f, CascadeUtilities.SineEaseInOut(Timer / 100f));
                    rampartBackglowRadius = Lerp(175f, 3f, CascadeUtilities.SineEaseInOut(Timer / 100f));
                }

                // Handle the anvil and summoning circle drawing.
                if (Timer <= 30f)
                    anvilAndSummoningCricleOpacity = Lerp(anvilAndSummoningCricleOpacity, 1f, CascadeUtilities.SineEaseInOut(Timer / 30f));
                if (Timer >= MaxChargeTime - 25 && Timer <= MaxChargeTime)
                    anvilAndSummoningCricleOpacity = Lerp(anvilAndSummoningCricleOpacity, 0f, CascadeUtilities.SineEaseInOut(Timer / 25f));

                // Dust visuals.
                if (Timer <= MaxChargeTime - 45)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 magicDustSpawnOffset = new Vector2(Main.rand.NextFloat(-100f, 100f), Main.rand.NextFloat(125f, 200f));
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + magicDustSpawnOffset, 267);
                        dust.color = Color.Lerp(Color.CornflowerBlue, Color.Fuchsia, Main.rand.NextFloat());
                        dust.noGravity = true;
                        dust.scale = 1f * Main.rand.NextFloat(1.1f, 1.25f);
                        dust.velocity = -Vector2.UnitY * Main.rand.NextFloat(3f, 8f);
                    }
                }

                if (Timer == MaxChargeTime)
                {
                    int dustType = Utils.SelectRandom(Main.rand, DustID.BlueTorch, DustID.Enchanted_Gold);
                    CascadeUtilities.CreateDustCircle(36, Projectile.Center, dustType, 10f, dustScale: 3f);
                    SoundEngine.PlaySound(CascadeSoundRegistry.AnvilHit, Projectile.Center);
                }
            }

            if (Timer >= MaxChargeTime)
            {
                Projectile.GetNearestTarget(1500f, 500f, out bool foundTarget, out NPC target);
                if (!foundTarget)
                {
                    Projectile.Kill();
                    return;
                }

                // Zoom over to the target upon charge completion.
                Projectile.SimpleMove(target.Center, 100f, 55f);
                Projectile.rotation += Projectile.velocity.X * 0.03f;
            }

            Timer++;
            Projectile.AdjustProjectileHitboxByScale(62f, 64f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(CommonCalamitySounds.OtherwordlyHitSound, Projectile.Center);
            Color pulseRingColor = Color.Lerp(Color.Goldenrod, Color.CornflowerBlue, Main.rand.NextFloat());
            PulseRingParticle deathRing = new(Projectile.Center, Vector2.Zero, pulseRingColor, 0.01f, 6f, 75);
            deathRing.SpawnCasParticle();

            // Deploy the bombs.
            int bombCount = Main.rand.Next(6, 14);
            for (int i = 0; i < bombCount; i++)
            {
                Vector2 bombVelocity = Vector2.UnitX.RotatedByRandom(TwoPi) * Main.rand.NextFloat(4f, 25f);
                Projectile.BetterNewProjectile(Projectile.Center, bombVelocity, ModContent.ProjectileType<DetonatingDraedonHeart>(), (int)(Projectile.damage * 0.45f), Projectile.knockBack);
            }

            // Some particles to mimic an explosion like effect.
            for (int i = 0; i < 35; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedByRandom(TwoPi) * Main.rand.NextFloat(15f, 25f);
                Color color = Color.Lerp(Color.Goldenrod, Color.CornflowerBlue, Main.rand.NextFloat());
                float scale = Main.rand.NextFloat(1.25f, 3f);
                HeavySmokeParticle deathSmoke = new(Projectile.Center, velocity, color, Main.rand.Next(75, 140), scale, Main.rand.NextFloat(0.35f, 1f), 0.06f, true, 0);
                deathSmoke.SpawnCasParticle();
            }

            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) * 25f;
                Color normalColor = Color.Lerp(Color.Goldenrod, Color.CornflowerBlue, Main.rand.NextFloat());
                Color bloomColor = Color.Lerp(Color.PaleGoldenrod, Color.LightSkyBlue, Main.rand.NextFloat());
                float scale = Main.rand.NextFloat(0.45f, 4f);
                int lifespan = Main.rand.Next(15, 45);
                SparkleParticle deathSparkles = new(Projectile.Center, velocity, normalColor, bloomColor, scale, lifespan, 0.25f, bloomScale: scale);
                deathSparkles.SpawnCasParticle();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ref float anvilAndSummoningCricleOpacity = ref Projectile.Cascade().ExtraAI[AnvilAndSummoningCricleOpacityIndex];
            if (anvilAndSummoningCricleOpacity > 0f)
                DrawAnvilAndSummoningCircle();

            DrawRampart();

            return false;
        }

        public void DrawRampart()
        {
            ref float rampartBackglowOpacity = ref Projectile.Cascade().ExtraAI[RampartBackglowOpacityIndex];
            ref float rampartBackglowRadius = ref Projectile.Cascade().ExtraAI[RampartBackglowRadiusIndex];
            ref float rampartBackglowSpin = ref Projectile.Cascade().ExtraAI[RampartBackglowSpinIndex];

            Texture2D rampartGlow = ModContent.Request<Texture2D>("Cascade/Content/Items/Dedicated/Jacob/RampartGlow").Value;

            Main.spriteBatch.UseBlendState(BlendState.Additive);
            for (int i = 0; i < 8; i++)
            {
                rampartBackglowSpin += TwoPi / 600f;
                Vector2 rampartBackglowDrawPosition = Projectile.Center + Vector2.UnitY.RotatedBy(rampartBackglowSpin + TwoPi * i / 8f) * rampartBackglowRadius + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                Color color = Color.Goldenrod;
                if (i % 2 == 0)
                    color = Color.Cyan;
                Main.EntitySpriteDraw(rampartGlow, rampartBackglowDrawPosition, null, color * rampartBackglowOpacity, Projectile.rotation, rampartGlow.Size() / 2f, Projectile.scale * 1.075f, SpriteEffects.None, 0);
            }
            Utilities.DrawAfterimagesCentered(Projectile, 0, Projectile.GetAlpha(Color.Cyan));
            Main.spriteBatch.ResetToDefault();

            Projectile.DrawTextureOnProjectile(Projectile.GetAlpha(Color.White), Projectile.rotation, Projectile.scale);
        }

        public void DrawAnvilAndSummoningCircle()
        {
            ref float anvilAndSummoningCricleOpacity = ref Projectile.Cascade().ExtraAI[AnvilAndSummoningCricleOpacityIndex];
            ref float cosmicAnvilBackglowSpin = ref Projectile.Cascade().ExtraAI[CosmicAnvilBackglowSpinIndex];

            Texture2D cosmicAnvil = ModContent.Request<Texture2D>("Cascade/Content/Items/Dedicated/Jacob/CosmicAnvil").Value;
            Texture2D summoningCircle = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/RancorMagicCircle").Value;
            Texture2D blurredSummoningCircle = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/RancorMagicCircleGlowmask").Value;

            // Summoning Circle.
            Vector2 summoningCircleDrawPosition = Projectile.Center + Vector2.UnitY * 125f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;

            ApplyShader(blurredSummoningCircle, anvilAndSummoningCricleOpacity, -Projectile.rotation, -Vector2.UnitY.ToRotation(), Color.Fuchsia * 0.8f, Color.DarkCyan * 0.8f, BlendState.Additive);
            Main.EntitySpriteDraw(blurredSummoningCircle, summoningCircleDrawPosition, null, Color.White, 0f, blurredSummoningCircle.Size() / 2f, Projectile.scale * 1.45f, SpriteEffects.None, 0);
            ApplyShader(summoningCircle, anvilAndSummoningCricleOpacity, Projectile.rotation, -Vector2.UnitY.ToRotation(), Color.CornflowerBlue, Color.Purple, BlendState.AlphaBlend);
            Main.EntitySpriteDraw(summoningCircle, summoningCircleDrawPosition, null, Color.White, 0f, summoningCircle.Size() / 2f, Projectile.scale * 1.25f, SpriteEffects.None, 0);
            Main.spriteBatch.ResetToDefault();

            // Cosmic Anvil.
            Vector2 cosmicAnvilDrawPosition = summoningCircleDrawPosition + new Vector2(10f, -20f);
            Main.spriteBatch.UseBlendState(BlendState.Additive);
            for (int i = 0; i < 4; i++)
            {
                cosmicAnvilBackglowSpin += TwoPi / 300f;
                Vector2 cosmicAnvilOrbitingBackglowDrawPosition = cosmicAnvilDrawPosition + Vector2.UnitY.RotatedBy(cosmicAnvilBackglowSpin + TwoPi * i / 4f) * 10f;
                Color cosmicAnvilBackglowColor = Utilities.ColorSwap(Color.Magenta, Color.Fuchsia, 1f) * anvilAndSummoningCricleOpacity;

                Main.EntitySpriteDraw(cosmicAnvil, cosmicAnvilOrbitingBackglowDrawPosition, null, cosmicAnvilBackglowColor, 0f, cosmicAnvil.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            }
            Main.spriteBatch.ResetToDefault();

            Main.EntitySpriteDraw(cosmicAnvil, cosmicAnvilDrawPosition, null, Color.White * anvilAndSummoningCricleOpacity, 0f, cosmicAnvil.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        }

        public void ApplyShader(Texture2D texture, float opacity, float circularRotation, float directionRotation, Color startingColor, Color endingColor, BlendState blendMode)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            CalamityUtils.CalculatePerspectiveMatricies(out var viewMatrix, out var projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(startingColor);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSecondaryColor(endingColor);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSaturation(directionRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uDirection"].SetValue(Projectile.direction);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uImageSize0"].SetValue(texture.Size());
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["overallImageSize"].SetValue(texture.Size());
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
        }
    }
}
