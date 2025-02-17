﻿namespace Cascade.Content.Items.Dedicated.Raesh
{
    public class DroseraeDictionaryHoldout : ModProjectile, ILocalizedModType
    {
        private Player Owner => Main.player[Projectile.owner];

        private ref float Timer => ref Projectile.ai[0];

        private const int MaxChargeTime = 60;

        private const int RitualCircleOpacityIndex = 0;

        private const int RitualCircleRotationIndex = 1;

        private const int RitualCircleScaleIndex = 2;

        public new string LocalizationCategory => "Projectiles.Magic";

        public override string Texture => CascadeUtilities.EmptyPixelPath;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 114;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            ref float ritualCircleOpacity = ref Projectile.Cascade().ExtraAI[RitualCircleOpacityIndex];
            ref float ritualCircleRotation = ref Projectile.Cascade().ExtraAI[RitualCircleRotationIndex];
            ref float ritualCircleScale = ref Projectile.Cascade().ExtraAI[RitualCircleScaleIndex];

            bool manaIsAvailable = Owner.CheckMana(Owner.HeldItem.mana);
            bool weaponIsInUse = manaIsAvailable && Owner.PlayerIsChannelingWithItem(ModContent.ItemType<DroseraeDictionary>());
            bool shouldDespawn = Owner.ShouldDespawnHeldProj(ModContent.ItemType<DroseraeDictionary>()) || !weaponIsInUse;

            if (shouldDespawn)
            {
                Projectile.Kill();
                return;
            }

            DoBehavior_MainAttack(ref ritualCircleOpacity, ref ritualCircleScale);

            Timer++;
            Projectile.Center = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * 60f;
            Projectile.rotation = Owner.AngleTo(Main.MouseWorld);
            ritualCircleRotation += TwoPi / 150f;
            UpdatePlayerVariables();
        }

        public void DoBehavior_MainAttack(ref float ritualCircleOpacity, ref float ritualCircleScale)
        {
            // Scale up and fade in.
            if (Timer <= MaxChargeTime)
            {
                ritualCircleOpacity = Lerp(ritualCircleOpacity, 1f, Timer / MaxChargeTime);
                ritualCircleScale = Lerp(ritualCircleScale, 1f, Timer / MaxChargeTime);
                DrawInChargeParticles();
            }

            // Fire.
            if (Timer >= MaxChargeTime && Timer % 30 == 0)
            {
                Vector2 flytrapMawSpawnPos = Projectile.Center;
                Vector2 flyTrapMawVelocity = Projectile.SafeDirectionTo(Main.MouseWorld) * 35f;

                float damageScaleFactor = Lerp(1f, 5f, Utils.GetLerpValue(Owner.statLifeMax, 100f, Owner.statLife, true));
                int damage = Projectile.originalDamage.GetPercentageOfInteger(damageScaleFactor);
                Projectile.BetterNewProjectile(flytrapMawSpawnPos, flyTrapMawVelocity, ModContent.ProjectileType<FlytrapMaw>(), damage, Projectile.knockBack, CascadeSoundRegistry.FlytrapMawSpawn, null, Projectile.owner);

                Owner.ConsumeManaManually(Owner.HeldItem.mana);
                ParticleBurst();
                Timer = MaxChargeTime;
            }
        }

        public void DrawInChargeParticles()
        {
            Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width * 0.375f + 50f, Projectile.height * 0.485f + 50f);
            Vector2 velocity = Vector2.Normalize(Projectile.Center - spawnPosition) * Main.rand.NextFloat(5f, 9f);

            int lifespan = Main.rand.Next(30, 45);
            float scale = Main.rand.NextFloat(0.65f, 1f);

            SparkParticle magicSparks = new(spawnPosition, velocity, Color.Crimson, scale, lifespan);
            magicSparks.SpawnCasParticle();
        }

        public void ParticleBurst()
        {
            int sparkCount = Main.rand.Next(15, 25);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(Projectile.width * 0.375f, Projectile.height * 0.485f) * Main.rand.NextFloat(0.05f, 0.2f);

                int lifespan = Main.rand.Next(30, 45);
                float scale = Main.rand.NextFloat(0.65f, 1f);

                SparkParticle magicSparks = new(Projectile.Center, velocity, Color.Crimson, scale, lifespan);
                magicSparks.SpawnCasParticle();
            }
        }

        public void IdleDustEffects()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width * 0.375f, Projectile.height * 0.485f);
                Color dustColor = Color.Lerp(Color.Crimson, Color.DarkRed, Main.rand.NextFloat());
                float dustScale = Main.rand.NextFloat(0.65f, 1f);
                CascadeUtilities.CreateDustLoop(3, spawnPosition, Vector2.Zero, 264, dustScale: dustScale, dustColor: dustColor);
            }
        }

        public void UpdatePlayerVariables()
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.ChangeDir(Sign(Projectile.rotation.ToRotationVector2().X));
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - PiOver2);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawRitualCircle();
            Main.spriteBatch.ResetToDefault();

            return false;
        }

        public void DrawRitualCircle()
        {
            ref float ritualCircleOpacity = ref Projectile.Cascade().ExtraAI[RitualCircleOpacityIndex];
            ref float ritualCircleRotation = ref Projectile.Cascade().ExtraAI[RitualCircleRotationIndex];
            ref float ritualCircleScale = ref Projectile.Cascade().ExtraAI[RitualCircleScaleIndex];

            Texture2D ritualCircle = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/RancorMagicCircle").Value;
            Texture2D blurredRitualCircle = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/RancorMagicCircleGlowmask").Value;

            // Summoning Circle.
            Vector2 ritualCircleDrawPosition = Projectile.Center + Projectile.rotation.ToRotationVector2() - Main.screenPosition;

            ApplyShader(blurredRitualCircle, ritualCircleOpacity, -ritualCircleRotation, Projectile.AngleTo(Main.MouseWorld), Projectile.direction, Color.Crimson, Color.Red, BlendState.Additive);
            Main.EntitySpriteDraw(blurredRitualCircle, ritualCircleDrawPosition, null, Color.Red, 0f, blurredRitualCircle.Size() / 2f, ritualCircleScale * 1.275f, SpriteEffects.None, 0);

            ApplyShader(ritualCircle, ritualCircleOpacity, ritualCircleRotation, Projectile.AngleTo(Main.MouseWorld), Projectile.direction, Color.DarkRed, Color.Crimson, BlendState.AlphaBlend);
            Main.EntitySpriteDraw(ritualCircle, ritualCircleDrawPosition, null, Color.Red, 0f, ritualCircle.Size() / 2f, ritualCircleScale, SpriteEffects.None, 0);

            Main.spriteBatch.ResetToDefault();
        }

        public static void ApplyShader(Texture2D texture, float opacity, float circularRotation, float directionRotation, int direction, Color startingColor, Color endingColor, BlendState blendMode)
        {
            Main.spriteBatch.PrepareForShaders(blendMode);

            CalamityUtils.CalculatePerspectiveMatricies(out var viewMatrix, out var projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(startingColor);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSecondaryColor(endingColor);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSaturation(directionRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uDirection"].SetValue(direction);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uImageSize0"].SetValue(texture.Size());
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["overallImageSize"].SetValue(texture.Size());
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
        }
    }
}
