﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.AstrumDeus
{
	public class Starcore : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starcore");
			Tooltip.SetDefault("May the stars guide your way\n" +
                "Summons Astrum Deus");
		}
		
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.maxStack = 20;
			item.rare = 7;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.consumable = true;
		}
		
		public override bool CanUseItem(Player player)
		{
			return !Main.dayTime && !NPC.AnyNPCs(mod.NPCType("AstrumDeusHead")) && !NPC.AnyNPCs(mod.NPCType("AstrumDeusHeadSpectral"));
		}

        public override bool UseItem(Player player)
        {
            for (int x = 0; x < 10; x++)
            {
                NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("AstrumDeusHead"));
            }
            NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("AstrumDeusHeadSpectral"));
            Main.PlaySound(SoundID.Roar, player.position, 0);
			return true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "Stardust", 25);
            recipe.AddIngredient(null, "AstralJelly", 8);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}