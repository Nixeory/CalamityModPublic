﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor;
using CalamityMod.Items.VanillaArmorChanges;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;
using CalamityMod.EntitySources;

namespace CalamityMod.CalPlayer
{
    public partial class CalamityPlayer : ModPlayer
    {
        #region On Hit Anything
        public override void OnHitAnything(float x, float y, Entity victim)
        {
            rageCombatFrames = RageCombatDelayTime;

            if (AdamantiteSet)
            {
                adamantiteSetDefenseBoostInterpolant += 1.75f / AdamantiteArmorSetChange.TimeUntilBoostCompletelyDecays;
                adamantiteSetDefenseBoostInterpolant = MathHelper.Clamp(adamantiteSetDefenseBoostInterpolant, 0f, 1f);
                AdamantiteSetDecayDelay = AdamantiteArmorSetChange.TimeUntilDecayBeginsAfterAttacking;
            }
        }
        #endregion

        #region On Hit NPC
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            // Handle on-hit melee effects for the gem tech armor set.
            GemTechState.MeleeOnHitEffects(target);

            // Handle on-hit melee effects for the mythril armor set.
            MythrilArmorSetChange.OnHitEffects(target, damage, Player);

            if (witheringWeaponEnchant)
                witheringDamageDone += (int)(damage * (crit ? 2D : 1D));

            if (flamingItemEnchant)
                target.AddBuff(BuffType<VulnerabilityHex>(), VulnerabilityHex.AflameDuration);

            target.Calamity().IncreasedHeatEffects = fireball || cinnamonRoll;

            target.Calamity().IncreasedColdEffects = eskimoSet;

            target.Calamity().IncreasedSicknessAndWaterEffects = evergreenGin;

            switch (item.type)
            {
                case ItemID.CobaltSword:
                    target.Calamity().miscDefenseLoss = (int)(target.defense * 0.25);
                    break;

                case ItemID.PalladiumSword:
                    if (!target.canGhostHeal || Player.moonLeech)
                        return;
                    Player.lifeRegenTime += 2;
                    break;

                case ItemID.MythrilSword:
                    target.damage = (int)(target.defDamage * 0.9);
                    break;

                case ItemID.OrichalcumSword:
                    if (Player.petalTimer > 0)
                        Player.petalTimer /= 2;
                    break;

                case ItemID.AdamantiteSword:
                    float slowDownMult = 0.5f;
                    if (CalamityLists.enemyImmunityList.Contains(target.type) || target.boss)
                        slowDownMult = 0.95f;
                    target.velocity *= slowDownMult;
                    break;

                case ItemID.CandyCaneSword:
                    if (!target.canGhostHeal || Player.moonLeech)
                        return;
                    Player.statLife += 2;
                    Player.HealEffect(2);
                    break;

                case ItemID.TheHorsemansBlade:
                    if (target.SpawnedFromStatue)
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile proj = Main.projectile[i];
                            if (proj.type == ProjectileID.FlamingJack && proj.owner == Main.myPlayer)
                                proj.Kill();
                        }
                    }
                    break;

                case ItemID.DeathSickle:
                    target.AddBuff(BuffType<WhisperingDeath>(), 120);
                    break;

                case ItemID.Excalibur:
                case ItemID.TrueExcalibur:
                    target.AddBuff(BuffType<HolyFlames>(), 120);
                    break;

                case ItemID.FieryGreatsword:
                case ItemID.MoltenPickaxe:
                case ItemID.MoltenHamaxe:
                    target.AddBuff(BuffID.OnFire, 120);
                    break;

                case ItemID.BladeofGrass:
                    target.AddBuff(BuffID.Poisoned, 210);
                    break;

                case ItemID.BeeKeeper:
                    target.AddBuff(BuffID.Poisoned, 240);
                    break;

                case ItemID.LightsBane:
                case ItemID.NightsEdge:
                    target.AddBuff(BuffID.ShadowFlame, 120);
                    break;

                case ItemID.TrueNightsEdge:
                    target.AddBuff(BuffID.CursedInferno, 60);
                    target.AddBuff(BuffID.ShadowFlame, 120);
                    break;

                case ItemID.BloodButcherer:
                    target.AddBuff(BuffType<BurningBlood>(), 60);
                    break;

                case ItemID.IceSickle:
                case ItemID.Frostbrand:
                    target.AddBuff(BuffID.Frostburn, 300);
                    break;

