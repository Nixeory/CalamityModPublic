﻿using Terraria;
using Terraria.ModLoader;
using CalamityMod.CalPlayer;

namespace CalamityMod.Buffs.Shrines
{
    public class ChiBuff : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Chi");
			Description.SetDefault("Your next attack is boosted and you are more resilient");
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = false;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			longerExpertDebuff = false;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<CalamityPlayer>(mod).trinketOfChiBuff = true;
		}
	}
}
