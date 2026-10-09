using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>The rule book: word blocks floating in the world are read as "NOUN IS PROPERTY" sentences.</summary>
public static class Rules
{
	public const int Baba = 0, Slime = 1, Zombie = 2, Is = 3, You = 4, Win = 5, Hot = 6, Float = 7, Push = 8;
	public static readonly string[] Names = { "BABA", "SLIME", "ZOMBIE", "IS", "YOU", "WIN", "HOT", "FLOAT", "PUSH" };
	public static bool IsNoun(int w) => w <= Zombie;
	public static bool IsProp(int w) => w >= You;

	public static readonly List<(int noun, int prop)> Active = new();
	public static readonly HashSet<int> ActiveBlocks = new();
	static readonly List<NPC> blocks = new();
	public static int LastRuleCount;
	/// <summary>Block (whoAmI) -> word it becomes on its next hit (used by scripted scenes).</summary>
	public static readonly Dictionary<int, int> Forced = new();

	public static bool Has(int noun, int prop)
	{
		foreach (var r in Active) if (r.noun == noun && r.prop == prop) return true;
		return false;
	}

	public static string Describe() => string.Join("   ", Active.Select(r => Names[r.noun] + " IS " + Names[r.prop]));

	public static int Word(NPC n) => (int)n.ai[0];

	/// <summary>Reads the sentences from the positions of the blocks (left to right, or top to bottom).</summary>
	public static void Update()
	{
		blocks.Clear();
		int t = ModContent.NPCType<WordBlock>();
		foreach (NPC n in Main.npc) if (n.active && n.type == t) blocks.Add(n);
		Active.Clear();
		ActiveBlocks.Clear();
		foreach (NPC mid in blocks) {
			if (Word(mid) != Is) continue;
			Link(mid, Neighbor(mid, -1, 0), Neighbor(mid, 1, 0));
			Link(mid, Neighbor(mid, 0, -1), Neighbor(mid, 0, 1));
		}
		if (Active.Count != LastRuleCount) {
			LastRuleCount = Active.Count;
		}
	}

	static void Link(NPC mid, NPC a, NPC b)
	{
		if (a == null || b == null) return;
		int n = Word(a), p = Word(b);
		if (!IsNoun(n) || !(IsProp(p) || IsNoun(p)) || n == p) return;
		Active.Add((n, p));
		ActiveBlocks.Add(a.whoAmI); ActiveBlocks.Add(mid.whoAmI); ActiveBlocks.Add(b.whoAmI);
	}

	static NPC Neighbor(NPC from, int dx, int dy)
	{
		NPC best = null; float bd = 999;
		foreach (NPC c in blocks) {
			if (c == from) continue;
			Vector2 d = c.Center - from.Center;
			float along = dx != 0 ? d.X * dx : d.Y * dy, across = dx != 0 ? Math.Abs(d.Y) : Math.Abs(d.X);
			if (along > 50 && along < 120 && across < 30 && along < bd) { best = c; bd = along; }
		}
		return best;
	}

	public static bool Matches(NPC n, int noun)
	{
		if (!n.active || n.boss || n.townNPC) return false;
		if (n.ModNPC != null) return noun == Baba && n.type == ModContent.NPCType<Baba>();
		string name = NPCID.Search.GetName(n.type);
		if (noun == Slime) return name.Contains("Slime") && !name.Contains("King");
		if (noun == Zombie) return name.Contains("Zombie");
		return false;
	}

	/// <summary>A hit (arrow, sword, anything) turns the word into the next word of its kind.</summary>
	public static void Hit(NPC block, int forced = -1)
	{
		if (block.ai[3] > 8) return;
		int w = Word(block), next = w;
		if (Forced.Remove(block.whoAmI, out int f)) forced = f;
		if (forced >= 0) next = forced;
		else if (IsNoun(w)) next = (w + 1) % 3;
		else if (IsProp(w)) next = w == Push ? You : w + 1;
		block.ai[0] = next;
		block.ai[3] = 24;
		Color c = IsNoun(next) ? new Color(255, 90, 170) : IsProp(next) ? Color.Gold : Color.White;
		Mix.Burst(block.Center, DustID.WhiteTorch, 18, 28f, 4f, c, 1.5f);
		Mix.Burst(block.Center, DustID.PinkTorch, 6, 28f, 3f);
		Mix.Sound(SoundID.Item35, block.Center);
		Mix.Popup(block.Top, Names[next], c);
	}

	public static NPC Morph(NPC old, int newType)
	{
		Vector2 pos = old.Bottom, vel = old.velocity;
		old.active = false;
		NPC fresh = Mix.Spawn(newType, pos);
		if (fresh == null) return null;
		fresh.velocity = vel;
		fresh.direction = old.direction;
		Mix.Burst(pos + new Vector2(0, -20), DustID.WhiteTorch, 22, 22f, 4f, Color.White, 1.6f);
		Mix.Burst(pos + new Vector2(0, -20), DustID.PinkTorch, 8, 22f, 3f);
		Mix.Sound(SoundID.Item8, pos);
		return fresh;
	}

	/// <summary>Floats the starting sentences in the air above the stage.</summary>
	public static void BuildBoard()
	{
		int[][] rows = {
			new[] { Baba, Is, You },
			new[] { Slime, Is, Baba },
			new[] { Zombie, Is, Hot },
		};
		Vector2 c = Mix.Center;
		for (int r = 0; r < rows.Length; r++)
			for (int i = 0; i < 3; i++) Place(rows[r][i], c + new Vector2((i - 1) * 80, -125 - r * 72));
	}

	public static NPC Place(int word, Vector2 center)
	{
		NPC n = Mix.Spawn<WordBlock>(center + new Vector2(0, 28));
		if (n == null) return null;
		n.ai[0] = word; n.ai[1] = center.X; n.ai[2] = center.Y; n.ai[3] = 20;
		n.Center = center;
		Mix.Burst(center, DustID.WhiteTorch, 14, 26f, 3f);
		return n;
	}

	/// <summary>Block of row r (0 top-down), column c, nearest to the stage.</summary>
	public static NPC BlockAt(int row, int col)
	{
		Vector2 want = Mix.Center + new Vector2((col - 1) * 80, -125 - row * 72);
		return blocks.OrderBy(b => Vector2.DistanceSquared(new Vector2(b.ai[1], b.ai[2]), want)).FirstOrDefault(b => Vector2.Distance(new Vector2(b.ai[1], b.ai[2]), want) < 40);
	}
}