                case ItemID.IceBlade:
                    target.AddBuff(BuffID.Frostburn, 120);
                    break;
            }

            ItemLifesteal(target, item, damage);
            ItemOnHit(item, damage, target.Center, crit, (target.damage > 5 || target.boss) && !target.SpawnedFromStatue);
            NPCDebuffs(target, item.CountsAsClass<MeleeDamageClass>(), item.CountsAsClass<RangedDamageClass>(), item.CountsAsClass<MagicDamageClass>(), item.CountsAsClass<SummonDamageClass>(), item.Calamity().rogue, false);

            // Shattered Community tracks all damage dealt with Rage Mode (ignoring dummies).
            if (target.type == NPCID.TargetDummy || target.type == NPCType<SuperDummyNPC>())
                return;
            if (rageModeActive && shatteredCommunity)
                ShatteredCommunity.AccumulateRageDamage(Player, this, damage);
        }
        #endregion

        #region On Hit NPC With Proj
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;
            CalamityGlobalNPC cgn = target.Calamity();

            // Handle on-hit melee effects for the gem tech armor set.
            if (proj.CountsAsClass(DamageClass.Melee))
                GemTechState.MeleeOnHitEffects(target);

            // Handle on-hit projectiles effects for the mythril armor set.
            if (proj.type != ModContent.ProjectileType<MythrilFlare>())
                MythrilArmorSetChange.OnHitEffects(target, damage, Player);

            if (witheringWeaponEnchant)
                witheringDamageDone += (int)(damage * (crit ? 2D : 1D));

            cgn.IncreasedHeatEffects = fireball || cinnamonRoll;
            cgn.IncreasedColdEffects = eskimoSet;
            cgn.IncreasedSicknessAndWaterEffects = evergreenGin;

            switch (proj.type)
            {
                case ProjectileID.CobaltNaginata:
                    target.Calamity().miscDefenseLoss = (int)(target.defense * 0.25);
                    break;

                case ProjectileID.PalladiumPike:
                    if (!target.canGhostHeal || Player.moonLeech)
                        return;
                    Player.lifeRegenTime += 2;
                    break;

                case ProjectileID.MythrilHalberd:
                    target.damage = (int)(target.defDamage * 0.9);
                    break;

                case ProjectileID.OrichalcumHalberd:
                    if (Player.petalTimer > 0)
                        Player.petalTimer /= 2;
                    break;

                case ProjectileID.AdamantiteGlaive:
                    float slowDownMult = 0.5f;
                    if (CalamityLists.enemyImmunityList.Contains(target.type) || target.boss)
                        slowDownMult = 0.95f;
                    target.velocity *= slowDownMult;
                    break;

                case ProjectileID.FruitcakeChakram:
                    if (!target.canGhostHeal || Player.moonLeech)
                        return;
                    Player.statLife += 2;
                    Player.HealEffect(2);
                    break;

                case ProjectileID.TheRottedFork:
                case ProjectileID.TheMeatball:
                case ProjectileID.CrimsonYoyo:
                case ProjectileID.BloodCloudRaining:
                case ProjectileID.BloodRain:
                    target.AddBuff(BuffType<BurningBlood>(), 60);
                    break;

                case ProjectileID.BallOHurt:
                case ProjectileID.CorruptYoyo:
                    target.AddBuff(BuffID.ShadowFlame, 60);
                    break;

                case ProjectileID.ObsidianSwordfish:
                    target.AddBuff(BuffID.OnFire, 180);
                    break;

                case ProjectileID.Flamelash:
                    target.AddBuff(BuffID.OnFire, 300);
                    break;

                case ProjectileID.BallofFire:
                    target.AddBuff(BuffID.OnFire, 180);
                    break;

                case ProjectileID.Cascade:
                case ProjectileID.Sunfury:
                case ProjectileID.Flamarang:
                case ProjectileID.FireArrow:
                    target.AddBuff(BuffID.OnFire, 120);
                    break;

                case ProjectileID.Spark:
                    target.AddBuff(BuffID.OnFire, 30);
                    break;

                case ProjectileID.GolemFist:
                    target.AddBuff(BuffType<ArmorCrunch>(), 180);
                    break;

                case ProjectileID.DeathSickle:
                    target.AddBuff(BuffType<WhisperingDeath>(), 60);
                    break;

                case ProjectileID.LightBeam:
                case ProjectileID.Gungnir:
                case ProjectileID.PaladinsHammerFriendly:
                    target.AddBuff(BuffType<HolyFlames>(), 120);
                    break;

                case ProjectileID.DarkLance:
                    target.AddBuff(BuffID.ShadowFlame, 120);
                    break;

                case ProjectileID.PoisonedKnife:
                    target.AddBuff(BuffID.Poisoned, 300);
                    break;

                case ProjectileID.ThornChakram:
                    target.AddBuff(BuffID.Poisoned, 210);
                    break;

                case ProjectileID.Bee:
                case ProjectileID.GiantBee:
                    target.AddBuff(BuffID.Poisoned, 120);
                    break;

                case ProjectileID.Wasp:
                    target.AddBuff(BuffID.Poisoned, 120);
                    target.AddBuff(BuffID.Venom, 60);
                    break;

                case ProjectileID.BoneArrow:
                    target.AddBuff(BuffType<ArmorCrunch>(), 300);
                    break;

                case ProjectileID.FrostBlastFriendly:
                case ProjectileID.NorthPoleWeapon:
                    target.AddBuff(BuffID.Frostburn, 300);
                    break;

                case ProjectileID.FrostBoltStaff:
                case ProjectileID.IceSickle:
                case ProjectileID.FrostBoltSword:
                case ProjectileID.FrostArrow:
                case ProjectileID.NorthPoleSpear:
                case ProjectileID.Amarok:
                    target.AddBuff(BuffID.Frostburn, 180);
                    break;

                case ProjectileID.Blizzard:
                case ProjectileID.NorthPoleSnowflake:
                    target.AddBuff(BuffID.Frostburn, 120);
                    break;

                case ProjectileID.SnowBallFriendly:
                case ProjectileID.IceBoomerang:
                case ProjectileID.IceBolt:
                case ProjectileID.FrostDaggerfish:
                case ProjectileID.FrostburnArrow:
                    target.AddBuff(BuffID.Frostburn, 60);
                    break;

                case ProjectileID.NightBeam:
                    target.AddBuff(BuffID.CursedInferno, 60);
                    target.AddBuff(BuffID.ShadowFlame, 120);
                    break;
            }

            // TODO -- This should be removed, why is this here?
            if (ProjectileID.Sets.StardustDragon[proj.type])
                target.immune[proj.owner] = 10;

            if (!proj.npcProj && !proj.trap && proj.friendly)
            {
                if ((plaguebringerCarapace || uberBees) && CalamityLists.friendlyBeeList.Contains(proj.type))
                    target.AddBuff(BuffType<Plague>(), 300);

                if (proj.type == ProjectileID.IchorArrow && Player.ActiveItem().type == ItemType<RaidersGlory>())
                    target.AddBuff(BuffID.Midas, 300, false);

                // All projectiles fired from Soma Prime are marked using CalamityGlobalProjectile
                CalamityGlobalProjectile cgp = proj.Calamity();
                if (cgp.appliesSomaShred)
                {
                    target.AddBuff(BuffType<Shred>(), 320);
                    // This information cannot be transferred through the buff, but is necessary to calculate damage
                    cgn.somaShredApplicator = Player.whoAmI;
                }

                ProjLifesteal(target, proj, damage, crit);
                ProjOnHit(proj, target.Center, crit, (target.damage > 5 || target.boss) && !target.SpawnedFromStatue);
                NPCDebuffs(target, proj.CountsAsClass(DamageClass.Melee), proj.CountsAsClass(DamageClass.Ranged), proj.CountsAsClass(DamageClass.Magic), proj.IsSummon(), cgp.rogue, true);

                // Shattered Community tracks all damage dealt with Rage Mode (ignoring dummies).
                if (target.type == NPCID.TargetDummy || target.type == NPCType<SuperDummyNPC>())
                    return;

                if (rageModeActive && shatteredCommunity)
                    ShatteredCommunity.AccumulateRageDamage(Player, this, damage);
            }
        }
        #endregion

        #region PvP
        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            switch (item.type)
            {
                case ItemID.PalladiumSword:
                case ItemID.PalladiumPike:
                    if (Player.moonLeech)
                        return;
                    Player.lifeRegenTime += 2;
                    break;

                case ItemID.OrichalcumSword:
                case ItemID.OrichalcumHalberd:
                    if (Player.petalTimer > 0)
                        Player.petalTimer /= 2;
                    break;

                case ItemID.CandyCaneSword:
                    if (Player.moonLeech)
                        return;
                    Player.statLife += 2;
                    Player.HealEffect(2);
                    break;

                case ItemID.DeathSickle:
                    target.AddBuff(BuffType<WhisperingDeath>(), 120);
                    break;

                case ItemID.Excalibur:
                case ItemID.TrueExcalibur:
                    target.AddBuff(BuffType<HolyFlames>(), 120);
                    break;

                case ItemID.FieryGreatsword:
                case ItemID.MoltenPickaxe:
                case ItemID.MoltenHamaxe:
                    target.AddBuff(BuffID.OnFire, 120);
                    break;

                case ItemID.BladeofGrass:
                    target.AddBuff(BuffID.Poisoned, 210);
                    break;

                case ItemID.BeeKeeper:
                    target.AddBuff(BuffID.Poisoned, 240);
                    break;

                case ItemID.LightsBane:
                case ItemID.NightsEdge:
                    target.AddBuff(BuffType<Shadowflame>(), 120);
                    break;

                case ItemID.TrueNightsEdge:
                    target.AddBuff(BuffID.CursedInferno, 60);
                    target.AddBuff(BuffType<Shadowflame>(), 120);
                    break;

                case ItemID.BloodButcherer:
                    target.AddBuff(BuffType<BurningBlood>(), 60);
                    break;

                case ItemID.IceSickle:
                case ItemID.Frostbrand:
                    target.AddBuff(BuffID.Frostburn, 300);
                    break;

                case ItemID.IceBlade:
                    target.AddBuff(BuffID.Frostburn, 120);
                    break;
            }
            ItemOnHit(item, damage, target.Center, crit, true);
            PvpDebuffs(target, item.CountsAsClass<MeleeDamageClass>(), item.CountsAsClass<RangedDamageClass>(), item.CountsAsClass<MagicDamageClass>(), item.CountsAsClass<SummonDamageClass>(), item.Calamity().rogue, false);
        }

        public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            switch (proj.type)
            {
                case ProjectileID.PalladiumPike:
                    if (Player.moonLeech)
                        return;
                    Player.lifeRegenTime += 2;
                    break;

                case ProjectileID.OrichalcumHalberd:
                    if (Player.petalTimer > 0)
                        Player.petalTimer /= 2;
                    break;

                case ProjectileID.FruitcakeChakram:
                    if (Player.moonLeech)
                        return;
                    Player.statLife += 2;
                    Player.HealEffect(2);
                    break;

                case ProjectileID.TheRottedFork:
                    target.AddBuff(BuffType<BurningBlood>(), 60);
                    break;

                case ProjectileID.ObsidianSwordfish:
                    target.AddBuff(BuffID.OnFire, 180);
                    break;

                case ProjectileID.Flamelash:
                    target.AddBuff(BuffID.OnFire, 300);
                    break;

                case ProjectileID.BallofFire:
                    target.AddBuff(BuffID.OnFire, 180);
                    break;

                case ProjectileID.Cascade:
                case ProjectileID.Sunfury:
                case ProjectileID.Flamarang:
                case ProjectileID.FireArrow:
                    target.AddBuff(BuffID.OnFire, 120);
                    break;

                case ProjectileID.Spark:
                    target.AddBuff(BuffID.OnFire, 30);
                    break;

                case ProjectileID.GolemFist:
                    target.AddBuff(BuffType<ArmorCrunch>(), 180);
                    break;

                case ProjectileID.DeathSickle:
                    target.AddBuff(BuffType<WhisperingDeath>(), 60);
                    break;

                case ProjectileID.LightBeam:
                case ProjectileID.Gungnir:
                case ProjectileID.PaladinsHammerFriendly:
                    target.AddBuff(BuffType<HolyFlames>(), 120);
                    break;

                case ProjectileID.DarkLance:
                    target.AddBuff(BuffType<Shadowflame>(), 120);
                    break;

                case ProjectileID.PoisonedKnife:
                    target.AddBuff(BuffID.Poisoned, 300);
                    break;

                case ProjectileID.ThornChakram:
                    target.AddBuff(BuffID.Poisoned, 210);
                    break;

                case ProjectileID.Bee:
                case ProjectileID.GiantBee:
                    target.AddBuff(BuffID.Poisoned, 120);
                    break;

                case ProjectileID.Wasp:
                    target.AddBuff(BuffID.Poisoned, 120);
                    target.AddBuff(BuffID.Venom, 60);
                    break;

                case ProjectileID.BoneArrow:
                    target.AddBuff(BuffType<ArmorCrunch>(), 300);
                    break;

                case ProjectileID.FrostBlastFriendly:
                case ProjectileID.NorthPoleWeapon:
                    target.AddBuff(BuffID.Frostburn, 300);
                    break;

                case ProjectileID.FrostBoltStaff:
                case ProjectileID.IceSickle:
                case ProjectileID.FrostBoltSword:
                case ProjectileID.FrostArrow:
                case ProjectileID.NorthPoleSpear:
                case ProjectileID.Amarok:
                    target.AddBuff(BuffID.Frostburn, 180);
                    break;

                case ProjectileID.Blizzard:
                case ProjectileID.NorthPoleSnowflake:
                    target.AddBuff(BuffID.Frostburn, 120);
                    break;

                case ProjectileID.SnowBallFriendly:
                case ProjectileID.IceBoomerang:
                case ProjectileID.IceBolt:
                case ProjectileID.FrostDaggerfish:
                case ProjectileID.FrostburnArrow:
                    target.AddBuff(BuffID.Frostburn, 60);
                    break;

                case ProjectileID.NightBeam:
                    target.AddBuff(BuffID.CursedInferno, 60);
                    target.AddBuff(BuffType<Shadowflame>(), 120);
                    break;
            }

            if (!proj.npcProj && !proj.trap && proj.friendly)
            {
                if ((plaguebringerCarapace || uberBees) && CalamityLists.friendlyBeeList.Contains(proj.type))
                {
                    target.AddBuff(BuffType<Plague>(), 300);
                }
                ProjOnHit(proj, target.Center, crit, true);
                PvpDebuffs(target, proj.CountsAsClass(DamageClass.Melee), proj.CountsAsClass(DamageClass.Ranged), proj.CountsAsClass(DamageClass.Magic), proj.IsSummon(), proj.Calamity().rogue, true);
            }
        }
        #endregion

        #region Item
        public void ItemOnHit(Item item, int damage, Vector2 position, bool crit, bool npcCheck)
        {
            var source = Player.GetProjectileSource_Item(item);
            if (!item.CountsAsClass<MeleeDamageClass>() && Player.meleeEnchant == 7)
                Projectile.NewProjectile(source, position, Player.velocity, ProjectileID.ConfettiMelee, 0, 0f, Player.whoAmI);

            if (reaverDefense)
                Player.lifeRegenTime += 1;

            if (Player.whoAmI == Main.myPlayer && desertProwler && crit && item.CountsAsClass<RangedDamageClass>())
            {
                bool noTornado = Player.ownedProjectileCounts[ProjectileType<DesertMark>()] < 1 && Player.ownedProjectileCounts[ProjectileType<DesertTornado>()] < 1;
                if (noTornado && Main.rand.NextBool(15))
                {
                    Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<DesertMark>(), damage, item.knockBack, Player.whoAmI);
                }
            }

            if (npcCheck)
            {
                if (item.CountsAsClass<MeleeDamageClass>() && aBulwarkRare && aBulwarkRareTimer == 0)
                {
                    aBulwarkRareTimer = 10;
                    for (int n = 0; n < 3; n++)
                        CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 29f, ProjectileType<AstralStar>(), (int)(320 * Player.AverageDamage()), 5f, Player.whoAmI);
                }
                if (unstablePrism && crit && Player.ownedProjectileCounts[ProjectileType<UnstableSpark>()] < 5)
                {
                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f);
                        Projectile.NewProjectile(source, position, velocity, ProjectileType<UnstableSpark>(), (int)(15 * Player.AverageDamage()), 0f, Player.whoAmI);
                    }
                }
                if (astralStarRain && crit && astralStarRainCooldown <= 0)
                {
                    astralStarRainCooldown = 60;
                    for (int n = 0; n < 3; n++)
                    {
                        int projectileType = Utils.SelectRandom(Main.rand, new int[]
                        {
                            ProjectileType<AstralStar>(),
                            ProjectileID.HallowStar,
                            ProjectileType<FallenStarProj>()
                        });
                        Projectile star = CalamityUtils.ProjectileRain(source, position, 400f, 100f, 500f, 800f, 12f, projectileType, (int)(120 * Player.AverageDamage()), 5f, Player.whoAmI);
                        if (star.whoAmI.WithinBounds(Main.maxProjectiles))
                            star.Calamity().forceTypeless = true;
                    }
                }
            }

            if (item.CountsAsClass<MeleeDamageClass>())
            {
                titanBoost = 600;
                if (npcCheck)
                {
                    if (ataxiaGeyser && Player.ownedProjectileCounts[ProjectileType<ChaosGeyser>()] < 3)
                    {
                        // Ataxia True Melee Geysers: 15%, softcap starts at 300 base damage
                        int geyserDamage = CalamityUtils.DamageSoftCap(damage * 0.15, 45);
                        Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<ChaosGeyser>(), geyserDamage, 2f, Player.whoAmI, 0f, 0f);
                    }
                    if (soaring)
                    {
                        double useTimeMultiplier = 0.85 + (item.useTime * item.useAnimation / 3600D); //28 * 28 = 784 is average so that equals 784 / 3600 = 0.217777 + 1 = 21.7% boost
                        double wingTimeFraction = Player.wingTimeMax / 20D;
                        double meleeStatMultiplier = (double)(Player.GetDamage(DamageClass.Melee) * (float)(Player.GetCritChance(DamageClass.Melee) / 10D));

                        if (Player.wingTime < Player.wingTimeMax)
                            Player.wingTime += (int)(useTimeMultiplier * (wingTimeFraction + meleeStatMultiplier));

                        if (Player.wingTime > Player.wingTimeMax)
                            Player.wingTime = Player.wingTimeMax;
                    }
                    if (bloodflareMelee && item.CountsAsClass<MeleeDamageClass>() && bloodflareMeleeHits < 15 && !bloodflareFrenzy && !Player.HasCooldown(BloodflareFrenzy.ID))
                        bloodflareMeleeHits++;
                }
            }
        }
        #endregion

        #region Proj On Hit
        public void ProjOnHit(Projectile proj, Vector2 position, bool crit, bool npcCheck)
        {
            CalamityGlobalProjectile modProj = proj.Calamity();
            var source = proj.GetProjectileSource_FromThis();
            bool hasClass = proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Ranged) || proj.CountsAsClass(DamageClass.Magic) || proj.IsSummon() || modProj.rogue;

            //flask of party affects all types of weapons, !proj.CountsAsClass(DamageClass.Melee) is to prevent double flask effects
            if (!proj.CountsAsClass(DamageClass.Melee) && Player.meleeEnchant == 7)
                Projectile.NewProjectile(source, position, proj.velocity, ProjectileID.ConfettiMelee, 0, 0f, proj.owner);

            if (alchFlask && Player.ownedProjectileCounts[ProjectileType<PlagueSeeker>()] < 3 && hasClass)
            {
                Projectile projectile = CalamityGlobalProjectile.SpawnOrb(proj, (int)(30 * Player.AverageDamage()), ProjectileType<PlagueSeeker>(), 400f, 12f);
                if (projectile.whoAmI.WithinBounds(Main.maxProjectiles))
                    Main.projectile[projectile.whoAmI].Calamity().forceTypeless = true;
            }

            if (theBee && Player.statLife >= Player.statLifeMax2)
                SoundEngine.PlaySound(SoundID.Item110, proj.Center);

            if (reaverDefense)
                Player.lifeRegenTime += 1;

            if (npcCheck)
            {
                if (unstablePrism && crit && Player.ownedProjectileCounts[ProjectileType<UnstableSpark>()] < 5)
                {
                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f);
                        Projectile.NewProjectile(source, position, velocity, ProjectileType<UnstableSpark>(), (int)(15 * Player.AverageDamage()), 0f, Player.whoAmI);
                    }
                }

                if (astralStarRain && crit && astralStarRainCooldown <= 0)
                {
                    astralStarRainCooldown = 60;
                    for (int n = 0; n < 3; n++)
                    {
                        int projectileType = Utils.SelectRandom(Main.rand, new int[]
                        {
                            ProjectileType<AstralStar>(),
                            ProjectileID.HallowStar,
                            ProjectileType<FallenStarProj>()
                        });
                        Projectile star = CalamityUtils.ProjectileRain(source, position, 400f, 100f, 500f, 800f, 25f, projectileType, (int)(120 * Player.AverageDamage()), 5f, Player.whoAmI);
                        if (star.whoAmI.WithinBounds(Main.maxProjectiles))
                            star.Calamity().forceTypeless = true;
                    }
                }
            }

            if (proj.CountsAsClass(DamageClass.Melee))
                MeleeOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.CountsAsClass(DamageClass.Ranged))
                RangedOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.CountsAsClass(DamageClass.Magic))
                MagicOnHit(proj, modProj, position, crit, npcCheck);
            if (proj.IsSummon())
                SummonOnHit(proj, modProj, position, crit, npcCheck);
            if (modProj.rogue)
                RogueOnHit(proj, modProj, position, crit, npcCheck);
        }

        #region Melee
        private void MeleeOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetProjectileSource_FromThis();
            Item heldItem = Player.ActiveItem();

            if (modProj.trueMelee)
            {
                titanBoost = 600;
                if (soaring)
                {
                    double useTimeMultiplier = 0.85 + (heldItem.useTime * heldItem.useAnimation / 3600D); //28 * 28 = 784 is average so that equals 784 / 3600 = 0.217777 + 1 = 21.7% boost
                    double wingTimeFraction = Player.wingTimeMax / 20D;
                    double meleeStatMultiplier = Player.GetDamage(DamageClass.Melee) * (float)(Player.GetCritChance(DamageClass.Melee) / 10D);

                    if (Player.wingTime < Player.wingTimeMax)
                        Player.wingTime += (int)(useTimeMultiplier * (wingTimeFraction + meleeStatMultiplier));

                    if (Player.wingTime > Player.wingTimeMax)
                        Player.wingTime = Player.wingTimeMax;
                }
                if (aBulwarkRare && aBulwarkRareTimer == 0)
                {
                    aBulwarkRareTimer = 10;
                    for (int n = 0; n < 3; n++)
                        CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 29f, ProjectileType<AstralStar>(), (int)(320 * Player.AverageDamage()), 5f, Player.whoAmI);
                }
            }

            if (npcCheck)
            {
                if (ataxiaGeyser && Player.ownedProjectileCounts[ProjectileType<ChaosGeyser>()] < 3)
                {
                    // Ataxia Melee Geysers: 15%, softcap starts at 240 base damage
                    int geyserDamage = CalamityUtils.DamageSoftCap(proj.damage * 0.15, 36);
                    Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<ChaosGeyser>(), geyserDamage, 0f, Player.whoAmI, 0f, 0f);
                }
                if (bloodflareMelee && modProj.trueMelee && bloodflareMeleeHits < 15 && !bloodflareFrenzy && !Player.HasCooldown(BloodflareFrenzy.ID))
                    bloodflareMeleeHits++;
            }
        }
        #endregion

        #region Ranged
        private void RangedOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetProjectileSource_FromThis();
            if (Player.whoAmI == Main.myPlayer && desertProwler && crit)
            {
                bool noTornado = Player.ownedProjectileCounts[ProjectileType<DesertMark>()] < 1 && Player.ownedProjectileCounts[ProjectileType<DesertTornado>()] < 1;
                if (noTornado && Main.rand.NextBool(15))
                {
                    Projectile.NewProjectile(source, position, Vector2.Zero, ProjectileType<DesertMark>(), proj.damage, proj.knockBack, Player.whoAmI);
                }
            }

            if (npcCheck)
            {
                if (tarraRanged && proj.CountsAsClass(DamageClass.Ranged) && tarraRangedCooldown <= 0)
                {
                    tarraRangedCooldown = 60;
                    for (int l = 0; l < 2; l++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                        int leafDamage = (int)(0.25 * proj.damage);
                        int leaf = Projectile.NewProjectile(source, position, velocity, ProjectileID.Leaf, leafDamage, 0f, Player.whoAmI);
                        if (leaf.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[leaf].Calamity().forceTypeless = true;
                            Main.projectile[leaf].netUpdate = true;
                        }
                    }
                    if (Player.ownedProjectileCounts[ProjectileType<TarraEnergy>()] < 2)
                    {
                        for (int projCount = 0; projCount < 2; projCount++)
                        {
                            Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                            int energyDamage = (int)(0.33 * proj.damage);
                            Projectile.NewProjectile(source, proj.Center, velocity, ProjectileType<TarraEnergy>(), energyDamage, 0f, proj.owner);
                        }
                    }
                }
                if (proj.type == ProjectileType<PolarStar>())
                {
                    polarisBoostCounter += 1;
                }
            }
        }
        #endregion

        #region Magic
        private void MagicOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetProjectileSource_FromThis();
            if (ataxiaMage && ataxiaDmg <= 0)
            {
                int orbDamage = (int)(proj.damage * 0.6);
                CalamityGlobalProjectile.SpawnOrb(proj, orbDamage, ProjectileType<AtaxiaOrb>(), 800f, 20f);
                int cooldown = (int)(orbDamage * 0.5);
                ataxiaDmg += cooldown;
            }
            if (tarraMage && crit)
            {
                tarraCrits++;
            }
            if (npcCheck)
            {
                if (bloodflareMage && bloodflareMageCooldown <= 0 && crit)
                {
                    bloodflareMageCooldown = 120;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                        // Bloodflare Mage Fireballs: 3 x 50%, softcap starts at 500 base damage to not overly punish slow weapons
                        int bloodflareFireballDamage = CalamityUtils.DamageSoftCap(proj.damage * 0.5, 250);
                        int fire = Projectile.NewProjectile(source, position, velocity, ProjectileID.BallofFire, bloodflareFireballDamage, 0f, Player.whoAmI);
                        if (fire.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[fire].Calamity().forceTypeless = true;
                            Main.projectile[fire].netUpdate = true;
                        }
                    }
                }
            }
            if (silvaMage && silvaMageCooldown <= 0 && (proj.penetrate == 1 || proj.timeLeft <= 5))
            {
                silvaMageCooldown = 300;
                SoundEngine.PlaySound(SoundID.Zombie, (int)proj.position.X, (int)proj.position.Y, 103);
                // Silva Mage Blasts: 800 + 60%, softcap on the whole combined thing starts at 1400
                int silvaBurstDamage = CalamityUtils.DamageSoftCap(800.0 + 0.6 * proj.damage, 1400);
                Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<SilvaBurst>(), silvaBurstDamage, 8f, Player.whoAmI);
            }
        }
        #endregion

        #region Summon
        private void SummonOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var source = proj.GetProjectileSource_FromThis();
            if (npcCheck)
            {
                if (phantomicArtifact)
                {
                    int restoreBuff = BuffType<PhantomicRestorationBuff>();
                    int empowerBuff = BuffType<PhantomicEmpowermentBuff>();
                    int shieldBuff = BuffType<PhantomicArmourBuff>();
                    int buffType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        restoreBuff,
                        empowerBuff,
                        shieldBuff
                    });
                    Player.AddBuff(buffType, 60);
                    if (buffType == restoreBuff)
                    {
                        if (phantomicHeartRegen == 1000 && Player.ownedProjectileCounts[ProjectileType<PhantomicHeart>()] == 0 && Main.rand.NextBool(20))
                        {
                            Vector2 target = proj.Center;
                            target.Y += Main.rand.Next(-50, 50);
                            target.X += Main.rand.Next(-50, 50);
                            Projectile.NewProjectile(source, target, Vector2.Zero, ProjectileType<PhantomicHeart>(), 0, 0f, Player.whoAmI, 0f);
                        }
                    }
                    else if (buffType == empowerBuff)
                    {
                        if (Player.ownedProjectileCounts[ProjectileType<PhantomicDagger>()] < 3 && Main.rand.NextBool(10))
                        {
                            int damage = (int)(75 * Player.MinionDamage());
                            int dagger = Projectile.NewProjectile(source, proj.position, proj.velocity, ProjectileType<PhantomicDagger>(), damage, 1f, Player.whoAmI, 0f);
                            if (dagger.WithinBounds(Main.maxProjectiles))
                                Main.projectile[dagger].Calamity().forceTypeless = true;
                        }
                    }
                    else
                    {
                        if (Player.ownedProjectileCounts[ProjectileType<PhantomicShield>()] == 0 && phantomicBulwarkCooldown == 0)
                            Projectile.NewProjectile(source, Player.position, Vector2.Zero, ProjectileType<PhantomicShield>(), 0, 0f, Player.whoAmI, 0f);
                    }
                }
                else if (hallowedRune)
                {
                    int buffType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        BuffType<HallowedRuneAtkBuff>(),
                        BuffType<HallowedRuneRegenBuff>(),
                        BuffType<HallowedRuneDefBuff>()
                    });
                    Player.AddBuff(buffType, 60);
                }
                else if (sGenerator)
                {
                    int buffType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        BuffType<SpiritGeneratorAtkBuff>(),
                        BuffType<SpiritGeneratorRegenBuff>(),
                        BuffType<SpiritGeneratorDefBuff>()
                    });
                    Player.AddBuff(buffType, 60);
                }
            }

            // Fearmonger set's colossal life regeneration
            if (fearmongerSet)
            {
                fearmongerRegenFrames += 10;
                if (fearmongerRegenFrames > 90)
                    fearmongerRegenFrames = 90;
            }

            //Priorities: Nucleogenesis => Starbuster Core => Nuclear Rod => Jelly-Charged Battery
            List<int> summonExceptionList = new List<int>()
            {
                ProjectileType<EnergyOrb>(),
                ProjectileType<IrradiatedAura>(),
                ProjectileType<SummonAstralExplosion>(),
                ProjectileType<ApparatusExplosion>(),
                ProjectileType<HallowedStarSummon>()
            };

            if (summonExceptionList.TrueForAll(x => proj.type != x))
            {
                if (jellyDmg <= 0)
                {
                    if (nucleogenesis)
                    {
                        int projectile = Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<ApparatusExplosion>(), (int)(60 * Player.MinionDamage()), 4f, proj.owner);
                        if (projectile.WithinBounds(Main.maxProjectiles))
                            Main.projectile[projectile].Calamity().forceTypeless = true;
                        jellyDmg = 100f;
                    }
                    else if (starbusterCore)
                    {
                        int projectile = Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<SummonAstralExplosion>(), (int)(40 * Player.MinionDamage()), 3.5f, proj.owner);
                        if (projectile.WithinBounds(Main.maxProjectiles))
                            Main.projectile[projectile].Calamity().forceTypeless = true;
                        jellyDmg = 60f;
                    }
                    else if (nuclearRod)
                    {
                        int projectile = Projectile.NewProjectile(source, proj.Center, Vector2.Zero, ProjectileType<IrradiatedAura>(), (int)(20 * Player.MinionDamage()), 0f, proj.owner);
                        if (projectile.WithinBounds(Main.maxProjectiles))
                            Main.projectile[projectile].Calamity().forceTypeless = true;
                        jellyDmg = 60f;
                    }
                    else if (jellyChargedBattery)
                    {
                        CalamityGlobalProjectile.SpawnOrb(proj, (int)(15 * Player.MinionDamage()), ProjectileType<EnergyOrb>(), 800f, 15f);
                        jellyDmg = 60f;
                    }
                }

                if (hallowedPower)
                {
                    if (hallowedRuneCooldown <= 0)
                    {
                        hallowedRuneCooldown = 180;
                        Vector2 spawnPosition = position - new Vector2(0f, 920f).RotatedByRandom(0.3f);
                        float speed = Main.rand.NextFloat(17f, 23f);
                        int projectile = Projectile.NewProjectile(source, spawnPosition, Vector2.Normalize(position - spawnPosition) * speed, ProjectileType<HallowedStarSummon>(), (int)(30 * Player.MinionDamage()), 3f, proj.owner);
                        if (projectile.WithinBounds(Main.maxProjectiles))
                            Main.projectile[projectile].Calamity().forceTypeless = true;
                    }
                }
            }
        }
        #endregion

        #region Rogue
        private void RogueOnHit(Projectile proj, CalamityGlobalProjectile modProj, Vector2 position, bool crit, bool npcCheck)
        {
            var spawnSource = proj.GetProjectileSource_FromThis();
            if (modProj.stealthStrike && dragonScales && CalamityUtils.CountProjectiles(ProjectileType<InfernadoFriendly>()) < 1)
            {
                int projTileX = (int)(proj.Center.X / 16f);
                int projTileY = (int)(proj.Center.Y / 16f);
                int distance = 100;
                if (projTileX < 10)
                {
                    projTileX = 10;
                }
                if (projTileX > Main.maxTilesX - 10)
                {
                    projTileX = Main.maxTilesX - 10;
                }
                if (projTileY < 10)
                {
                    projTileY = 10;
                }
                if (projTileY > Main.maxTilesY - distance - 10)
                {
                    projTileY = Main.maxTilesY - distance - 10;
                }
                for (int x = projTileX; x < projTileX + distance; x++)
                {
                    Tile tile = Main.tile[projTileX, projTileY];
                    if (tile.HasTile && (Main.tileSolid[tile.TileType] || tile.LiquidAmount != 0))
                    {
                        projTileX = x;
                        break;
                    }
                }
                int projectileIndex = Projectile.NewProjectile(spawnSource, projTileX * 16 + 8, projTileY * 16 - 24, 0f, 0f, ProjectileType<InfernadoFriendly>(), (int)(550 * Player.RogueDamage()), 15f, Main.myPlayer, 16f, 16f);
                if (projectileIndex.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[projectileIndex].Calamity().forceTypeless = true;
                    Main.projectile[projectileIndex].netUpdate = true;
                    Main.projectile[projectileIndex].localNPCHitCooldown = 10;
                }
            }

            if (crit && tarraThrowing && tarraThrowingCrits < 25 && !tarragonImmunity && !Player.HasCooldown(Cooldowns.TarragonImmunity.ID))
                tarraThrowingCrits++;

            if (xerocSet && xerocDmg <= 0 && Player.ownedProjectileCounts[ProjectileType<XerocFire>()] < 3 && Player.ownedProjectileCounts[ProjectileType<XerocBlast>()] < 3)
            {
                switch (Main.rand.Next(5))
                {
                    case 0:
                        // Exodus Rogue Stars: 80%
                        int starDamage = (int)(proj.damage * 0.8);
                        CalamityGlobalProjectile.SpawnOrb(proj, starDamage, ProjectileType<XerocStar>(), 800f, Main.rand.Next(15, 30));
                        xerocDmg += (int)(starDamage * 0.5);
                        break;

                    case 1:
                        // Exodus Rogue Orbs: 60%
                        int orbDamage = (int)(proj.damage * 0.6);
                        CalamityGlobalProjectile.SpawnOrb(proj, orbDamage, ProjectileType<XerocOrb>(), 800f, 30f);
                        xerocDmg += (int)(orbDamage * 0.5);
                        break;

                    case 2:
                        // Exodus Rogue Fire: 15%
                        int fireDamage = (int)(proj.damage * 0.15);
                        Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<XerocFire>(), fireDamage, 0f, proj.owner, 0f, 0f);
                        break;

                    case 3:
                        // Exodus Rogue Blast: 20%
                        int blastDamage = (int)(proj.damage * 0.2);
                        Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<XerocBlast>(), blastDamage, 0f, proj.owner, 0f, 0f);
                        break;

                    case 4:
                        // Exodus Rogue Bubble: 60%
                        int bubbleDamage = (int)(proj.damage * 0.6);
                        CalamityGlobalProjectile.SpawnOrb(proj, bubbleDamage, ProjectileType<XerocBubble>(), 800f, 15f);
                        xerocDmg += (int)(bubbleDamage * 0.5);
                        break;

                    default:
                        break;
                }
            }

            if (modProj.stealthStrike && rogueCrownCooldown <= 0 && modProj.stealthStrikeHitCount < 5)
            {
                bool spawnedFeathers = false;
                if (nanotech)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 source = new Vector2(position.X + Main.rand.Next(-201, 201), Main.screenPosition.Y - 600f - Main.rand.Next(50));
                        Vector2 velocity = (position - source) / 40f;
                        Projectile.NewProjectile(spawnSource, source, velocity, ProjectileType<NanoFlare>(), (int)(120 * Player.RogueDamage()), 3f, proj.owner);
                    }
                }
                else if (moonCrown)
                {
                    int lunarFlareDamage = (int)(MoonstoneCrown.BaseDamage * Player.RogueDamage());
                    float lunarFlareKB = 3f;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 source = new Vector2(position.X + Main.rand.Next(-201, 201), Main.screenPosition.Y - 600f - Main.rand.Next(50));
                        Vector2 velocity = (position - source) / 10f;
                        int flare = Projectile.NewProjectile(spawnSource, source, velocity, ProjectileID.LunarFlare, lunarFlareDamage, lunarFlareKB, proj.owner);
                        if (flare.WithinBounds(Main.maxProjectiles))
                            Main.projectile[flare].Calamity().forceTypeless = true;
                    }
                }
                else if (featherCrown)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 source = new Vector2(position.X + Main.rand.Next(-201, 201), Main.screenPosition.Y - 600f - Main.rand.Next(50));
                        float speedX = (position.X - source.X) / 30f;
                        float speedY = (position.Y - source.Y) * 8;
                        Vector2 velocity = new Vector2(speedX, speedY);
                        int featherDamage = (int)(15 * Player.RogueDamage());
                        int feather = Projectile.NewProjectile(spawnSource, source, velocity, ProjectileType<StickyFeather>(), featherDamage, 3f, proj.owner);
                        if (feather.WithinBounds(Main.maxProjectiles))
                            Main.projectile[feather].Calamity().forceTypeless = true;
                    }
                    spawnedFeathers = true;
                }
                rogueCrownCooldown = spawnedFeathers ? 15 : 60;
            }

            if (forbiddenCirclet && modProj.stealthStrike && forbiddenCooldown <= 0 && modProj.stealthStrikeHitCount < 5)
            {
                for (int index2 = 0; index2 < 6; index2++)
                {
                    float xVector = Main.rand.Next(-35, 36) * 0.02f;
                    float yVector = Main.rand.Next(-35, 36) * 0.02f;
                    xVector *= 10f;
                    yVector *= 10f;
                    int eater = Projectile.NewProjectile(spawnSource, proj.Center.X, proj.Center.Y, xVector, yVector, ProjectileType<ForbiddenCircletEater>(), (int)(40 * Player.RogueDamage()), proj.knockBack, proj.owner);
                    if (eater.WithinBounds(Main.maxProjectiles))
                        Main.projectile[eater].Calamity().forceTypeless = true;
                    forbiddenCooldown = 15;
                }
            }

            if (titanHeartSet && modProj.stealthStrike && titanCooldown <= 0 && modProj.stealthStrikeHitCount < 5)
            {
                Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<SabatonBoom>(), (int)(50 * Player.RogueDamage()), proj.knockBack, proj.owner, 1f, 0f);
                SoundEngine.PlaySound(SoundID.Item14, proj.Center);
                for (int dustexplode = 0; dustexplode < 360; dustexplode++)
                {
                    Vector2 dustd = new Vector2(17f, 17f).RotatedBy(MathHelper.ToRadians(dustexplode));
                    int d = Dust.NewDust(proj.Center, proj.width, proj.height, Main.rand.NextBool(2) ? DustType<AstralBlue>() : DustType<AstralOrange>(), dustd.X, dustd.Y, 100, default, 1f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].position = proj.Center;
                    Main.dust[d].velocity *= 0.1f;
                }
                titanCooldown = 15;
            }

            if (corrosiveSpine && modProj.stealthStrikeHitCount < 5)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Main.rand.NextBool(2))
                    {
                        int type = -1;
                        switch (Main.rand.Next(15))
                        {
                            case 0:
                                type = ProjectileType<Corrocloud1>();
                                break;
                            case 1:
                                type = ProjectileType<Corrocloud2>();
                                break;
                            case 2:
                                type = ProjectileType<Corrocloud3>();
                                break;
                        }
                        // Should never happen, but just in case-
                        if (type != -1)
                        {
                            float speed = Main.rand.NextFloat(5f, 11f);
                            int cloud = Projectile.NewProjectile(spawnSource, position, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * speed, type, (int)(30 * Player.RogueDamage()), proj.knockBack, Player.whoAmI);
                            if (cloud.WithinBounds(Main.maxProjectiles))
                                Main.projectile[cloud].Calamity().forceTypeless = true;
                        }
                    }
                }
            }

            if (shadow && shadowPotCooldown <= 0 && modProj.stealthStrikeHitCount < 5)
            {
                if (CalamityLists.javelinProjList.Contains(proj.type))
                {
                    int randrot = Main.rand.Next(-30, 391);
                    Vector2 SoulSpeed = new Vector2(13f, 13f).RotatedBy(MathHelper.ToRadians(randrot));
                    int soul = Projectile.NewProjectile(spawnSource, proj.Center, SoulSpeed, ProjectileType<PenumbraSoul>(), (int)(proj.damage * 0.1), 3f, proj.owner, 0f, 0f);
                    if (soul.WithinBounds(Main.maxProjectiles))
                        Main.projectile[soul].Calamity().forceTypeless = true;
                    shadowPotCooldown = 30;
                }
                if (CalamityLists.spikyBallProjList.Contains(proj.type))
                {
                    int scythe = Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<CosmicScythe>(), (int)(proj.damage * 0.05), 3f, proj.owner, 1f, 0f);
                    if (scythe.WithinBounds(Main.maxProjectiles))
                        Main.projectile[scythe].Calamity().forceTypeless = true;
                    Main.projectile[scythe].usesLocalNPCImmunity = true;
                    Main.projectile[scythe].localNPCHitCooldown = 10;
                    Main.projectile[scythe].penetrate = 2;
                    shadowPotCooldown = 30;
                }
                if (CalamityLists.daggerProjList.Contains(proj.type))
                {
                    Vector2 shardVelocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                    shardVelocity.Normalize();
                    shardVelocity *= 5f;
                    int shard = Projectile.NewProjectile(spawnSource, proj.Center, shardVelocity, ProjectileType<EquanimityDarkShard>(), (int)(proj.damage * 0.15), 0f, proj.owner);
                    if (shard.WithinBounds(Main.maxProjectiles))
                        Main.projectile[shard].Calamity().forceTypeless = true;
                    Main.projectile[shard].timeLeft = 150;
                    shadowPotCooldown = 30;
                }
                if (CalamityLists.boomerangProjList.Contains(proj.type))
                {
                    int spiritDamage = (int)(proj.damage * 0.2);
                    Projectile ghost = CalamityGlobalProjectile.SpawnOrb(proj, spiritDamage, ProjectileID.SpectreWrath, 800f, 4f);
                    if (ghost.whoAmI.WithinBounds(Main.maxProjectiles))
                    {
                        ghost.Calamity().forceTypeless = true;
                        ghost.penetrate = 1;
                    }
                    shadowPotCooldown = 30;
                }
                if (CalamityLists.flaskBombProjList.Contains(proj.type))
                {
                    int blackhole = Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<ShadowBlackhole>(), (int)(proj.damage * 0.05), 3f, proj.owner, 0f, 0f);
                    if (blackhole.WithinBounds(Main.maxProjectiles))
                        Main.projectile[blackhole].Calamity().forceTypeless = true;
                    Main.projectile[blackhole].Center = proj.Center;
                    shadowPotCooldown = 30;
                }
            }

            if (npcCheck)
            {
                // Umbraphile cannot trigger off of itself. It is guaranteed on stealth strikes and 25% chance otherwise.
                if (umbraphileSet && ((modProj.stealthStrike && modProj.stealthStrikeHitCount < 5) || Main.rand.NextBool(4)))
                {
                    // Umbraphile Rogue Blasts: 25%, softcap starts at 200 base damage
                    int umbraBlastDamage = CalamityUtils.DamageSoftCap(proj.damage * 0.25, 50);
                    Projectile.NewProjectile(spawnSource, proj.Center, Vector2.Zero, ProjectileType<UmbraphileBoom>(), umbraBlastDamage, 0f, Player.whoAmI);
                }

                if (raiderTalisman && raiderStack < 150 && crit && raiderCooldown <= 0)
                {
                    raiderStack++;
                    raiderCooldown = 30;
                }
                if (electricianGlove && modProj.stealthStrike && modProj.stealthStrikeHitCount < 5)
                {
                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 velocity = CalamityUtils.RandomVelocity(50f, 30f, 60f);
                        int spark = Projectile.NewProjectile(spawnSource, position, velocity, ProjectileType<Spark>(), (int)(20 * Player.RogueDamage()), 0f, Player.whoAmI);
                        if (spark.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[spark].Calamity().forceTypeless = true;
                            Main.projectile[spark].localNPCHitCooldown = -1;
                        }
                    }
                }
            }
            modProj.stealthStrikeHitCount++;
        }
        #endregion
        #endregion

        #region Debuffs
        public void NPCDebuffs(NPC target, bool melee, bool ranged, bool magic, bool summon, bool rogue, bool proj)
        {
            if (melee) //prevents Deep Sea Dumbell from snagging true melee debuff memes
            {
                if (eGauntlet)
                {
                    int duration = 300;
                    target.AddBuff(BuffID.OnFire, duration, false);
                    target.AddBuff(BuffID.Frostburn, duration, false);
                    target.AddBuff(BuffType<HolyFlames>(), duration, false);
                }
                if (cryogenSoul || frostFlare)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffID.Frostburn);
                }
                if (yInsignia)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffType<HolyFlames>());
                }
                if (ataxiaFire)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffID.OnFire, 4f);
                }
                if (aWeapon)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffType<AbyssalFlames>());
                }
            }
            if (armorCrumbling || armorShattering)
            {
                if (melee || rogue)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffType<ArmorCrunch>());
                }
            }
            if (rogue)
            {
                switch (Player.meleeEnchant)
                {
                    case 1:
                        target.AddBuff(BuffID.Venom, 60 * Main.rand.Next(5, 10), false);
                        break;
                    case 2:
                        target.AddBuff(BuffID.CursedInferno, 60 * Main.rand.Next(3, 7), false);
                        break;
                    case 3:
                        target.AddBuff(BuffID.OnFire, 60 * Main.rand.Next(3, 7), false);
                        break;
                    case 5:
                        target.AddBuff(BuffID.Ichor, 60 * Main.rand.Next(10, 20), false);
                        break;
                    case 6:
                        target.AddBuff(BuffID.Confused, 60 * Main.rand.Next(1, 4), false);
                        break;
                    case 8:
                        target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 10), false);
                        break;
                    case 4:
                        target.AddBuff(BuffID.Midas, 120, false);
                        break;
                }
                if (titanHeartMask)
                {
                    target.AddBuff(BuffType<AstralInfectionDebuff>(), 120);
                }
                if (corrosiveSpine)
                {
                    target.AddBuff(BuffID.Venom, 240);
                }
                if (aWeapon)
                {
                    CalamityUtils.Inflict246DebuffsNPC(target, BuffType<AbyssalFlames>());
                }
            }
            if (summon)
            {
                if (pArtifact && !profanedCrystal)
                {
                    target.AddBuff(BuffType<HolyFlames>(), 300);
                }
                if (profanedCrystalBuffs)
                {
                    target.AddBuff(Main.dayTime ? BuffType<HolyFlames>() : BuffType<Nightwither>(), 600);
                }
                if (divineBless)
                {
                    target.AddBuff(BuffType<BanishingFire>(), 60);
                }

                if (holyMinions)
                {
                    target.AddBuff(BuffType<HolyFlames>(), 180);
                }

                if (shadowMinions)
                {
                    target.AddBuff(BuffID.ShadowFlame, 180);
                }

                if (voltaicJelly)
                {
                    //100% chance for Star Tainted Generator or Nucleogenesis
                    //20% chance for Voltaic Jelly
                    if (Main.rand.NextBool(starTaintedGenerator ? 1 : 5))
                    {
                        target.AddBuff(BuffID.Electrified, 60);
                    }
                }

                if (starTaintedGenerator)
                {
                    target.AddBuff(BuffType<AstralInfectionDebuff>(), 180);
                    target.AddBuff(BuffType<Irradiated>(), 180);
                }
            }
            if (omegaBlueChestplate)
                target.AddBuff(BuffType<CrushDepth>(), 180);
            if (sulfurSet)
                target.AddBuff(BuffID.Poisoned, 120);
            if (abyssalAmulet)
            {
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<CrushDepth>());
            }
            if (dsSetBonus)
            {
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<DemonFlames>());
            }
            if (alchFlask)
            {
                CalamityUtils.Inflict246DebuffsNPC(target, BuffType<Plague>());
            }
            if (holyWrath)
            {
                target.AddBuff(BuffType<HolyFlames>(), 180, false);
            }
            if (vexation)
            {
                if ((Player.armor[0].type == ItemType<ReaverHelm>() || Player.armor[0].type == ItemType<ReaverHeadgear>() ||
                    Player.armor[0].type == ItemType<ReaverVisage>()) && Player.armor[1].type == ItemType<ReaverScaleMail>() &&
                    Player.armor[2].type == ItemType<ReaverCuisses>())
                {
                    target.AddBuff(BuffID.CursedInferno, 90, false);
                    target.AddBuff(BuffID.Venom, 120, false);
                }
            }
        }

        public void PvpDebuffs(Player target, bool melee, bool ranged, bool magic, bool summon, bool rogue, bool proj)
        {
            if (melee)
            {
                if (eGauntlet)
                {
                    int duration = 300;
                    target.AddBuff(BuffID.OnFire, duration, false);
                    target.AddBuff(BuffID.Frostburn, duration, false);
                    target.AddBuff(BuffType<HolyFlames>(), duration, false);
                }
                if (aWeapon)
                {
                    CalamityUtils.Inflict246DebuffsPvp(target, BuffType<AbyssalFlames>());
                }
                if (cryogenSoul || frostFlare)
                {
                    CalamityUtils.Inflict246DebuffsPvp(target, BuffID.Frostburn);
                }
                if (yInsignia)
                {
                    CalamityUtils.Inflict246DebuffsPvp(target, BuffType<HolyFlames>());
                }
                if (ataxiaFire)
                {
                    CalamityUtils.Inflict246DebuffsPvp(target, BuffID.OnFire, 4f);
                }
            }
            if (armorCrumbling || armorShattering)
            {
                if (melee || rogue)
                {
                    CalamityUtils.Inflict246DebuffsPvp(target, BuffType<ArmorCrunch>());
                }
            }
            if (rogue)
            {
                switch (Player.meleeEnchant)
                {
                    case 1:
                        target.AddBuff(BuffID.Venom, 60 * Main.rand.Next(5, 10), false);
                        break;
                    case 2:
                        target.AddBuff(BuffID.CursedInferno, 60 * Main.rand.Next(3, 7), false);
                        break;
                    case 3:
                        target.AddBuff(BuffID.OnFire, 60 * Main.rand.Next(3, 7), false);
                        break;
                    case 5:
                        target.AddBuff(BuffID.Ichor, 60 * Main.rand.Next(10, 20), false);
                        break;
                    case 6:
                        target.AddBuff(BuffID.Confused, 60 * Main.rand.Next(1, 4), false);
                        break;
                    case 8:
                        target.AddBuff(BuffID.Poisoned, 60 * Main.rand.Next(5, 10), false);
                        break;
                }
                if (titanHeartMask)
                {
                    target.AddBuff(BuffType<AstralInfectionDebuff>(), 120);
                }
                if (corrosiveSpine)
                {
                    target.AddBuff(BuffID.Venom, 240);
                }
                if (aWeapon)
                {
                    CalamityUtils.Inflict246DebuffsPvp(target, BuffType<AbyssalFlames>());
                }
            }
            if (summon)
            {
                if (pArtifact && !profanedCrystal)
                {
                    target.AddBuff(BuffType<HolyFlames>(), 300);
                }
                if (profanedCrystalBuffs)
                {
                    target.AddBuff(Main.dayTime ? BuffType<HolyFlames>() : BuffType<Nightwither>(), 600);
                }

                if (holyMinions)
                {
                    target.AddBuff(BuffType<HolyFlames>(), 180);
                }

                if (shadowMinions)
                {
                    target.AddBuff(BuffID.ShadowFlame, 180);
                }

                if (voltaicJelly)
                {
                    //100% chance for Star Tainted Generator or Nucleogenesis
                    //20% chance for Voltaic Jelly
                    if (Main.rand.NextBool(starTaintedGenerator ? 1 : 5))
                    {
                        target.AddBuff(BuffID.Electrified, 60);
                    }
                }

                if (starTaintedGenerator)
                {
                    target.AddBuff(BuffType<AstralInfectionDebuff>(), 180);
                    target.AddBuff(BuffType<Irradiated>(), 180);
                }
            }
            if (omegaBlueChestplate)
                target.AddBuff(BuffType<CrushDepth>(), 180);
            if (sulfurSet)
                target.AddBuff(BuffID.Poisoned, 120);
            if (alchFlask)
            {
                CalamityUtils.Inflict246DebuffsPvp(target, BuffType<Plague>());
            }
            if (abyssalAmulet)
            {
                CalamityUtils.Inflict246DebuffsPvp(target, BuffType<CrushDepth>());
            }
            if (holyWrath)
            {
                target.AddBuff(BuffType<HolyFlames>(), 180, false);
            }
            if (vexation)
            {
                if ((Player.armor[0].type == ItemType<ReaverHelm>() || Player.armor[0].type == ItemType<ReaverHeadgear>() ||
                    Player.armor[0].type == ItemType<ReaverVisage>()) && Player.armor[1].type == ItemType<ReaverScaleMail>() &&
                    Player.armor[2].type == ItemType<ReaverCuisses>())
                {
                    target.AddBuff(BuffID.CursedInferno, 90, false);
                    target.AddBuff(BuffID.Venom, 120, false);
                }
            }
        }
        #endregion

        #region Lifesteal
        public void ProjLifesteal(NPC target, Projectile proj, int damage, bool crit)
        {
            CalamityGlobalProjectile modProj = proj.Calamity();

            // Spectre Damage set and Nebula set work on enemies which are "immune to lifesteal"
            if (!target.canGhostHeal)
            {
                if (Player.ghostHurt)
                {
                    proj.ghostHurt(damage, target.Center, target);
                }

                if (Player.setNebula && Player.nebulaCD == 0 && Main.rand.NextBool(3))
                {
                    Player.nebulaCD = 30;
                    int boosterType = Utils.SelectRandom(Main.rand, new int[]
                    {
                        ItemID.NebulaPickup1,
                        ItemID.NebulaPickup2,
                        ItemID.NebulaPickup3
                    });
                    var source = Player.GetItemSource_Misc(ItemSourceID.SetBonus_Nebula);
                    int nebulaBooster = Item.NewItem(source, target.Center, target.Size, boosterType, 1, false, 0, false, false);
                    Main.item[nebulaBooster].velocity.Y = Main.rand.Next(-20, 1) * 0.2f;
                    Main.item[nebulaBooster].velocity.X = Main.rand.Next(10, 31) * 0.2f * proj.direction;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, nebulaBooster, 0f, 0f, 0f, 0, 0, 0);
                    }
                }
            }

            if (bloodflareSet && !target.SpawnedFromStatue && (target.damage > 0 || target.boss) && !Player.moonLeech)
            {
                if ((target.life < target.lifeMax * 0.5) && bloodflareHeartTimer <= 0)
                {
                    bloodflareHeartTimer = 300;
                    DropHelper.DropItem(target, ItemID.Heart);
                }
            }

            if (Main.player[Main.myPlayer].lifeSteal > 0f && target.canGhostHeal && !Player.moonLeech)
            {
                // Increases the degree to which Spectre Healing set contributes to the lifesteal cap
                if (Player.ghostHeal)
                {
                    float cooldownMult = 0.2f;
                    cooldownMult -= proj.numHits * 0.05f;
                    if (cooldownMult < 0f)
                        cooldownMult = 0f;

                    float cooldown = damage * cooldownMult;
                    Main.player[Main.myPlayer].lifeSteal -= cooldown;
                }

                // Increases the degree to which Vampire Knives contribute to the lifesteal cap
                if (proj.type == ProjectileID.VampireKnife)
                {
                    float cooldown = damage * 0.075f;
                    if (cooldown < 0f)
                        cooldown = 0f;

                    Main.player[Main.myPlayer].lifeSteal -= cooldown;
                }

                if (vampiricTalisman && modProj.rogue && crit)
                {
                    float heal = MathHelper.Clamp(damage * 0.011f, 0f, 5f);
                    if ((int)heal > 0)
                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileID.VampireHeal, 1200f, 3f);
                }

                if (bloodyGlove && modProj.rogue && modProj.stealthStrike)
                {
                    Player.statLife += 1;
                    Player.HealEffect(1);
                }

                if ((target.damage > 5 || target.boss) && !target.SpawnedFromStatue)
                {
                    if (bloodflareThrowing && modProj.rogue && crit && Main.rand.NextBool(2))
                    {
                        float projHitMult = 0.03f;
                        projHitMult -= proj.numHits * 0.015f;
                        if (projHitMult < 0f)
                            projHitMult = 0f;

                        float cooldownMult = damage * projHitMult;
                        if (cooldownMult < 0f)
                            cooldownMult = 0f;

                        if (Player.lifeSteal > 0f)
                        {
                            Player.statLife += 1;
                            Player.HealEffect(1);
                            Player.lifeSteal -= cooldownMult * 2f;
                        }
                    }

                    if (bloodflareMelee && modProj.trueMelee)
                    {
                        int healAmount = Main.rand.Next(2) + 1;
                        Player.statLife += healAmount;
                        Player.HealEffect(healAmount);
                    }
                }

                bool otherHealTypes = auricSet || silvaSet || tarraMage || ataxiaMage;

                if (proj.CountsAsClass(DamageClass.Magic) && Player.ActiveItem().CountsAsClass<MagicDamageClass>())
                {
                    if (manaOverloader && otherHealTypes)
                    {
                        if (Main.rand.NextBool(2))
                        {
                            float healMult = 0.2f;
                            healMult -= proj.numHits * 0.05f;
                            float heal = damage * healMult * (Player.statMana / (float)Player.statManaMax2);

                            if (heal > CalamityMod.lifeStealCap)
                                heal = CalamityMod.lifeStealCap;

                            if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                                return;

                            CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<ManaOverloaderHealOrb>(), 1200f, 3f);
                        }
                    }
                }

                if (auricSet)
                {
                    float healMult = 0.05f;
                    healMult -= proj.numHits * 0.025f;
                    float heal = damage * healMult;

                    if (heal > CalamityMod.lifeStealCap)
                        heal = CalamityMod.lifeStealCap;

                    if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                        return;

                    CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<AuricOrb>(), 1200f, 3f);
                }
                else if (silvaSet)
                {
                    float healMult = 0.03f;
                    healMult -= proj.numHits * 0.015f;
                    float heal = damage * healMult;

                    if (heal > CalamityMod.lifeStealCap)
                        heal = CalamityMod.lifeStealCap;

                    if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                        return;

                    CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<SilvaOrb>(), 1200f, 3f);
                }
                else if (proj.CountsAsClass(DamageClass.Magic) && Player.ActiveItem().CountsAsClass<MagicDamageClass>())
                {
                    if (manaOverloader)
                    {
                        float healMult = 0.2f;
                        healMult -= proj.numHits * 0.05f;
                        float heal = damage * healMult * (Player.statMana / (float)Player.statManaMax2);

                        if (heal > CalamityMod.lifeStealCap)
                            heal = CalamityMod.lifeStealCap;

                        if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            return;

                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<ManaOverloaderHealOrb>(), 1200f, 3f);
                    }

                    if (tarraMage)
                    {
                        if (tarraMageHealCooldown <= 0)
                        {
                            tarraMageHealCooldown = 90;

                            float healMult = 0.1f;
                            healMult -= proj.numHits * 0.05f;
                            float heal = damage * healMult;

                            if (heal > CalamityMod.lifeStealCap)
                                heal = CalamityMod.lifeStealCap;

                            if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                                return;

                            Main.player[Main.myPlayer].lifeSteal -= heal * 6f;

                            int healAmount = (int)heal;
                            Player.statLife += healAmount;
                            Player.HealEffect(healAmount);

                            if (Player.statLife > Player.statLifeMax2)
                                Player.statLife = Player.statLifeMax2;
                        }
                    }
                    else if (ataxiaMage)
                    {
                        float healMult = 0.1f;
                        healMult -= proj.numHits * 0.05f;
                        float heal = damage * healMult;

                        if (heal > CalamityMod.lifeStealCap)
                            heal = CalamityMod.lifeStealCap;

                        if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            return;

                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<AtaxiaHealOrb>(), 1200f, 3f);
                    }
                }

                if (reaverDefense)
                {
                    float healMult = 0.2f;
                    healMult -= proj.numHits * 0.05f;
                    float heal = damage * healMult;

                    if (heal > CalamityMod.lifeStealCap)
                        heal = CalamityMod.lifeStealCap;
                    if (Main.rand.Next(10) > 0)
                        heal = 0;

                    if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                        return;

                    CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<ReaverHealOrb>(), 1200f, 3f);
                }

                if (modProj.rogue)
                {
                    if (xerocSet && xerocDmg <= 0 && Player.ownedProjectileCounts[ProjectileType<XerocFire>()] < 3 && Player.ownedProjectileCounts[ProjectileType<XerocBlast>()] < 3)
                    {
                        float healMult = 0.06f;
                        healMult -= proj.numHits * 0.015f;
                        float heal = damage * healMult;

                        if (heal > CalamityMod.lifeStealCap)
                            heal = CalamityMod.lifeStealCap;

                        if (!CalamityGlobalProjectile.CanSpawnLifeStealProjectile(healMult, heal))
                            return;

                        CalamityGlobalProjectile.SpawnLifeStealProjectile(proj, Player, heal, ProjectileType<XerocHealOrb>(), 1200f, 3f);
                    }
                }
            }
        }

        public void ItemLifesteal(NPC target, Item item, int damage)
        {
            if (bloodflareSet && !target.SpawnedFromStatue && (target.damage > 0 || target.boss))
            {
                if ((target.life < target.lifeMax * 0.5) && bloodflareHeartTimer <= 0)
                {
                    bloodflareHeartTimer = 300;
                    DropHelper.DropItem(target, ItemID.Heart);
                }
            }

            if ((target.damage > 5 || target.boss) && !target.SpawnedFromStatue && target.canGhostHeal && !Player.moonLeech)
            {
                if (bloodflareMelee && item.CountsAsClass<MeleeDamageClass>())
                {
                    int healAmount = Main.rand.Next(2) + 1;
                    Player.statLife += healAmount;
                    Player.HealEffect(healAmount);
                }
            }

            if (reaverDefense)
            {
                if (Main.player[Main.myPlayer].lifeSteal > 0f && target.canGhostHeal && !Player.moonLeech)
                {
                    float healMult = 0.2f;
                    float heal = damage * healMult;

                    if (heal > CalamityMod.lifeStealCap)
                        heal = CalamityMod.lifeStealCap;
                    if (Main.rand.Next(10) > 0)
                        heal = 0;

                    if ((int)heal > 0 && !Main.player[Main.myPlayer].moonLeech)
                    {
                        Main.player[Main.myPlayer].lifeSteal -= heal * 3f;

                        float lowestHealthCheck = 0f;
                        int healTarget = Player.whoAmI;
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player otherPlayer = Main.player[i];
                            if (otherPlayer.active && !otherPlayer.dead && ((!Player.hostile && !otherPlayer.hostile) || Player.team == otherPlayer.team))
                            {
                                float playerDist = Vector2.Distance(target.Center, otherPlayer.Center);
                                if (playerDist < 1200f && (otherPlayer.statLifeMax2 - otherPlayer.statLife) > lowestHealthCheck)
                                {
                                    lowestHealthCheck = otherPlayer.statLifeMax2 - otherPlayer.statLife;
                                    healTarget = i;
                                }
                            }
                        }

                        var source = new ProjectileSource_ReaverHeal(Player);
                        Projectile.NewProjectile(source, target.Center, Vector2.Zero, ProjectileType<ReaverHealOrb>(), 0, 0f, Player.whoAmI, healTarget, heal);
                    }
                }
            }
        }
        #endregion

        #region The Horseman's Blade
        public static void HorsemansBladeOnHit(Player player, int targetIdx, int damage, float knockback, int extraUpdateAmt = 0, int type = ProjectileID.FlamingJack)
        {
            int logicCheckScreenHeight = Main.LogicCheckScreenHeight;
            int logicCheckScreenWidth = Main.LogicCheckScreenWidth;
            int x = Main.rand.Next(100, 300);
            int y = Main.rand.Next(100, 300);
            switch (Main.rand.Next(4))
            {
                case 0:
                    x -= logicCheckScreenWidth / 2 + x;
                    break;
                case 1:
                    x += logicCheckScreenWidth / 2 - x;
                    break;
                case 2:
                    y -= logicCheckScreenHeight / 2 + y;
                    break;
                case 3:
                    y += logicCheckScreenHeight / 2 - y;
                    break;
                default:
                    break;
            }
            x += (int)player.position.X;
            y += (int)player.position.Y;
            float speed = 8f;
            Vector2 spawnPos = new Vector2(x, y);
            Vector2 velocity = Main.npc[targetIdx].DirectionFrom(spawnPos);
            velocity *= speed;

            var source = player.GetProjectileSource_Item(player.ActiveItem());
            int projectile = Projectile.NewProjectile(source, spawnPos, velocity, type, damage, knockback, player.whoAmI, targetIdx, 0f);
            Main.projectile[projectile].extraUpdates += extraUpdateAmt;
        }
        #endregion
    }
}
