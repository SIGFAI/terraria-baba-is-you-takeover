using Terraria;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>Any friendly projectile that touches a word block changes the word (an aimed one only its own block).</summary>
public class WordHits : GlobalProjectile
{
	public override bool InstancePerEntity => true;
	public int Target = -1;

	public override void AI(Projectile projectile)
	{
		if (!projectile.friendly || projectile.damage <= 0) return;
		int t = ModContent.NPCType<WordBlock>();
		foreach (NPC n in Main.npc)
			if (n.active && n.type == t && (Target < 0 || Target == n.whoAmI) && projectile.Hitbox.Intersects(n.Hitbox)) {
				Rules.Hit(n);
				projectile.penetrate--;
				if (projectile.penetrate == 0) projectile.Kill();
				break;
			}
	}
}
