using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Utilities;

namespace CalamityMod.Items.DesertScourge
{
	public class DesertScourgeBag : ModItem
	{
		public override int BossBagNPC => mod.NPCType("DesertScourgeHead");

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults()
		{
			item.maxStack = 999;
			item.consumable = true;
			item.width = 24;
			item.height = 24;
			item.rare = 9;
			item.expert = true;
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override void OpenBossBag(Player player)
		{
			// Materials
			DropHelper.DropItem(player, mod.ItemType("VictoryShard"), 10, 16);
			DropHelper.DropItem(player, ItemID.Coral, 7, 11);
			DropHelper.DropItem(player, ItemID.Seashell, 7, 11);
			DropHelper.DropItem(player, ItemID.Starfish, 7, 11);

			// Weapons
			DropHelper.DropItemChance(player, mod.ItemType("AquaticDischarge"), 3);
			DropHelper.DropItemChance(player, mod.ItemType("Barinade"), 3);
			DropHelper.DropItemChance(player, mod.ItemType("StormSpray"), 3);
			DropHelper.DropItemChance(player, mod.ItemType("SeaboundStaff"), 3);
			float f = Main.rand.NextFloat();
			bool replaceWithRare = f <= DropHelper.RareVariantDropRateFloat; // 1/40 chance overall of getting Dune Hopper
			if (f < 0.3333f) // 1/3 chance of getting Scourge of the Desert OR Dune Hopper replacing it
			{
				DropHelper.DropItemCondition(player, mod.ItemType("ScourgeoftheDesert"), !replaceWithRare);
				DropHelper.DropItemCondition(player, mod.ItemType("DuneHopper"), replaceWithRare);
			}

			// Equipment
			DropHelper.DropItem(player, mod.ItemType("OceanCrest"));
			DropHelper.DropItemChance(player, mod.ItemType("AeroStone"), 9);
			DropHelper.DropItemChance(player, mod.ItemType("DeepDiver"), DropHelper.RareVariantDropRateInt);

			// Vanity
			DropHelper.DropItemChance(player, mod.ItemType("DesertScourgeMask"), 7);

			// Fishing
			DropHelper.DropItemChance(player, ItemID.HighTestFishingLine, 12);
			DropHelper.DropItemChance(player, ItemID.AnglerTackleBag, 12);
			DropHelper.DropItemChance(player, ItemID.TackleBox, 12);
			DropHelper.DropItemChance(player, ItemID.AnglerEarring, 9);
			DropHelper.DropItemChance(player, ItemID.FishermansGuide, 9);
			DropHelper.DropItemChance(player, ItemID.WeatherRadio, 9);
			DropHelper.DropItemChance(player, ItemID.Sextant, 9);
			DropHelper.DropItemChance(player, ItemID.AnglerHat, 4);
			DropHelper.DropItemChance(player, ItemID.AnglerVest, 4);
			DropHelper.DropItemChance(player, ItemID.AnglerPants, 4);
			DropHelper.DropItemChance(player, ItemID.FishingPotion, 4, 2, 3);
			DropHelper.DropItemChance(player, ItemID.SonarPotion, 4, 2, 3);
			DropHelper.DropItemChance(player, ItemID.CratePotion, 4, 2, 3);
			DropHelper.DropItemCondition(player, ItemID.GoldenBugNet, NPC.downedBoss3, 18, 1, 1);
		}
	}
}
