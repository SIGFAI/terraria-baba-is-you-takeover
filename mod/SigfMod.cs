using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class SigfMod : ModSystem
{
	/// <summary>Shoots an arrow up into a word block (row 0 = bottom row, column 0..2); word = what it becomes (-1 = next word).</summary>
	static void Arrow(int row, int col, int word = -1)
	{
		NPC b = Rules.BlockAt(row, col);
		if (b == null) return;
		if (word >= 0) Rules.Forced[b.whoAmI] = word;
		Mix.Camera(b.Center + new Vector2(0, 120), 4);
		Vector2 from = b.Center + new Vector2(0, 150);
		Projectile a = Mix.Shoot(ProjectileID.WoodenArrowFriendly, from, new Vector2(0, -14f), 10);
		if (a != null) a.GetGlobalProjectile<WordHits>().Target = b.whoAmI;
		Mix.Sound(SoundID.Item5, from);
	}

	static void Wave(int type, int count, double radius)
	{
		for (int i = 0; i < count; i++) Mix.Spawn(type, Mix.Ground(radius));
	}

	static void Shower(int count)
	{
		Vector2 c = Mix.Host.Center;
		int baba = ModContent.NPCType<Baba>();
		for (int i = 0; i < count; i++) {
			NPC n = Mix.Spawn(baba, c + new Vector2((i - count / 2f) * 46, -330 - Main.rand.Next(80)));
			if (n != null) Mix.Burst(n.Center, DustID.WhiteTorch, 10, 20f, 3f);
		}
		Mix.Sound(SoundID.Item29, c);
	}

	public override void OnWorldLoad()
	{
		Mix.WanderTiles = 14;
		Mix.After(0.8, Rules.BuildBoard);
		Mix.After(2.5, () => Shower(7));
		Mix.Every(5, () => {
			if (Mix.Hostiles(Mix.Host.Center, 40).Count < 12) Mix.Spawn(NPCID.BlueSlime, Mix.Ground(18));
		});
		Mix.Every(8, () => {
			if (Mix.Hostiles(Mix.Host.Center, 40).Count < 12) Mix.Spawn(NPCID.Zombie, Mix.Ground(22));
		});
		Mix.After(2, () => Mix.Spawn(NPCID.BlueSlime, Mix.Ahead(6)));
		// Not in the demo: a stray arrow rewrites a rule now and then, so the world never settles.
		if (!Mix.IsDemo) Mix.Every(25, () => Arrow(Main.rand.Next(3), Main.rand.Next(3) == 1 ? 1 : (Main.rand.NextBool() ? 0 : 2)));

		Mix.Demo(1, () => Mix.Title("BABA IS YOU", "Terraria x Baba Is You", 4));
		Mix.Demo(3, () => Wave(NPCID.BlueSlime, 3, 8));
		Mix.Demo(5, () => Wave(NPCID.Zombie, 3, 16));
		Mix.Demo(15, () => { Mix.Title("BREAK THE RULES", "arrows rewrite the words", 3); Arrow(0, 2, Rules.Win); });
		Mix.Demo(19, () => { Wave(NPCID.BlueSlime, 3, 5); Mix.Spawn<Baba>(Mix.Ahead(3)); });
		Mix.Demo(30, () => { Arrow(2, 2, Rules.Float); Wave(NPCID.Zombie, 3, 14); });
		Mix.Demo(38, () => { Arrow(0, 2, Rules.You); });
		Mix.Demo(42, () => { Mix.Title("RULE WAND", "shoot IS blocks: X IS BABA", 3); Mix.Arm<RuleWand>(); Wave(NPCID.Zombie, 2, 10); });
		Mix.Demo(52, () => Wave(NPCID.BlueSlime, 2, 10));
		Mix.Demo(56, () => { Arrow(1, 0, Rules.Zombie); Arrow(2, 2, Rules.Push); });
		Mix.Demo(66, () => { Arrow(1, 0, Rules.Slime); Wave(NPCID.Zombie, 2, 10); });
	}
}
