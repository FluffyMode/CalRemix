﻿using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Items.Accessories;
using CalamityMod.Projectiles.Typeless;
using CalamityMod;
using CalamityMod.Rarities;
using CalamityMod.Items;
using CalamityMod;
using CalRemix.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalRemix.Items.Accessories
{
    public class MagnaCore : ModItem
    {
        public override string Texture => "CalamityMod/Items/Accessories/EtherealTalisman";
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Magna Core");
            Tooltip.SetDefault("'Most drink the wine to lose focus, while others transcend...\n" +
            "30% increased ranged damage, 25% increased ranged critical strike chance, and 100% reduced ammo usage\n" +
            "15 increased defense, 2 increased life regen, and 35% increased pick speed\n" +
            "You are surrounded by 4 demonic portals that buff ranged projectiles that intersect them\n" +
            "Skull targets occasionally appear on enemies which increase damage taken when hit");
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 36;
            Item.value = CalamityGlobalItem.Rarity15BuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.eTalisman = true;
            player.GetModPlayer<CalRemixPlayer>().marnite = true;

            player.manaMagnet = true;

            player.statManaMax2 += 250;
            player.GetDamage<MagicDamageClass>() += 0.3f;
            player.manaCost *= 0f;
            player.GetCritChance<MagicDamageClass>() += 25;


            if (Main.LocalPlayer.HasBuff(BuffID.ManaSickness))
            {
                if (!Main.LocalPlayer.HasCooldown(MagnaCoreCooldown.ID) && player.active)
                {
                    float variance = MathHelper.TwoPi / 8;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 velocity = new Vector2(0f, 10f);
                        velocity = velocity.RotatedBy(variance * i);
                        int p = Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center, velocity, ModContent.ProjectileType<MarniteOrb>(), 20000, 0, player.whoAmI);
                        if (Main.projectile.IndexInRange(p))
                            Main.projectile[p].originalDamage = 20000;
                    }
                    Main.LocalPlayer.AddCooldown(MagnaCoreCooldown.ID, CalamityUtils.SecondsToFrames(20));
                }
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<EtherealTalisman>(1).
                AddIngredient<UnstableGraniteCore>(1).
                AddIngredient<SupremeManaPotion>(20).
                AddIngredient(ItemID.GraniteBlock, 50).
                AddIngredient(ItemID.MarbleBlock, 50).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
