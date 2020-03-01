using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class StarfleetMK2 : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starmada");
            Tooltip.SetDefault("Fires a barrage of stars and plasma blasts");
        }

        public override void SetDefaults()
        {
            item.damage = 241;
            item.knockBack = 15f;
            item.shootSpeed = 16f;
            item.useStyle = 5;
            item.useAnimation = 27;
            item.useTime = 27;
            item.reuseDelay = 0;
            item.width = 122;
            item.height = 50;
            item.UseSound = SoundID.Item92;
            item.shoot = ModContent.ProjectileType<StarfleetMK2Gun>();
            item.value = Item.buyPrice(1, 80, 0, 0);
            item.rare = 10;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.ranged = true;
            item.channel = true;
            item.useTurn = false;
            item.useAmmo = AmmoID.FallenStar;
            item.autoReuse = true;
            item.Calamity().customRarity = CalamityRarity.DarkBlue;
        }

        public override bool CanUseItem(Player player)
        {
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == ModContent.ProjectileType<StarfleetMK2Gun>() && p.owner == player.whoAmI)
                {
                    return false;
                }
            }
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position.X, position.Y, speedX, speedY, ModContent.ProjectileType<StarfleetMK2Gun>(), 0, 0f, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Starfleet>());
            recipe.AddIngredient(ModContent.ItemType<StarSputter>());
            recipe.AddIngredient(ModContent.ItemType<CosmiliteBar>(), 10);
            recipe.AddIngredient(ModContent.ItemType<ExodiumClusterOre>(), 15);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
