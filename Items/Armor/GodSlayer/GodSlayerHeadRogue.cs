﻿using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.CalPlayer.Dashes;

namespace CalamityMod.Items.Armor.GodSlayer
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("GodSlayerMask")]
    public class GodSlayerHeadRogue : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("God Slayer Mask");
            Tooltip.SetDefault("14% increased rogue damage and critical strike chance, 5% increased movement speed");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.buyPrice(0, 75, 0, 0);
            Item.defense = 29; //96
            Item.Calamity().customRarity = CalamityRarity.DarkBlue;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<GodSlayerChestplate>() && legs.type == ModContent.ItemType<GodSlayerLeggings>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.godSlayer = true;
            modPlayer.godSlayerThrowing = true;
            modPlayer.rogueStealthMax += 1.2f;
            modPlayer.wearingRogueArmor = true;
            var hotkey = CalamityKeybinds.GodSlayerDashHotKey.TooltipHotkeyString();
            player.setBonus = "Allows you to dash for an immense distance in 8 directions\n" +
                "Press " + hotkey + " while holding down the movement keys in the direction you want to dash\n" +
                "Enemies you dash through take massive damage\n" +
                "During the dash you are immune to most debuffs\n" +
                "The dash has a 35 second cooldown\n" +
                "While at full HP all of your rogue stats are boosted by 10%\n" +
                "If you take over 80 damage in one hit you will be given extra immunity frames\n" +
                "Rogue stealth builds while not attacking and slower while moving, up to a max of 120\n" +
                "Once you have built max stealth, you will be able to perform a Stealth Strike\n" +
                "Rogue stealth only reduces when you attack, it does not reduce while moving\n" +
                "The higher your rogue stealth the higher your rogue damage, crit, and movement speed";

            if (modPlayer.godSlayerDashHotKeyPressed)
            {
                modPlayer.DashID = GodslayerArmorDash.ID;
                player.dash = 0;
            }
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<ThrowingDamageClass>() += 0.14f;
            player.GetCritChance<ThrowingDamageClass>() += 14;
            player.moveSpeed += 0.05f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CosmiliteBar>(14).
                AddIngredient<AscendantSpiritEssence>(2).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}