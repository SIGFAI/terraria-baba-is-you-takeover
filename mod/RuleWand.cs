using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>Fires IS blocks that rewrite whatever they hit into a Baba. Aims at the nearest enemy by itself.</summary>
public class RuleWand : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 40; Item.height = 38;
		Item.damage = 12; Item.DamageType = DamageClass.Ranged; Item.knockBack = 3f; Item.noMelee = true;
		Item.useTime = 26; Item.useAnimation = 26; Item.useStyle = ItemUseStyleID.Shoot; Item.autoReuse = true;
		Item.UseSound = SoundID.Item8; Item.rare = ItemRarityID.Pink; Item.value = Item.buyPrice(gold: 1);
		Item.shoot = ModContent.ProjectileType<IsBolt>(); Item.shootSpeed = 11f;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		NPC target = Mix.Nearest(player.Center, 22, Mix.IsHostile);
		if (target != null) velocity = Mix.Aim(player.Center, target.Center, Item.shootSpeed);
	}

	public override void AddRecipes() =>
		CreateRecipe().AddIngredient(ItemID.Wood, 10).AddIngredient(ItemID.PinkGel, 5).AddTile(TileID.WorkBenches).Register();
}
