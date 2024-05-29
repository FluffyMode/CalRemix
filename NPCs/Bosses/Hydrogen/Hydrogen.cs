﻿using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using CalamityMod.World;
using CalamityMod.Particles;
using CalRemix.Projectiles.Hostile;
using CalRemix.Items.Placeables;
using CalamityMod.Events;
using CalRemix.Biomes;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Materials;
using System;
using CalamityMod.Projectiles.Enemy;
using Newtonsoft.Json.Serialization;
using CalamityMod.Items.Placeables;
using System.Net.Http.Headers;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Tiles.Furniture.Monoliths;
using System.Collections.Generic;
using Terraria.Utilities;
using CalRemix.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace CalRemix.NPCs.Bosses.Hydrogen
{
    [AutoloadBossHead]
    public class Hydrogen : ModNPC
    {
        public ref float Phase => ref NPC.ai[0];

        public ref Player Target => ref Main.player[NPC.target];

        public Rectangle teleportPos = new Rectangle();

        public static readonly SoundStyle HitSound = new("CalRemix/Sounds/IonogenHit", 3);
        public static readonly SoundStyle DeathSound = new("CalRemix/Sounds/CarcinogenDeath");

        public enum PhaseType
        {
            Sealed = 0,
            Idle = 1,
            MissileLaunch = 2,
            Mines = 3,
            Death = 4
        }

        public override bool IsLoadingEnabled(Mod mod)
        {
            return true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hydrogen");
        }

        public override void SetDefaults()
        {
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.npcSlots = 24f;
            NPC.damage = 100;
            NPC.width = 82;
            NPC.height = 88;
            NPC.defense = 15;
            NPC.DR_NERD(0.3f);
            NPC.LifeMaxNERB(40000, 48000, 300000);
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 40, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.DeathSound = null;
            NPC.Calamity().VulnerableToWater = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot("CalRemix/Sounds/Music/AtomicReinforcement");
            }
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SulphurousSeaBiome>().Type };
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<HydrogenShield>(), ai0: NPC.whoAmI);
        }

        public override void AI()
        {
            NPC.TargetClosest();
            float lifeRatio = NPC.life / NPC.lifeMax;
            bool rev = CalamityWorld.revenge || BossRushEvent.BossRushActive;
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
            bool master = Main.masterMode || BossRushEvent.BossRushActive;
            bool expert = Main.expertMode || BossRushEvent.BossRushActive;
            /*if (NPC.life == NPC.lifeMax)
            {
                Phase = (int)PhaseType.Idle;
            }*/
            if (NPC.life <= 1)
            {
                NPC.ai[1] = 0;
                NPC.ai[2] = 0;
                NPC.ai[3] = 0;
                Phase = (int)PhaseType.Death;
            }
            else if (Target == null || Target.dead)
            {
                NPC.velocity.Y += 1;
                NPC.Calamity().newAI[3]++;
                if (NPC.Calamity().newAI[3] > 240)
                {
                    NPC.active = false;
                }
                return;
            }
            NPC.Calamity().newAI[3] = 0;
            switch (Phase)
            {
                case (int)PhaseType.Sealed:
                    {
                        NPC.damage = 0;
                        NPC.boss = false;
                        NPC.velocity = Vector2.Zero;
                        NPC.chaseable = false;
                        if (lifeRatio < 0.9f)
                        {
                            NPC.damage = 100;
                            NPC.boss = true;
                            NPC.chaseable = true;
                            Phase = (int)PhaseType.Idle;
                        }
                        break;
                    }
                case (int)PhaseType.Idle:
                    {
                        int phaseTime = 90;
                        NPC.ai[1]++;
                        NPC.velocity = NPC.DirectionTo(Target.Center) * 5;
                        if (NPC.ai[1] > phaseTime)
                        {
                            NPC.ai[1] = 0;
                            Phase = (int)PhaseType.MissileLaunch;
                        }
                        break;
                    }
                case (int)PhaseType.MissileLaunch:
                    {
                        int rocketRate = 5;
                        int fireDelay = 30;
                        int rocketAmt = 10;
                        int salvoAmount = 2;
                        float missileSpread = 45f;
                        NPC.ai[1]++;
                        NPC.velocity *= 0.95f;
                        if (NPC.ai[1] > fireDelay)
                        {
                            NPC.ai[2]++;
                            if (NPC.ai[2] % rocketRate == 0)
                            {
                                {
                                    int type = NPC.ai[2] > (rocketAmt - 2) * rocketRate ? ModContent.ProjectileType<HydrogenWarhead>() : ModContent.ProjectileType<HydrogenShell>();
                                    Vector2 acidSpeed = (Vector2.UnitY * Main.rand.NextFloat(-10f, -8f)).RotatedByRandom(MathHelper.ToRadians(missileSpread));
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, acidSpeed, type, (int)(NPC.damage * 0.25f), 3f, Main.myPlayer, Target.whoAmI);
                                }
                                if (NPC.ai[2] > rocketAmt * rocketRate)
                                {
                                    NPC.ai[2] = 0;
                                    NPC.ai[1] = 0;
                                    NPC.ai[3]++;
                                }
                            }
                            if (NPC.ai[3] >= salvoAmount)
                            {
                                NPC.ai[1] = 0;
                                NPC.ai[2] = 0;
                                NPC.ai[3] = 0;
                                Phase = (int)PhaseType.Mines;
                            }
                        }
                        break;
                    }
                case (int)PhaseType.Mines:
                    {
                        int whenToSummon = 90;
                        int mineAmt = 32;
                        int mineRange = 4000;
                        int mineSpeed = 4;
                        int phaseTime = 360;
                        NPC.velocity = NPC.DirectionTo(Target.Center) * 2;
                        NPC.ai[1]++;
                        if (NPC.ai[1] == whenToSummon)
                        {
                            for (int i = 0; i < mineAmt; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), Target.Center + new Vector2(Main.rand.Next(-mineRange, mineRange), Main.rand.Next(400, 600)), Vector2.UnitY * -mineSpeed, ModContent.ProjectileType<HydrogenMine>(), (int)(NPC.damage * 0.25f), 0f, Main.myPlayer);
                            }
                        }
                        if (NPC.ai[1] > phaseTime)
                        {
                            NPC.ai[1] = 0;
                            Phase = (int)PhaseType.Idle;
                        }
                        break;
                    }
                case (int)PhaseType.Death:
                    {
                        NPC.Calamity().newAI[1]++;
                        int doomsdayTimer = 720;
                        int spawnFridge = 120;
                        int startExplosion = doomsdayTimer - 120;
                        int tikTok = startExplosion / 11;
                        NPC.velocity *= 0.95f;
                        if (NPC.Calamity().newAI[1] == spawnFridge)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-400, -400), Vector2.UnitY * 4, ModContent.ProjectileType<Fridge>(), 0, 0f, Main.myPlayer);
                        }
                        if (NPC.Calamity().newAI[1] > doomsdayTimer)
                        {
                            NPC.active = false;
                            NPC.HitEffect();
                            NPC.NPCLoot();

                            NPC.netUpdate = true;

                            // Prevent netUpdate from being blocked by the spam counter.
                            if (NPC.netSpam >= 10)
                                NPC.netSpam = 9;
                        }
                        if (NPC.Calamity().newAI[1] == startExplosion)
                        {
                            Main.LocalPlayer.Calamity().GeneralScreenShakePower = 222;
                            SoundEngine.PlaySound(CalamityMod.NPCs.ExoMechs.Ares.AresGaussNuke.NukeExplosionSound);
                        }
                        if (NPC.Calamity().newAI[1] > startExplosion)
                        {
                            NPC.localAI[0] += 4.25f;
                        }
                        if ((10 - NPC.localAI[1]) > 0)
                        {
                            if (NPC.Calamity().newAI[1] % tikTok == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Camera);
                                CombatText.NewText(NPC.getRect(), Color.Lerp(Color.White, Color.Red, NPC.localAI[1] / 10), (int)(10 - NPC.localAI[1]));
                                NPC.localAI[1]++;
                            }
                        }
                        break;
                    }
            }
        }

        public void DustExplosion()
        {
            for (int i = 0; i < 40; i++)
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LunarRust, Main.rand.NextFloat(-22, 22), Main.rand.NextFloat(-22, 22), Scale: Main.rand.NextFloat(0.8f, 2f));
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = (Main.dust[d].position - NPC.Center).SafeNormalize(Vector2.One) * Main.rand.Next(10, 18);
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
        new FlavorTextBestiaryInfoElement("After the Archwizard was dishonorably discharged from the war, he fell into a state of smoking and gambling. During a gambling night, he sealed himself inside of a chunk of asbestos to win a bet. He was never heard from again.")
            });
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.soundDelay == 0 && Phase != (int)PhaseType.Death)
            {
                NPC.soundDelay = 3;
                SoundEngine.PlaySound(HitSound, NPC.Center);
            }
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LunarRust, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LunarRust, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<SeaPrism>(), 1, 4, 8);
        }
        public override void OnKill()
        {
            RemixDowned.downedHydrogen = true;
            CalRemixWorld.UpdateWorldBool();
        }

        public override bool SpecialOnKill()
        {
            // work you stupid stupid
            RemixDowned.downedHydrogen = true;
            CalRemixWorld.UpdateWorldBool();
            return false;
        }

        public override bool CheckDead()
        {
            NPC.life = 1;
            NPC.Calamity().newAI[0] = 1;
            NPC.active = true;
            NPC.dontTakeDamage = true;

            NPC.netUpdate = true;

            // Prevent netUpdate from being blocked by the spam counter.
            if (NPC.netSpam >= 10)
                NPC.netSpam = 9;
            return false;
        }

        public override bool CheckActive()
        {
            return NPC.Calamity().newAI[0] != 1;
        }

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            if (Phase == (int)PhaseType.Sealed || Phase == (int)PhaseType.Death)
                return false;
            return null;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if ((Phase == (int)PhaseType.Sealed && !ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[projectile.type]) || Phase == (int)PhaseType.Death)
                return false;
            return null;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture + "Goner").Value;
            Vector2 drawPos = NPC.Center - screenPos;
            if (NPC.localAI[1] > 0)
                drawPos += new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            spriteBatch.Draw(TextureAssets.Npc[Type].Value, drawPos, null, NPC.GetAlpha(drawColor), NPC.rotation, TextureAssets.Npc[Type].Value.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, Color.Red * (NPC.localAI[1] / 10), NPC.rotation, tex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
