using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;
//using TerrariaOverhaul;

namespace CalamityMod.Items.Weapons.Astral
{
	public class AstralRepeater : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Astral Bow");
		}

	    public override void SetDefaults()
	    {
	        item.damage = 65;
	        item.crit += 25;
	        item.ranged = true;
	        item.width = 38;
	        item.height = 78;
	        item.useTime = 4;
	        item.reuseDelay = 15;
	        item.useAnimation = 12;
	        item.useStyle = 5;
	        item.noMelee = true;
	        item.knockBack = 2.5f;
            item.value = Item.buyPrice(0, 60, 0, 0);
            item.rare = 7;
	        item.UseSound = SoundID.Item5;
	        item.autoReuse = true;
	        item.shoot = 10;
	        item.shootSpeed = 16f;
	        item.useAmmo = 40;
	    }

        /*public void OverhaulInit()
        {
            this.SetTag("crossbow");
        }*/

        public override void AddRecipes()
	    {
	        ModRecipe recipe = new ModRecipe(mod);
	        recipe.AddIngredient(null, "AstralBar", 7);
	        recipe.AddTile(TileID.MythrilAnvil);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
	    }
	}
}