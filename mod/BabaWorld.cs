using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace Sigf.Content;

/// <summary>Reads the rules every tick and paints the world in Baba's dusk colours with a dot grid.</summary>
public class BabaWorld : ModSystem
{
	public override void PostUpdateNPCs() => Rules.Update();

	public override void ModifyScreenPosition()
	{
		Main.screenPosition.Y -= 140;
	}

	public override void PostUpdateEverything()
	{
		if (!Main.gameMenu) Main.GameZoomTarget = 1.5f;
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		layers.Insert(0, new LegacyGameInterfaceLayer("Sigf: Baba filter", () => {
			var px = TextureAssets.MagicPixel.Value;
			float w = Main.screenWidth, h = Main.screenHeight;
			Main.spriteBatch.Draw(px, new Rectangle(0, 0, (int)w + 1, (int)h + 1), new Color(70, 40, 120) * 0.2f);
			int step = 24;
			int ox = (int)(-Main.screenPosition.X % step), oy = (int)(-Main.screenPosition.Y % step);
			if (ox < 0) ox += step;
			if (oy < 0) oy += step;
			for (int y = oy; y < h; y += step)
				for (int x = ox; x < w; x += step)
					Main.spriteBatch.Draw(px, new Rectangle(x, y, 2, 2), Color.White * 0.22f);
			return true;
		}, InterfaceScaleType.None));

		layers.Add(new LegacyGameInterfaceLayer("Sigf: Rules", () => {
			string text = Rules.Active.Count == 0 ? "NO RULES" : Rules.Describe();
			Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(Main.screenWidth / 2f, 60), Color.White, 1.7f, 0.5f, 0.5f);
			return true;
		}, InterfaceScaleType.None));
	}
}
