﻿using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class OmegaBlueLeggings : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            DisplayName.SetDefault("Omega Blue Tentacles");
            Tooltip.SetDefault(@"12% increased movement speed
12% increased damage and critical strike chance");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 35, 25, 0);
            Item.rare = ItemRarityID.Red;
            Item.defense = 22;
            Item.Calamity().customRarity = CalamityRarity.PureGreen;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += 0.12f;
            player.GetCritChance<GenericDamageClass>() += 12;
            player.moveSpeed += 0.12f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ReaperTooth>(10).
                AddIngredient<Lumenite>(6).
                AddIngredient<Tenebris>(6).
                AddIngredient<RuinousSoul>(3).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
