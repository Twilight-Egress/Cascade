﻿namespace Cascade.Core.BaseEntities.ModNPCs
{
    public abstract class BaseAsteroid : ModNPC
    {
        public ref float RotationSpeedSpawnFactor => ref NPC.Cascade().ExtraAI[0];

        public ref float MaxTime => ref NPC.Cascade().ExtraAI[1];

        public ref float Timer => ref NPC.ai[0];

        public sealed override void OnSpawn(IEntitySource source)
        {
            RotationSpeedSpawnFactor = Main.rand.NextFloat(75f, 480f) * Utils.SelectRandom(Main.rand, -1, 1);
            MaxTime = Main.rand.NextFloat(1200, 7200);
            SafeOnSpawn(source);
        }

        public override bool PreAI()
        {
            // Add to the global list of classes that inherit this base class.
            Cascade.BaseAsteroidInheriters.AddWithCondition(NPC, !Cascade.BaseAsteroidInheriters.Contains(NPC));
            SafePreAI();
            return true;
        }

        public sealed override void AI()
        {
            NPC.TargetClosest();

            // Fade in.
            if (Timer < MaxTime)
                NPC.Opacity = Clamp(NPC.Opacity + 0.02f, 0f, 1f);

            // Idly rotate.
            NPC.rotation += Pi / RotationSpeedSpawnFactor;
            NPC.rotation += NPC.velocity.X * 0.03f;

            // If an asteroid falls within a certain distance of Terraria's mesosphere, it
            // begins to be pulled in by the planet's gravity.

            // In simple terms, the Y-velocity is increased once the asteroid is pushed low enough.
            if (NPC.Bottom.Y >= Main.maxTilesY + 1000f)
            {
                // Increase damage as Y-velocity begins to increase.
                NPC.damage = 150 * (int)Utils.GetLerpValue(0f, 1f, NPC.velocity.Y / 12f, true);
                NPC.velocity.Y = Clamp(NPC.velocity.Y + 0.03f, 0f, 18f);

                // Die upon tile collision, explode.
                if (Collision.SolidCollision(NPC.Center, NPC.width, NPC.height))
                {
                    NPC.life = 0;
                    NPC.checkDead();
                    OnMeteorCrashKill();
                    NPC.netUpdate = true;
                    return;
                }
            }
            else
            {
                // Kill the asteroid's damage if it is simply floating around.
                NPC.damage = 0;
                // Always decrease velocity so that they don't drift off into No Man's Land.
                NPC.velocity *= 0.99f;
            }

            NPC.ShowNameOnHover = false;
            NPC.AdjustNPCHitboxToScale(36f, 36f);

            // Despawn after some time as to not clog the NPC limit.
            Timer++;
            if (Timer >= MaxTime)
            {
                NPC.Opacity = Clamp(NPC.Opacity - 0.02f, 0f, 1f);
                if (NPC.Opacity <= 0f)
                {
                    NPC.active = false;
                }
            }

            if (SafePreAI())
                SafeAI();
        }

        public sealed override bool? CanBeHitByItem(Player player, Item item)
        {
            if (item.pick <= 0)
                return false;
            return null;
        }

        public sealed override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (!Cascade.PickaxeProjectileIDs.Contains(projectile.type))
                return false;
            return null;
        }

        public sealed override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (item.pick > 0)
            {
                modifiers.Knockback *= 0f;
                modifiers.FinalDamage *= 2f * Lerp(1f, 0.2f, item.pick / 250f);
            }

            SafeModifyHitByItem(player, item, ref modifiers);
        }

        public sealed override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[projectile.owner];
            Item item = player.ActiveItem();
            if (item.pick > 0 && projectile.owner == player.whoAmI)
            {
                modifiers.Knockback *= 0f;
                modifiers.FinalDamage *= 2f * Lerp(1f, 0.2f, item.pick / 250f);
            }

            SafeModifyHitByProjectile(projectile, ref modifiers);
        }


        public virtual void OnMeteorCrashKill() { }

        public virtual void SafeOnSpawn(IEntitySource source) { }

        public virtual bool SafePreAI() => true;

        public virtual void SafeAI() { }

        public virtual void SafeModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) { }

        public virtual void SafeModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) { }
    }
}
