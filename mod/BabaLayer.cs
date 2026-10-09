using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>Draws the white creature where the player's body would be.</summary>
public class BabaLayer : PlayerDrawLayer
{
	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.FirstVanillaLayer);

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		Player p = drawInfo.drawPlayer;
		if (p.dead) return;
		Texture2D tex = ModContent.Request<Texture2D>("Sigf/Content/Baba").Value;
		int fh = tex.Height / 4;
		int frame = 0;
		if (p.velocity.Y == 0 && System.Math.Abs(p.velocity.X) > 0.5f) frame = (int)(Main.GameUpdateCount / 5 % 4);
		else if (p.velocity.Y != 0) frame = 2;
		Vector2 pos = new Vector2((int)(p.Center.X - Main.screenPosition.X), (int)(p.Bottom.Y + p.gfxOffY - Main.screenPosition.Y)) + new Vector2(0, 2);
		var fx = p.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		drawInfo.DrawDataCache.Add(new DrawData(tex, pos, new Rectangle(0, frame * fh, tex.Width, fh), drawInfo.colorArmorBody, 0f, new Vector2(tex.Width / 2f, fh), 1.15f, fx, 0));
	}
}
