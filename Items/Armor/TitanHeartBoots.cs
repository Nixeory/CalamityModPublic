using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class TitanHeartBoots : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Titan Heart Boots");
			Tooltip.SetDefault("10% increased rogue velocity and 5% boosted rogue knockback");
		}

		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 18;
			item.value = Item.buyPrice(0, 12, 0, 0);
			item.rare = 5;
			item.defense = 14;
		}

		public override void UpdateEquip(Player player)
		{
			player.Calamity().titanHeartBoots = true;
			player.Calamity().throwingVelocity += 0.1f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Stardust>(), 14);
			recipe.AddIngredient(ModContent.ItemType<TitanHeart>());
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
