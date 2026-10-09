using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>A floating word tile. Arrows, spells and sword swings change the word; three in a row make a rule.</summary>
public class WordBlock : ModNPC
{
	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 9;
		NPCID.Sets.ImmuneToAllBuffs[Type] = true;
	}

	public override void SetDefaults()
	{
		NPC.width = 56; NPC.height = 56;
		NPC.lifeMax = 999; NPC.damage = 0; NPC.defense = 0; NPC.knockBackResist = 0f;
		NPC.friendly = true; NPC.dontTakeDamage = true; NPC.dontCountMe = true;
		NPC.noGravity = true; NPC.noTileCollide = true; NPC.aiStyle = -1;
		NPC.npcSlots = 0f;
	}

	public override bool CheckActive() => false;
	public override bool NeedSaving() => false;

	public override void AI()
	{
		NPC.velocity = Vector2.Zero;
		NPC.timeLeft = 600;
		Vector2 home = new Vector2(NPC.ai[1], NPC.ai[2]);
		NPC.Center = home + new Vector2(0, (float)System.Math.Sin(Main.GameUpdateCount * 0.04 + NPC.whoAmI) * 4f);
		if (NPC.ai[3] > 0) NPC.ai[3]--;
		if (Rules.ActiveBlocks.Contains(NPC.whoAmI)) Mix.Light(NPC.Center, new Color(0.9f, 0.7f, 0.3f));
	}

	public override void FindFrame(int frameHeight) => NPC.frame.Y = (int)NPC.ai[0] * frameHeight;

	public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		int fh = tex.Height / 9;
		var src = new Rectangle(0, (int)NPC.ai[0] * fh, tex.Width, fh);
		bool on = Rules.ActiveBlocks.Contains(NPC.whoAmI);
		float pop = 1f + 0.3f * (NPC.ai[3] / 24f);
		Vector2 pos = NPC.Center - screenPos, origin = new Vector2(tex.Width / 2f, fh / 2f);
		if (on) sb.Draw(tex, pos, src, new Color(255, 220, 120, 0) * 0.35f, 0f, origin, pop * 1.12f, SpriteEffects.None, 0f);
		sb.Draw(tex, pos, src, on ? Color.White : new Color(130, 130, 145), 0f, origin, pop, SpriteEffects.None, 0f);
		return false;
	}
}
