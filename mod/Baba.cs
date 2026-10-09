using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>The little white creature that takes over the world. Hostile until a rule says BABA IS YOU.</summary>
public class Baba : ModNPC
{
	public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

	public override void SetDefaults()
	{
		NPC.width = 46; NPC.height = 44; NPC.scale = 1.25f;
		NPC.lifeMax = 45; NPC.damage = 14; NPC.defense = 2; NPC.knockBackResist = 0.6f; NPC.value = 80;
		NPC.aiStyle = NPCAIStyleID.Fighter; AIType = NPCID.Zombie;
		NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath6;
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo) =>
		spawnInfo.Player.ZoneForest && !spawnInfo.Invasion ? 0.5f : 0f;

	public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ItemDropRule.Common(ItemID.SilverCoin, 1, 3, 8));

	public override void FindFrame(int frameHeight)
	{
		if (System.Math.Abs(NPC.velocity.X) < 0.2f || NPC.velocity.Y != 0) { NPC.frame.Y = 0; return; }
		NPC.frameCounter += System.Math.Abs(NPC.velocity.X) * 0.9f + 0.5f;
		NPC.frame.Y = (int)(NPC.frameCounter / 6 % 4) * frameHeight;
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		Mix.Burst(NPC.Center, DustID.WhiteTorch, hit.Damage > NPC.life ? 24 : 6, 18f, 3f, Color.White, 1.3f);
		Mix.Burst(NPC.Center, DustID.Smoke, 3, 14f, 2f, Color.Black, 1f);
	}
}
