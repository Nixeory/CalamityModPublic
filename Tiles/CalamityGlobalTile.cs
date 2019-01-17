using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.NPCs;

namespace CalamityMod.Tiles
{
    public class CalamityGlobalTile : GlobalTile
    {
        public static ushort[] PlantTypes = new ushort[]
        {
            TileID.Plants,
            TileID.CorruptPlants,
            TileID.JunglePlants,
            TileID.MushroomPlants,
            TileID.Plants2,
            TileID.JunglePlants2,
            TileID.HallowedPlants,
            TileID.HallowedPlants2,
            TileID.FleshWeeds,
            (ushort)CalamityMod.Instance.TileType("AstralShortPlants"),
            (ushort)CalamityMod.Instance.TileType("AstralTallPlants")
        };

        public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
        {
            for (int k = 0; k < PlantTypes.Length; k++)
            {
                if (PlantTypes[k] == type)
                {
                    CustomTileFraming.CheckPlants(i, j);
                    return false;
                }
            }
            if (type == TileID.Vines || type == TileID.CrimsonVines || type == TileID.HallowedVines || type == mod.TileType("AstralVines"))
            {
                CustomTileFraming.VineFrame(i, j);
                return false;
            }
            return base.TileFrame(i, j, type, ref resetFrame, ref noBreak);
        }

        public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            if (type == TileID.Cactus)
            {
                //GRABBING VARIABLES FOR CERTAIN THINGS
                Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                if (Main.drawToScreen)
                    zero = Vector2.Zero;
                //TESTING STUFF
                int frameX = tile.frameX;
                int frameY = tile.frameY;
                bool astralCactus = false;
                if (!Main.canDrawColorTile(i, j))
                {
                    int xTile = i;
                    if (frameX == 36)
                    {
                        xTile--;
                    }
                    if (frameX == 54)
                    {
                        xTile++;
                    }
                    if (frameX == 108)
                    {
                        if (frameY == 18)
                        {
                            xTile--;
                        }
                        else
                        {
                            xTile++;
                        }
                    }
                    int yTile = j;
                    bool flag = false;
                    if (Main.tile[xTile, yTile].type == 80 && Main.tile[xTile, yTile].active())
                    {
                        flag = true;
                    }
                    while (!Main.tile[xTile, yTile].active() || !Main.tileSolid[(int)Main.tile[xTile, yTile].type] || !flag)
                    {
                        if (Main.tile[xTile, yTile].type == 80 && Main.tile[xTile, yTile].active())
                        {
                            flag = true;
                        }
                        yTile++;
                        if (yTile > i + 20)
                        {
                            break;
                        }
                    }
                    //CACTUS CHECK
                    if (Main.tile[xTile, yTile].type == (ushort)mod.TileType("AstralSand"))
                    {
                        astralCactus = true;
                    }
                }
                //Draw Glow
                if (astralCactus)
                {
                    spriteBatch.Draw(CalamityMod.AstralCactusGlowTexture, new Vector2((float)(i * 16 - (int)Main.screenPosition.X), (float)(j * 16 - (int)Main.screenPosition.Y)) + zero, new Rectangle((int)frameX, (int)frameY, 16, 18), Color.White * 0.75f, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
                }
                return;
            }
        }

