using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>BABA IS YOU: the player is drawn as the little white creature, and sword swings change word blocks.</summary>
public class BabaPlayer : ModPlayer
{
	public override void HideDrawLayers(PlayerDrawSet drawInfo)
	{
		foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers) {
			if (layer is BabaLayer || layer == PlayerDrawLayers.HeldItem || layer == PlayerDrawLayers.FirstVanillaLayer) continue;
			layer.Hide();
		}
	}

	public override void PostUpdate()
	{
		if (Player.itemAnimation <= 0 || Player.HeldItem.damage <= 0) return;
		int reach = System.Math.Max(Player.HeldItem.width, 48);
		var r = new Rectangle((int)Player.Center.X - (Player.direction == -1 ? reach : 0), (int)Player.Center.Y - 40, reach, 70);
		foreach (NPC n in Main.npc)
			if (n.active && n.type == ModContent.NPCType<WordBlock>() && r.Intersects(n.Hitbox)) Rules.Hit(n);
	}
}
