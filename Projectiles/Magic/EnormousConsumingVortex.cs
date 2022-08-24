﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using CalamityMod.Particles;
using CalamityMod.Items.Weapons.Magic;

namespace CalamityMod.Projectiles.Magic
{
    public class EnormousConsumingVortex : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner];

        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public const float StartingScale = 0.0004f;

        public const float IdealScale = 2.7f;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Subsuming Vortex");

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.scale = StartingScale;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 7;
        }

        // Vanilla Terraria does not sync projectile scale by default.
        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.scale);

        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.scale = reader.ReadSingle();

        public override void AI()
        {
            // If the player is no longer able to channel the vortex, kill it.
            if (!Owner.channel || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            // Release energy from the book.
            if (Time >= SubsumingVortex.VortexShootDelay)
            {
                Vector2 bookPosition = Owner.Center + Vector2.UnitX * Owner.direction * 22f;
                if (Main.rand.NextBool())
                {
                    Vector2 energyVelocity = Main.rand.NextVector2Circular(3f, 3f);
                    Color energyColor = CalamityUtils.MulticolorLerp(Main.rand.NextFloat(), CalamityUtils.ExoPalette);
                    SquishyLightParticle exoEnergy = new(bookPosition, energyVelocity, 0.55f, energyColor, 40, 1f, 1.5f);
                    GeneralParticleHandler.SpawnParticle(exoEnergy);
                }

                // Fire vortices at nearby target.
                NPC potentialTarget = Projectile.Center.ClosestNPCAt(SubsumingVortex.SmallVortexTargetRange - 100f);
                if (potentialTarget != null && Time % SubsumingVortex.VortexReleaseRate == SubsumingVortex.VortexReleaseRate - 1)
                {
                    // CheckMana returns true if the mana cost can be paid..
                    bool allowContinuedUse = Owner.CheckMana(Owner.ActiveItem(), -1, true, false);
                    bool vortexStillInUse = Owner.channel && allowContinuedUse && !Owner.noItems && !Owner.CCed;
                    if (vortexStillInUse)
                    {
                        SoundEngine.PlaySound(SoundID.Item84, Projectile.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float hue = (Time - SubsumingVortex.VortexShootDelay) / 125f;
                            Vector2 vortexVelocity = Projectile.SafeDirectionTo(potentialTarget.Center) * 8f;
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vortexVelocity, ModContent.ProjectileType<ExoVortex>(), Projectile.damage, Projectile.knockBack, Projectile.owner, hue);
                        }
                        Projectile.netUpdate = true;
                    }
                    else if (!vortexStillInUse)
                        Projectile.Kill();
                }
            }

            // Create swirling exo energy.
            if (Main.rand.NextBool())
            {
                float dustSpawnChance = Utils.Remap(Time, 12f, SubsumingVortex.VortexShootDelay + 8f, 0.25f, 0.7f);
                for (int i = 0; i < 3; i++)
                {
                    if (Main.rand.NextFloat() < dustSpawnChance)
                    {
                        float spawnOffsetAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float hue = (float)Math.Sin(spawnOffsetAngle + Time / 26f) * 0.5f + 0.5f;
                        float spawnOffsetFactor = Main.rand.NextFloat(0.3f, 0.95f);
                        Vector2 energySpawnPosition = Projectile.Center + spawnOffsetAngle.ToRotationVector2() * Projectile.Size * spawnOffsetFactor;
                        Vector2 energyVelocity = (spawnOffsetAngle - MathHelper.PiOver2).ToRotationVector2() * (Main.rand.NextFloat(5f, 10f) * spawnOffsetFactor) * dustSpawnChance;
                        Color energyColor = CalamityUtils.MulticolorLerp(hue, CalamityUtils.ExoPalette);
                        SquishyLightParticle exoEnergy = new(energySpawnPosition, energyVelocity, Projectile.scale * Main.rand.NextFloat(0.18f, 0.3f), energyColor, 32, 1f, 1.5f);
                        GeneralParticleHandler.SpawnParticle(exoEnergy);
                    }
                }
            }

            // Hover above the target.
            if (Main.myPlayer == Projectile.owner)
            {
                // Smoothly approach a sinusoidal offset as time goes on.
                float verticalOffset = Utils.Remap(Time, 0f, 90f, -30f, (float)Math.Cos(Projectile.timeLeft / 32f) * 30f - 325f);
                Vector2 hoverDestination = Owner.Top + Vector2.UnitY * verticalOffset;
                hoverDestination += (Main.MouseWorld - hoverDestination) * SubsumingVortex.GiantVortexMouseDriftFactor;

                Vector2 idealVelocity = Vector2.Zero.MoveTowards(hoverDestination - Projectile.Center, 32f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealVelocity, 0.04f);
                Projectile.netSpam = 0;
                Projectile.netUpdate = true;
            }

            // Re-determine the hitbox size.
            Projectile.Opacity = Utils.GetLerpValue(0f, 20f, Time, true);
            Projectile.scale = Utils.Remap(Time, 0f, 45f, StartingScale, IdealScale);
            Projectile.ExpandHitboxBy((int)(Projectile.scale * 62f));

            AdjustPlayerValues();

            Time++;
        }

        public void AdjustPlayerValues()
        {
            Projectile.spriteDirection = Projectile.direction = Owner.direction;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D worleyNoise = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BlobbyNoise").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 scale = Projectile.Size / worleyNoise.Size() * 2f;
            float spinRotation = Main.GlobalTimeWrappedHourly * 2.4f;
            
            GameShaders.Misc["CalamityMod:ExoVortex"].Apply();
            
            // Draw the vortex.
            for (int i = 0; i < CalamityUtils.ExoPalette.Length; i++)
            {
                float spinDirection = (i % 2f == 0f).ToDirectionInt();
                Vector2 drawOffset = (MathHelper.TwoPi * i / CalamityUtils.ExoPalette.Length + Main.GlobalTimeWrappedHourly * spinDirection * 4f).ToRotationVector2() * Projectile.scale * 15f;
                Main.spriteBatch.Draw(worleyNoise, drawPosition + drawOffset, null, CalamityUtils.ExoPalette[i], spinDirection * spinRotation, worleyNoise.Size() * 0.5f, scale, 0, 0f);
            }

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