        public override bool Drop(int i, int j, int type)
        {
            if (type == 28)
            {
                int x = Main.maxTilesX;
                int y = Main.maxTilesY;
                int genLimit = x / 2;
                int abyssChasmSteps = y / 4;
                int abyssChasmY = ((y - abyssChasmSteps) + (int)((double)y * 0.055)); //132 = 1932 large
                if (y < 1500)
                {
                    abyssChasmY = ((y - abyssChasmSteps) + (int)((double)y * 0.095)); //114 = 1014 small
                }
                else if (y < 2100)
                {
                    abyssChasmY = ((y - abyssChasmSteps) + (int)((double)y * 0.0735)); //132 = 1482 medium
                }
                int abyssChasmX = (CalamityWorld.abyssSide ? genLimit - (genLimit - 135) : genLimit + (genLimit - 135));

                bool abyssPosX = false;
                bool sulphurPosX = false;
                bool abyssPosY = (j <= abyssChasmY);
                if (CalamityWorld.abyssSide)
                {
                    if (i < 380)
                    {
                        sulphurPosX = true;
                    }
                    if (i < abyssChasmX + 80)
                    {
                        abyssPosX = true;
                    }
                }
                else
                {
                    if (i > Main.maxTilesX - 380)
                    {
                        sulphurPosX = true;
                    }
                    if (i > abyssChasmX - 80)
                    {
                        abyssPosX = true;
                    }
                }
                if (abyssPosX && abyssPosY)
                {
                    if (Main.rand.Next(10) == 0)
                    {
                        int expr_B64 = WorldGen.genRand.Next(15);
                        if (expr_B64 == 0)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 296, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 1)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 294, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 2)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 298, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 3)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 302, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 4)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 288, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 5)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 305, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 6)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 289, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 7)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 302, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 8)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, mod.ItemType("AnechoicCoating"), 1, false, 0, false, false);
                        }
                        if (expr_B64 == 9)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 291, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 10)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2346, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 11)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2323, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 12)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2327, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 13)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2345, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 14)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2348, 1, false, 0, false, false);
                        }
                    }
                    else
                    {
                        int num9 = Main.rand.Next(10); //0 to 9
                        if (num9 == 0) //spelunker glowsticks
                        {
                            int num10 = Main.rand.Next(2, 6);
                            if (Main.expertMode)
                            {
                                num10 += Main.rand.Next(1, 7);
                            }
                            Item.NewItem(i * 16, j * 16, 16, 16, 3002, num10, false, 0, false, false);
                        }
                        else if (num9 == 1) //hellfire arrows
                        {
                            int stack = Main.rand.Next(10, 21);
                            Item.NewItem(i * 16, j * 16, 16, 16, 265, stack, false, 0, false, false);
                        }
                        else if (num9 == 2) //stew
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, mod.ItemType("SunkenStew"), 1, false, 0, false, false);
                        }
                        else if (num9 == 3) //sticky dynamite
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2896, 1, false, 0, false, false);
                        }
                        else //money
                        {
                            float num13 = (float)(5000 + WorldGen.genRand.Next(-100, 101));
                            while ((int)num13 > 0)
                            {
                                if (num13 > 1000000f)
                                {
                                    int num14 = (int)(num13 / 1000000f);
                                    if (num14 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num14 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num14 /= Main.rand.Next(3) + 1;
                                    }
                                    num13 -= (float)(1000000 * num14);
                                    Item.NewItem(i * 16, j * 16, 16, 16, 74, num14, false, 0, false, false);
                                }
                                else if (num13 > 10000f)
                                {
                                    int num15 = (int)(num13 / 10000f);
                                    if (num15 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num15 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num15 /= Main.rand.Next(3) + 1;
                                    }
                                    num13 -= (float)(10000 * num15);
                                    Item.NewItem(i * 16, j * 16, 16, 16, 73, num15, false, 0, false, false);
                                }
                                else if (num13 > 100f)
                                {
                                    int num16 = (int)(num13 / 100f);
                                    if (num16 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num16 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num16 /= Main.rand.Next(3) + 1;
                                    }
                                    num13 -= (float)(100 * num16);
                                    Item.NewItem(i * 16, j * 16, 16, 16, 72, num16, false, 0, false, false);
                                }
                                else
                                {
                                    int num17 = (int)num13;
                                    if (num17 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num17 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num17 /= Main.rand.Next(4) + 1;
                                    }
                                    if (num17 < 1)
                                    {
                                        num17 = 1;
                                    }
                                    num13 -= (float)num17;
                                    Item.NewItem(i * 16, j * 16, 16, 16, 71, num17, false, 0, false, false);
                                }
                            }
                        }
                    }
                }
                else if (sulphurPosX)
                {
                    if (Main.rand.Next(15) == 0)
                    {
                        int expr_B64 = WorldGen.genRand.Next(15);
                        if (expr_B64 == 0)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 296, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 1)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 294, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 2)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 298, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 3)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 302, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 4)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 288, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 5)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 305, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 6)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 289, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 7)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 302, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 8)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, mod.ItemType("AnechoicCoating"), 1, false, 0, false, false);
                        }
                        if (expr_B64 == 9)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 291, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 10)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2346, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 11)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2323, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 12)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2327, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 13)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2345, 1, false, 0, false, false);
                        }
                        if (expr_B64 == 14)
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 2348, 1, false, 0, false, false);
                        }
                    }
                    else
                    {
                        int num9 = Main.rand.Next(10); //0 to 9
                        if (num9 == 0) //glowsticks
                        {
                            int num10 = Main.rand.Next(2, 6);
                            if (Main.expertMode)
                            {
                                num10 += Main.rand.Next(1, 7);
                            }
                            Item.NewItem(i * 16, j * 16, 16, 16, 282, num10, false, 0, false, false);
                        }
                        else if (num9 == 1) //jesters arrows
                        {
                            int stack = Main.rand.Next(10, 21);
                            Item.NewItem(i * 16, j * 16, 16, 16, 51, stack, false, 0, false, false);
                        }
                        else if (num9 == 2) //potion
                        {
                            Item.NewItem(i * 16, j * 16, 16, 16, 188, 1, false, 0, false, false);
                        }
                        else if (num9 == 3) //bomb
                        {
                            int stack = Main.rand.Next(5, 9);
                            Item.NewItem(i * 16, j * 16, 16, 16, 166, 1, false, 0, false, false);
                        }
                        else //money
                        {
                            float num13 = (float)(2500 + WorldGen.genRand.Next(-100, 101));
                            while ((int)num13 > 0)
                            {
                                if (num13 > 1000000f)
                                {
                                    int num14 = (int)(num13 / 1000000f);
                                    if (num14 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num14 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num14 /= Main.rand.Next(3) + 1;
                                    }
                                    num13 -= (float)(1000000 * num14);
                                    Item.NewItem(i * 16, j * 16, 16, 16, 74, num14, false, 0, false, false);
                                }
                                else if (num13 > 10000f)
                                {
                                    int num15 = (int)(num13 / 10000f);
                                    if (num15 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num15 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num15 /= Main.rand.Next(3) + 1;
                                    }
                                    num13 -= (float)(10000 * num15);
                                    Item.NewItem(i * 16, j * 16, 16, 16, 73, num15, false, 0, false, false);
                                }
                                else if (num13 > 100f)
                                {
                                    int num16 = (int)(num13 / 100f);
                                    if (num16 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num16 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num16 /= Main.rand.Next(3) + 1;
                                    }
                                    num13 -= (float)(100 * num16);
                                    Item.NewItem(i * 16, j * 16, 16, 16, 72, num16, false, 0, false, false);
                                }
                                else
                                {
                                    int num17 = (int)num13;
                                    if (num17 > 50 && Main.rand.Next(2) == 0)
                                    {
                                        num17 /= Main.rand.Next(3) + 1;
                                    }
                                    if (Main.rand.Next(2) == 0)
                                    {
                                        num17 /= Main.rand.Next(4) + 1;
                                    }
                                    if (num17 < 1)
                                    {
                                        num17 = 1;
                                    }
                                    num13 -= (float)num17;
                                    Item.NewItem(i * 16, j * 16, 16, 16, 71, num17, false, 0, false, false);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public override bool PreHitWire(int i, int j, int type)
        {
            return !CalamityWorld.bossRushActive;
        }

        public override void RandomUpdate(int i, int j, int type)
        {
            if (Main.netMode != 1)
            {
                if (type == (ushort)mod.TileType("ArenaTile"))
                {
                    if (!NPC.AnyNPCs(mod.NPCType("SupremeCalamitas")))
                    {
                        for (int k = 0; k < Main.maxTilesX; k++)
                        {
                            for (int l = 0; l < Main.maxTilesY; l++)
                            {
                                if (Main.tile[k, l].type == (ushort)mod.TileType("ArenaTile"))
                                {
                                    Main.tile[k, l].active(false);
                                    WorldGen.SquareTileFrame(k, l);
                                }
                            }
                        }
                    }
                }
            }
            if (Main.hardMode && !NPC.downedPlantBoss && !NPC.downedGolemBoss && !NPC.downedAncientCultist && !NPC.downedMoonlord && !CalamityWorld.downedCalamitas)
            {
                if (j > (int)Main.worldSurface)
                {
                    if (Main.tile[i, j] != null)
                    {
                        if (Main.tile[i, j].nactive())
                        {
                            if (type == 60)
                            {
                                int j2 = j - 1;
                                if (Main.tile[i, j2] != null)
                                {
                                    if (Main.tile[i, j2].liquid == 0)
                                    {
                                        if (WorldGen.genRand.Next(3) == 0)
                                        {
                                            bool flag18 = true;
                                            int num80 = 400;
                                            for (int num81 = i - num80; num81 < i + num80; num81 += 2)
                                            {
                                                for (int num82 = j - num80; num82 < j + num80; num82 += 2)
                                                {
                                                    if (num81 > 1 && num81 < Main.maxTilesX - 2 && num82 > 1 && num82 < Main.maxTilesY - 2 && Main.tile[num81, num82].active() && Main.tile[num81, num82].type == 238)
                                                    {
                                                        flag18 = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (flag18)
                                            {
                                                WorldGen.PlaceJunglePlant(i, j2, 238, 0, 0);
                                                WorldGen.SquareTileFrame(i, j2, true);
                                                WorldGen.SquareTileFrame(i + 1, j2 + 1, true);
                                                if (Main.tile[i, j2].type == 238 && Main.netMode == 2)
                                                {
                                                    NetMessage.SendTileSquare(-1, i, j2, 4, TileChangeType.None);
                                                }
                                            }
                                        }
                                        if (NPC.downedMechBossAny && WorldGen.genRand.Next(6) == 0)
                                        {
                                            bool flag19 = true;
                                            int num83 = 150;
                                            for (int num84 = i - num83; num84 < i + num83; num84 += 2)
                                            {
                                                for (int num85 = j - num83; num85 < j + num83; num85 += 2)
                                                {
                                                    if (num84 > 1 && num84 < Main.maxTilesX - 2 && num85 > 1 && num85 < Main.maxTilesY - 2 && Main.tile[num84, num85].active() && Main.tile[num84, num85].type == 236)
                                                    {
                                                        flag19 = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (flag19)
                                            {
                                                WorldGen.PlaceJunglePlant(i, j2, 236, WorldGen.genRand.Next(3), 0);
                                                WorldGen.SquareTileFrame(i, j2, true);
                                                WorldGen.SquareTileFrame(i + 1, j2 + 1, true);
                                                if (Main.tile[i, j2].type == 236 && Main.netMode == 2)
                                                {
                                                    NetMessage.SendTileSquare(-1, i, j2, 4, TileChangeType.None);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
