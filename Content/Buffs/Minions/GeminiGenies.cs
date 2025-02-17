﻿using Cascade.Content.Items.Accessories.Elementals.TwinGeminiGenies;

namespace Cascade.Content.Buffs.Minions
{
    public class GeminiGenies : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "Buffs";

        public override string Texture => "CalamityMod/Buffs/Summon/SandyHealingWaifu";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Cascade_Buffs().GeminiGenies = true;
            bool isDunaOrPsytriGone = player.ownedProjectileCounts[ModContent.ProjectileType<GeminiGenieSandy>()] < 0 || player.ownedProjectileCounts[ModContent.ProjectileType<GeminiGeniePsychic>()] < 0;
            if (isDunaOrPsytriGone)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
