﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles;

namespace CalamityMod.NPCs.ProfanedGuardianBoss
{
    [AutoloadBossHead]
    public class ProfanedGuardianBoss : ModNPC
	{
		public int flareTimer = 0; //projectile stuff
		public int flareProjectiles = 1;
		public int dustTimer = 3;
		
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Profaned Guardian");
			Main.npcFrameCount[npc.type] = 6;
		}
		
		public override void SetDefaults()
		{
			npc.npcSlots = 20f;
			npc.aiStyle = -1;
			npc.damage = 100;
			npc.width = 100; //324
			npc.height = 80; //216
			npc.defense = 50;
			npc.lifeMax = 102500;
            if (CalamityWorld.bossRushActive)
            {
                npc.lifeMax = CalamityWorld.death ? 1200000 : 1000000;
            }
            npc.knockBackResist = 0f;
			npc.noGravity = true;
			npc.noTileCollide = true;
			aiType = -1;
			npc.boss = true;
            Mod calamityModMusic = ModLoader.GetMod("CalamityModMusic");
            if (calamityModMusic != null)
                music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/Guardians");
            else
                music = MusicID.Boss1;
            for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
				npc.buffImmune[BuffID.Ichor] = false;
                npc.buffImmune[BuffID.CursedInferno] = false;
                npc.buffImmune[mod.BuffType("AbyssalFlames")] = false;
                npc.buffImmune[mod.BuffType("ArmorCrunch")] = false;
                npc.buffImmune[mod.BuffType("DemonFlames")] = false;
                npc.buffImmune[mod.BuffType("GodSlayerInferno")] = false;
                npc.buffImmune[mod.BuffType("Nightwither")] = false;
                npc.buffImmune[mod.BuffType("Shred")] = false;
                npc.buffImmune[mod.BuffType("WhisperingDeath")] = false;
                npc.buffImmune[mod.BuffType("SilvaStun")] = false;
            }
			npc.value = Item.buyPrice(0, 25, 0, 0);
			npc.HitSound = SoundID.NPCHit52;
			npc.DeathSound = SoundID.NPCDeath55;
		}
		
		public override void FindFrame(int frameHeight)
        {
            npc.frameCounter += 0.15f;
            npc.frameCounter %= Main.npcFrameCount[npc.type];
            int frame = (int)npc.frameCounter;
            npc.frame.Y = frame * frameHeight;
        }
		
		public override void AI()
		{
			CalamityGlobalNPC.doughnutBoss = npc.whoAmI;
			bool fireBalls = (double)npc.life <= (double)npc.lifeMax * 0.75;
			bool powerBoost = (double)npc.life <= (double)npc.lifeMax * 0.5;
			bool fireDust = (double)npc.life <= (double)npc.lifeMax * 0.25;
			Player player = Main.player[npc.target];
			Vector2 vector = npc.Center;
			bool expertMode = Main.expertMode;
			bool revenge = CalamityWorld.revenge;
			bool isHoly = player.ZoneHoly;
			bool isHell = player.ZoneUnderworldHeight;
			npc.defense = (isHoly || isHell || CalamityWorld.bossRushActive) ? 50 : 99999;
			Vector2 vectorCenter = npc.Center;
			npc.TargetClosest(false);
			if (!Main.dayTime || !player.active || player.dead)
			{
				npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!Main.dayTime || !player.active || player.dead)
                {
                    npc.velocity = new Vector2(0f, -10f);
                    if (npc.timeLeft > 150)
                    {
                        npc.timeLeft = 150;
                    }
                    return;
                }
			}
			if (npc.timeLeft < 1800)
			{
				npc.timeLeft = 1800;
			}
			bool flag100 = false;
			for (int num569 = 0; num569 < 200; num569++)
			{
				if ((Main.npc[num569].active && Main.npc[num569].type == (mod.NPCType("ProfanedGuardianBoss2"))) || (Main.npc[num569].active && Main.npc[num569].type == mod.NPCType("ProfanedGuardianBoss3")))
				{
					flag100 = true;
				}
			}
			if (flag100)
			{
				npc.dontTakeDamage = true;
			}
			else
			{
				npc.dontTakeDamage = false;
			}
			if (Math.Sign(npc.velocity.X) != 0) 
			{
				npc.spriteDirection = -Math.Sign(npc.velocity.X);
			}
			npc.spriteDirection = Math.Sign(npc.velocity.X);
			float num998 = 8f;
			float scaleFactor3 = 300f;
			float num999 = 800f;
			float num1000 = powerBoost ? 14f : 16f;
			if (revenge)
			{
				num1000 *= 1.15f;
			}
			float num1001 = 5f;
			float scaleFactor4 = 0.8f;
			int num1002 = 0;
			float scaleFactor5 = 10f;
			float num1003 = 30f;
			float num1004 = 150f;
			float num1005 = powerBoost ? 14f : 16f;
			if (revenge)
			{
				num1005 *= 1.15f;
			}
			float num1006 = 0.333333343f;
			float num1007 = 8f;
			num1006 *= num1005;
			int num1009 = (npc.ai[0] == 2f) ? 2 : 1;
			int num1010 = (npc.ai[0] == 2f) ? 80 : 60;
			for (int num1011 = 0; num1011 < 2; num1011++) 
			{
				if (Main.rand.Next(3) < num1009) 
				{
					int num1012 = Dust.NewDust(npc.Center - new Vector2((float)num1010), num1010 * 2, num1010 * 2, 244, npc.velocity.X * 0.5f, npc.velocity.Y * 0.5f, 90, default(Color), 1.5f);
					Main.dust[num1012].noGravity = true;
					Main.dust[num1012].velocity *= 0.2f;
					Main.dust[num1012].fadeIn = 1f;
				}
			}
			if (Main.netMode != 1)
			{
				npc.localAI[0] += expertMode ? 2f : 1f;
				if (npc.localAI[0] >= 300f)
				{
					npc.localAI[0] = 0f;
					npc.TargetClosest(true);
					if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
					{
						Main.PlaySound(SoundID.Item20, npc.position);
						Vector2 value9 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
						float spread = 45f * 0.0174f;
				    	double startAngle = Math.Atan2(npc.velocity.X, npc.velocity.Y) - spread / 2;
				    	double deltaAngle = spread / 8f;
				    	double offsetAngle;
				    	int damage = expertMode ? 33 : 48;
				    	int projectileShot = mod.ProjectileType("ProfanedSpear");
				    	int i;
				    	for (i = 0; i < 8; i++)
				    	{
				   			offsetAngle = (startAngle + deltaAngle * ( i + i * i ) / 2f ) + 32f * i;
				        	Projectile.NewProjectile(value9.X, value9.Y, (float)( Math.Sin(offsetAngle) * 5f ), (float)( Math.Cos(offsetAngle) * 5f ), projectileShot, damage, 0f, Main.myPlayer, 0f, 0f);
				        	Projectile.NewProjectile(value9.X, value9.Y, (float)( -Math.Sin(offsetAngle) * 5f ), (float)( -Math.Cos(offsetAngle) * 5f ), projectileShot, damage, 0f, Main.myPlayer, 0f, 0f);
				    	}
					}
				}
			}
			if (fireBalls && flareTimer == 0)
			{
				flareTimer = 420;
			}
			if (flareTimer > 0)
			{
				flareTimer--;
				if (flareTimer == 0)
				{
					if (Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
					{
						if (Main.netMode != 1)
						{
							Main.PlaySound(SoundID.Item20, npc.position);
							Vector2 value9 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
							float spread = 90f * 0.0174f;
					    	double startAngle = Math.Atan2(npc.velocity.X, npc.velocity.Y) - spread / 2;
					    	double deltaAngle = spread / 8f;
					    	double offsetAngle;
					    	int damage = expertMode ? 34 : 50;
					    	int j;
					    	for (j = 0; j < flareProjectiles; j++)
					    	{
					    		int randomTime = Main.rand.Next(60, 120);
					   			offsetAngle = (startAngle + deltaAngle * ( j + j * j ) / 2f ) + 32f * j;
					   			int projectile = Projectile.NewProjectile(value9.X, value9.Y, 0f, 0f, mod.ProjectileType("GigaFlare"), damage, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
					        	int projectile2 = Projectile.NewProjectile(value9.X, value9.Y, 0f, 0f, mod.ProjectileType("GigaFlare"), damage, 0f, Main.myPlayer, 1f, (float)(npc.target + 1));
					        	Main.projectile[projectile].timeLeft = randomTime;
					        	Main.projectile[projectile2].timeLeft = randomTime;
					        	Main.projectile[projectile].velocity.X = (float)Main.rand.Next(-50, 51) * 0.125f;
					        	Main.projectile[projectile].velocity.Y = (float)Main.rand.Next(-50, 51) * 0.125f;
								Main.projectile[projectile2].velocity.X = (float)Main.rand.Next(-50, 51) * 0.125f;
								Main.projectile[projectile2].velocity.Y = (float)Main.rand.Next(-50, 51) * 0.125f;
					    	}
						}
					}
				}
			}
			if (npc.ai[0] == 0f) 
			{
				npc.knockBackResist = 0f;
				float scaleFactor6 = num998;
				Vector2 center4 = npc.Center;
				Vector2 center5 = Main.player[npc.target].Center;
				Vector2 vector126 = center5 - center4;
				Vector2 vector127 = vector126 - Vector2.UnitY * scaleFactor3;
				float num1013 = vector126.Length();
				vector126 = Vector2.Normalize(vector126) * scaleFactor6;
				vector127 = Vector2.Normalize(vector127) * scaleFactor6;
				bool flag64 = Collision.CanHit(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1);
				if (npc.ai[3] >= 120f) 
				{
					flag64 = true;
				}
				float num1014 = 8f;
				flag64 = (flag64 && vector126.ToRotation() > 3.14159274f / num1014 && vector126.ToRotation() < 3.14159274f - 3.14159274f / num1014);
				if (num1013 > num999 || !flag64) 
				{
					npc.velocity.X = (npc.velocity.X * (num1000 - 1f) + vector127.X) / num1000;
					npc.velocity.Y = (npc.velocity.Y * (num1000 - 1f) + vector127.Y) / num1000;
					if (!flag64) 
					{
						npc.ai[3] += 1f;
						if (npc.ai[3] == 120f) 
						{
							npc.netUpdate = true;
						}
					} 
					else
					{
						npc.ai[3] = 0f;
					}
				} 
				else 
				{
					npc.ai[0] = 1f;
					npc.ai[2] = vector126.X;
					npc.ai[3] = vector126.Y;
					npc.netUpdate = true;
				}
			} 
			else if (npc.ai[0] == 1f) 
			{
				npc.knockBackResist = 0f;
				npc.velocity *= scaleFactor4;
				npc.ai[1] += 1f;
				if (npc.ai[1] >= num1001) 
				{
					npc.ai[0] = 2f;
					npc.ai[1] = 0f;
					npc.netUpdate = true;
					Vector2 velocity = new Vector2(npc.ai[2], npc.ai[3]) + new Vector2((float)Main.rand.Next(-num1002, num1002 + 1), (float)Main.rand.Next(-num1002, num1002 + 1)) * 0.04f;
					velocity.Normalize();
					velocity *= scaleFactor5;
					npc.velocity = velocity;
				}
			} 
			else if (npc.ai[0] == 2f) 
			{
				if (Main.netMode != 1)
				{
					dustTimer--;
					if (fireDust && dustTimer <= 0)
					{
						Main.PlaySound(SoundID.Item20, npc.position);
						int damage = expertMode ? 38 : 56;
						Vector2 vector173 = Vector2.Normalize(player.Center - vectorCenter) * (float)(npc.width + 20) / 2f + vectorCenter;
						int projectile = Projectile.NewProjectile((int)vector173.X, (int)vector173.Y, (float)(npc.direction * 2), 4f, mod.ProjectileType("FlareDust"), damage, 0f, Main.myPlayer, 0f, 0f); //changed
						Main.projectile[projectile].timeLeft = 120;
						Main.projectile[projectile].velocity.X = 0f;
				        Main.projectile[projectile].velocity.Y = 0f;
			       		dustTimer = 3;
					}
				}
				npc.knockBackResist = 0f;
				float num1016 = num1003;
				npc.ai[1] += 1f;
				bool flag65 = Vector2.Distance(npc.Center, Main.player[npc.target].Center) > num1004 && npc.Center.Y > Main.player[npc.target].Center.Y;
				if ((npc.ai[1] >= num1016 && flag65) || npc.velocity.Length() < num1007) 
				{
					npc.ai[0] = 0f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.ai[3] = 0f;
					npc.velocity /= 2f;
					npc.netUpdate = true;
					npc.ai[1] = 45f;
					npc.ai[0] = 4f;
				} 
				else 
				{
					Vector2 center6 = npc.Center;
					Vector2 center7 = Main.player[npc.target].Center;
					Vector2 vec2 = center7 - center6;
					vec2.Normalize();
					if (vec2.HasNaNs()) 
					{
						vec2 = new Vector2((float)npc.direction, 0f);
					}
					npc.velocity = (npc.velocity * (num1005 - 1f) + vec2 * (npc.velocity.Length() + num1006)) / num1005;
				}
			} 
			else if (npc.ai[0] == 4f) 
			{
				npc.ai[1] -= 3f;
				if (npc.ai[1] <= 0f) 
				{
					npc.ai[0] = 0f;
					npc.ai[1] = 0f;
					npc.netUpdate = true;
				}
				npc.velocity *= 0.95f;
			}
		}
		
		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			cooldownSlot = 1;
			return true;
		}
		
		public override void BossLoot(ref string name, ref int potionType)
		{
			name = "A Profaned Guardian";
			potionType = ItemID.GreaterHealingPotion;
		}
		
		public override void NPCLoot()
		{
			Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("ProfanedCore"));
		}
		
		public override void OnHitPlayer(Player player, int damage, bool crit)
		{
			player.AddBuff(BuffID.OnFire, 600, true);
		}
		
		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax * 0.8f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.8f);
		}
		
		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 5; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, 244, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (npc.life <= 0)
			{
				for (int k = 0; k < 50; k++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 244, hitDirection, -1f, 0, default(Color), 1f);
				}
			}
		}
	}
}