﻿using Cascade.Content.Items.Dedicated.MPG;

namespace Cascade.Content.Buffs.Minions
{
    public class UnderworldLanterns : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "Buffs";

        public override string Texture => "Terraria/Images/Buff";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Cascade_Buffs().MoonSpiritLantern = true;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<UnderworldLantern>()] < 1)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
