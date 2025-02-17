﻿using CalamityMod.Events;
using Cascade.Content.Events;
using Cascade.Content.Events.CosmostoneShowers;

namespace Cascade.Content.Items
{
    public class CosmostoneShowersDebugItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public override void SetDefaults()
        {
            Item.width = 108;
            Item.height = 108;
            Item.noMelee = true;
            Item.useAnimation = 1;
            Item.useTime = 1;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override bool? UseItem(Player player)
        {
            // Tell the player off whenever they try to use the debug item during the wrong circumstances.
            if (Main.bloodMoon || Main.snowMoon || Main.pumpkinMoon)
            {
                Main.NewText("WARNING! Cosmostone Showers cannot occur during Blood, Pumpkin or Frost Moons!");
                return false;
            }

            if (BossRushEvent.BossRushActive)
            {
                Main.NewText("WARNING! Cosmostone Showers cannot occur during Boss Rush!");
                return false;
            }

            if (Main.dayTime)
            {
                Main.NewText("WARNING! Cosmostone Showers can only occur at night!");
                return false;
            }

            if (EventHandlerManager.SpecificEventIsActive<CosmostoneShowerEvent>())
            {
                Main.NewText("WARNING! A Cosmostone Showers event is already active in your world!");
                return false;
            }

            // Start the event if there are no issues.
            Main.NewText("Successfully started event!");
            EventHandlerManager.StartEvent<CosmostoneShowerEvent>();
            return true;
        }
    }
}
