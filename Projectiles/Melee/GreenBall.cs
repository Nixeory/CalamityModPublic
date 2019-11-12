﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GreenBall : ModProjectile
    {
        private int projTime = 15;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Green Bag");
        }

        public override void SetDefaults()
        {
            projectile.width = 20;
            projectile.height = 20;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = 2;
			projectile.alpha = 255;
            projectile.aiStyle = 14;
            projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Lighting.AddLight(projectile.Center, (255 - projectile.alpha) * 0.05f / 255f, (255 - projectile.alpha) * 0.45f / 255f, (255 - projectile.alpha) * 0.05f / 255f);
			if (projectile.alpha > 0)
			{
				projectile.alpha -= 25;
				if (projectile.alpha > 0)
					projectile.alpha = 0;
			}
			projectile.rotation = projectile.velocity.X * 0.04f;
            projTime--;
            if (projTime == 0)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, projectile.velocity.X * 0.35f, projectile.velocity.Y * 0.35f, ModContent.ProjectileType<GreenDust>(), (int)((double)projectile.damage * 0.75), projectile.knockBack, projectile.owner, 0f, 0f);
                }
                projTime = 15;
            }
        }

		public override Color? GetAlpha(Color lightColor)
		{
			if (projectile.timeLeft < 85)
			{
				byte b2 = (byte)(projectile.timeLeft * 3);
				byte a2 = (byte)(100f * ((float)b2 / 255f));
				return new Color((int)b2, (int)b2, (int)b2, (int)a2);
			}
			return new Color(255, 255, 255, 100);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.penetrate--;
            if (projectile.penetrate <= 0)
            {
                projectile.Kill();
            }
            else
            {
                projectile.ai[0] += 0.1f;
                if (projectile.velocity.X != oldVelocity.X)
                {
                    projectile.velocity.X = -oldVelocity.X;
                }
                if (projectile.velocity.Y != oldVelocity.Y)
                {
                    projectile.velocity.Y = -oldVelocity.Y;
                }
                projectile.velocity *= 0.98f;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.CursedInferno, 300);
        }
    }
}
