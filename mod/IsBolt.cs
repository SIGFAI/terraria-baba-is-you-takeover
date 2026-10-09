using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>A flying IS block: "X IS BABA" for whatever it touches.</summary>
public class IsBolt : ModProjectile
{
	public override void SetDefaults()
	{
		Projectile.width = 20; Projectile.height = 20; Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Ranged; Projectile.penetrate = 1; Projectile.timeLeft = 120;
		Projectile.tileCollide = true; Projectile.ignoreWater = true;
	}

	public override void AI()
	{
		Projectile.rotation += 0.25f * System.Math.Sign(Projectile.velocity.X == 0 ? 1 : Projectile.velocity.X);
		Mix.Light(Projectile.Center, new Color(1f, 0.4f, 0.8f));
		if (Main.rand.NextBool(2))
			Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, -Projectile.velocity * 0.1f, 0, default, 1.4f).noGravity = true;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (target.boss || target.townNPC || target.life <= 0 || target.type == ModContent.NPCType<Baba>()) return;
		Mix.Popup(target.Top + new Vector2(0, -12), (Rules.Matches(target, Rules.Slime) ? "SLIME" : Rules.Matches(target, Rules.Zombie) ? "ZOMBIE" : NPCID.Search.GetName(target.type).ToUpper()) + " IS BABA!", Color.HotPink);
		Rules.Morph(target, ModContent.NPCType<Baba>());
	}

	public override void OnKill(int timeLeft)
	{
		Mix.Burst(Projectile.Center, DustID.PinkTorch, 14, 14f, 4f, default, 1.5f);
		Mix.Sound(SoundID.Item10, Projectile.Center);
	}
}
