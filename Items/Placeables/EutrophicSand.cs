﻿using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Projectiles.Typeless;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Items.Placeables
{
    public class EutrophicSand : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            // DisplayName.SetDefault("Eutrophic Sand");
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Navystone>();
        }

        public override void SetDefaults()
        {
            Item.createTile = ModContent.TileType<Tiles.SunkenSea.EutrophicSand>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.width = 13;
            Item.height = 10;
            Item.maxStack = 9999;
            Item.ammo = AmmoID.Sand;
            Item.shoot = ModContent.ProjectileType<EutrophicSandBallGun>();
            Item.notAmmo = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<EutrophicSandWallSafe>(4).
                AddTile(TileID.WorkBenches).
                Register();

            CreateRecipe().
                AddIngredient<EutrophicSandWall>(4).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
