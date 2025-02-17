namespace Cascade.Content.EntityOverrides.Items.HoneyComb
{
    public class StingerNecklaceOverride : ItemOverride
    {
        public override int TypeToOverride => ItemID.StingerNecklace;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
	    {
            player.Cascade_BeeFlightTimeBoost().BeeFlightBoost = 1; 
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            Utilities.EditTooltipByNum(1, item, tooltips, (t) => t.Text += "\nIncreases wing flight time by 7%");
        }
    }
}