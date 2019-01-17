using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;
//using TerrariaOverhaul;

namespace CalamityMod.Items.Weapons
{
    public class ArterialAssault : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Arterial Assault");
            Tooltip.SetDefault("Fires a chain of bloodfire arrows from the sky");
        }

        public override void SetDefaults()
        {
            item.damage = 232;
            item.ranged = true;
            item.width = 44;
            item.height = 100;
            item.useTime = 3;
            item.reuseDelay = 10;
            item.useAnimation = 15;
            item.useStyle = 5;
            item.noMelee = true;
            item.knockBack = 4.25f;
            item.value = Item.buyPrice(1, 40, 0, 0);
            item.rare = 10;
            item.UseSound = SoundID.Item102;
            item.autoReuse = true;
            item.shoot = mod.ProjectileType("BloodfireArrow");
            item.shootSpeed = 30f;
            item.useAmmo = 40;
        }

        /*public void OverhaulInit()
        {
            this.SetTag("bow");
        }*/

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            foreach (TooltipLine line2 in list)
            {
                if (line2.mod == "Terraria" && line2.Name == "ItemName")
                {
                    line2.overrideColor = new Color(0, 255, 0);
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(null, "BloodstoneCore", 5);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            float num72 = item.shootSpeed;
            Vector2 vector2 = player.RotatedRelativePoint(player.MountedCenter, true);
            float num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
            float num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
            if (player.gravDir == -1f)
            {
                num79 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector2.Y;
            }
            float num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
            float num81 = num80;
            if ((float.IsNaN(num78) && float.IsNaN(num79)) || (num78 == 0f && num79 == 0f))
            {
                num78 = (float)player.direction;
                num79 = 0f;
                num80 = num72;
            }
            else
            {
                num80 = num72 / num80;
            }
            num78 *= num80;
            num79 *= num80;
            vector2 = new Vector2(player.position.X + (float)player.width * 0.5f + (-(float)player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
            vector2.X = (vector2.X + player.Center.X) / 2f;
            vector2.Y -= 100f;
            num78 = (float)Main.mouseX + Main.screenPosition.X - vector2.X;
            num79 = (float)Main.mouseY + Main.screenPosition.Y - vector2.Y;
            if (num79 < 0f)
            {
                num79 *= -1f;
            }
            if (num79 < 20f)
            {
                num79 = 20f;
            }
            num80 = (float)Math.Sqrt((double)(num78 * num78 + num79 * num79));
            num80 = num72 / num80;
            num78 *= num80;
            num79 *= num80;
            float speedX4 = num78;
            float speedY5 = num79;
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                int bloodfire = Projectile.NewProjectile(vector2.X, vector2.Y, speedX4, speedY5, mod.ProjectileType("BloodfireArrow"), damage, knockBack, player.whoAmI, 0f, (float)Main.rand.Next(15));
                Main.projectile[bloodfire].tileCollide = false;
            }
            else
            {
                int num121 = Projectile.NewProjectile(vector2.X, vector2.Y, speedX4, speedY5, type, damage, knockBack, player.whoAmI, 0f, 0f);
                Main.projectile[num121].noDropItem = true;
            }
            return false;
        }
    }
}