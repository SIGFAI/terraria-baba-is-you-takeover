using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>Applies the active rules to every enemy of the matching kind.</summary>
public class RuleNpc : GlobalNPC
{
	public override bool InstancePerEntity => true;

	bool seen, origFriendly, origGravity;
	int origDamage;
	float origKb;
	long born;
	int attackCool, pushCool;

	public override void SetDefaults(NPC npc) => born = Main.GameUpdateCount;

	public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
	{
		spawnRate = (int)(spawnRate * 0.6f);
		maxSpawns = (int)(maxSpawns * 1.5f);
	}

	public override void PostAI(NPC npc)
	{
		if (npc.type == ModContent.NPCType<WordBlock>() || npc.boss || npc.townNPC) return;
		if (!seen) {
			seen = true;
			origFriendly = npc.friendly; origDamage = npc.damage; origKb = npc.knockBackResist; origGravity = npc.noGravity;
		}
		bool you = false, win = false, hot = false, flt = false, push = false;
		int morph = -1;
		foreach (var (noun, prop) in Rules.Active) {
			if (!Rules.Matches(npc, noun)) continue;
			switch (prop) {
				case Rules.You: you = true; break;
				case Rules.Win: win = true; break;
				case Rules.Hot: hot = true; break;
				case Rules.Float: flt = true; break;
				case Rules.Push: push = true; break;
				default: if (prop != noun) morph = prop; break;
			}
		}

		npc.friendly = you || origFriendly;
		npc.damage = you || win ? 0 : origDamage;
		npc.knockBackResist = push ? 1.8f : origKb;
		npc.noGravity = flt || origGravity;
		if (attackCool > 0) attackCool--;
		if (pushCool > 0) pushCool--;

		if (morph >= 0 && Main.GameUpdateCount - born > 150) {
			int type = morph == Rules.Baba ? ModContent.NPCType<Baba>() : morph == Rules.Slime ? NPCID.BlueSlime : NPCID.Zombie;
			Rules.Morph(npc, type);
			return;
		}

		if (you) {
			if (Main.rand.NextBool(40)) Dust.NewDustPerfect(npc.Top + new Vector2(Main.rand.Next(-8, 8), 0), DustID.PinkTorch, new Vector2(0, -1.2f), 0, default, 1.1f).noGravity = true;
			Ally(npc);
		}
		if (win) {
			if (Main.rand.NextBool(3)) Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(18, 22), DustID.YellowStarDust, new Vector2(0, -1f), 0, default, 1f).noGravity = true;
			Mix.Light(npc.Center, Color.Gold);
			foreach (Player p in Mix.Players)
				if (p.Hitbox.Intersects(npc.Hitbox)) { Victory(npc); return; }
		}
		if (hot) {
			if (Main.GameUpdateCount % 20 == 0) npc.AddBuff(BuffID.OnFire, 120);
			Mix.Light(npc.Center, new Color(1f, 0.5f, 0.1f));
		}
		if (flt) Hover(npc);
		if (push && pushCool == 0) {
			foreach (Player p in Mix.Players)
				if (p.Hitbox.Intersects(npc.Hitbox)) {
					npc.velocity = new Vector2(p.direction * 9f, -5f);
					pushCool = 25;
					Mix.Burst(npc.Center, DustID.Smoke, 8, 16f, 3f);
					Mix.Sound(SoundID.Item52, npc.Center);
				}
		}
	}

	static void Hover(NPC npc)
	{
		int tx = (int)(npc.Center.X / 16f), ty = (int)(npc.Bottom.Y / 16f), gap = 0;
		while (gap < 9 && !WorldGen.SolidTile(tx, ty + gap)) gap++;
		float want = gap < 6 ? -2.2f : 0.7f;
		npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, want, 0.08f);
		if (Main.rand.NextBool(6)) Dust.NewDustPerfect(npc.Bottom, DustID.Cloud, new Vector2(0, 1f), 0, default, 1f).noGravity = true;
	}

	void Ally(NPC npc)
	{
		NPC target = Mix.Nearest(npc.Center, 14, Mix.IsHostile);
		if (target != null) {
			int dir = target.Center.X > npc.Center.X ? 1 : -1;
			npc.velocity.X = MathHelper.Clamp(npc.velocity.X + dir * 0.14f, -3.2f, 3.2f);
			npc.direction = npc.spriteDirection = dir;
		}
		if (attackCool > 0) return;
		foreach (NPC foe in Mix.NpcsNear(npc.Center, 3, Mix.IsHostile)) {
			if (!foe.Hitbox.Intersects(npc.Hitbox)) continue;
			int dir = foe.Center.X > npc.Center.X ? 1 : -1;
			foe.SimpleStrikeNPC(14, dir, false, 5f);
			Mix.Burst(foe.Center, DustID.WhiteTorch, 8, 14f, 3f);
			Mix.Sound(SoundID.NPCHit1, foe.Center);
			attackCool = 25;
			break;
		}
	}

	static void Victory(NPC npc)
	{
		Mix.Popup(npc.Top, "YOU WIN!", Color.Gold);
		Mix.Burst(npc.Center, DustID.GoldCoin, 30, 26f, 6f);
		Mix.Burst(npc.Center, DustID.YellowStarDust, 20, 26f, 5f);
		Mix.Sound(SoundID.Item4, npc.Center);
		Mix.Sound(SoundID.CoinPickup, npc.Center);
		Mix.Drop(ItemID.SilverCoin, npc.Center, 30);
		npc.active = false;
	}
}
