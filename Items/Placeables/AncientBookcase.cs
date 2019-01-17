using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
	public class AncientBookcase : ModItem
	{
		public override void SetStaticDefaults()
        {
        }

		public override void SetDefaults()
        {
            item.width = 28;
			item.height = 20;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
            item.rare = 3;
            item.consumable = true;
			item.createTile = mod.TileType("AncientBookcase");
		}

		public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "BrimstoneSlag", 20);
            recipe.AddIngredient(ItemID.Book, 10);
            recipe.SetResult(this, 1);
            recipe.AddTile(null, "AncientAltar");
            recipe.AddRecipe();
        }
    }
}