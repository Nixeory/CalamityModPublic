﻿
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureMonolith
{
    public class MonolithPlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults()
        {
            Item.width = 8;
            Item.height = 10;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FurnitureMonolith.MonolithPlatform>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(2).AddIngredient(ModContent.ItemType<AstralMonolith>()).Register();
        }
    }
}
