﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.PlaguebringerGoliath
{
	public class Abomination : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Abombination");
			Tooltip.SetDefault("Calls in the airborne jungle abomination\n" +
                "Summons the Plaguebringer Goliath");
		}
		
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 18;
			item.maxStack = 20;
			item.rare = 8;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.consumable = true;
		}
		
		public override bool CanUseItem(Player player)
		{
			return player.ZoneJungle && !NPC.AnyNPCs(mod.NPCType("PlaguebringerGoliath"));
		}
		
		public override bool UseItem(Player player)
		{
			NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("PlaguebringerGoliath"));
			Main.PlaySound(SoundID.Roar, player.position, 0);
			return true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "PlagueCellCluster", 10);
			recipe.AddIngredient(ItemID.IronBar, 5);
            recipe.anyIronBar = true;
            recipe.AddIngredient(ItemID.Stinger, 2);
			recipe.AddIngredient(ItemID.Obsidian, 3);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}