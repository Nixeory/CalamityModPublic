﻿using CalamityMod.Dusts.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureOtherworldly
{
    [LegacyName("OccultDresser")]
    public class OtherworldlyDresser : ModTile
    {
        public override void SetStaticDefaults()
        {
            this.SetUpDresser();
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Otherworldly Dresser");
            AddMapEntry(new Color(191, 142, 111), name);
            TileID.Sets.DisableSmartCursor[Type] = true;
            AdjTiles = new int[] { TileID.Dressers };
            ContainerName.SetDefault("Otherworldly Dresser");
            DresserDrop = ModContent.ItemType<Items.Placeables.FurnitureOtherworldly.OtherworldlyDresser>();
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 1, 0f, 0f, 1, new Color(125, 94, 128), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<OtherworldlyTileCloth>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override bool RightClick(int i, int j)
        {
            return CalamityUtils.DresserRightClick();
        }

        public override void MouseOverFar(int i, int j)
        {
            CalamityUtils.DresserMouseFar<Items.Placeables.FurnitureOtherworldly.OtherworldlyDresser>(ContainerName.GetDefault());
        }

        public override void MouseOver(int i, int j)
        {
            CalamityUtils.DresserMouseOver<Items.Placeables.FurnitureOtherworldly.OtherworldlyDresser>(ContainerName.GetDefault());
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 32, DresserDrop);
            Chest.DestroyChest(i, j);
        }
    }
}