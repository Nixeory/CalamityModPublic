using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Summon;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class CosmicViperEngineBuff : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Cosmic Viper");
            Description.SetDefault("The cosmic gunship will protect you");
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CosmicViperSummon>()] > 0)
            {
                modPlayer.cosmicViper = true;
            }
            if (!modPlayer.cosmicViper)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}
