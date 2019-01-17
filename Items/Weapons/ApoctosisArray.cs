using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons 
{
	public class ApoctosisArray : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Apoctosis Array");
			Tooltip.SetDefault("Fires ion blasts that speed up and then explode\nThe higher your mana the more damage they will do");
		}

		public override void SetDefaults()
		{
			item.width = 98;
			item.damage = 58;
			item.magic = true;
			item.mana = 12;
			item.useAnimation = 7;
			item.useTime = 7;
			item.useStyle = 5;
			item.knockBack = 6.75f;
			item.UseSound = SoundID.Item91;
			item.autoReuse = true;
			item.noMelee = true;
			item.height = 34;
            item.value = Item.buyPrice(1, 20, 0, 0);
            item.rare = 10;
            item.shoot = mod.ProjectileType("IonBlast");
			item.shootSpeed = 8f;
		}
		
		public override void ModifyTooltips(List<TooltipLine> list)
	    {
	        foreach (TooltipLine line2 in list)
	        {
	            if (line2.mod == "Terraria" && line2.Name == "ItemName")
	            {
	                line2.overrideColor = new Color(0, 255, 200);
	            }
	        }
	    }
		
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-25, 0);
		}
		
		public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
	        float manaAmount = ((float)player.statMana * 0.01f);
	        float damageMult = manaAmount;
	        int projectile = Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, (int)((double)damage * damageMult), knockBack, player.whoAmI, 0.0f, 0.0f);
	        Main.projectile[projectile].scale = (manaAmount * 0.375f);
	    	return false;
		}
		
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "IonBlaster");
			recipe.AddIngredient(ItemID.LunarBar, 10);
	        recipe.AddTile(TileID.LunarCraftingStation);
	        recipe.SetResult(this);
	        recipe.AddRecipe();
		}
	}
}
