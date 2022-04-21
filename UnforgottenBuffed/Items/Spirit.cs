using System;
using Microsoft.Xna.Framework;
using StarsAbove;
using StarsAbove.Items.Materials;
using StarsAbove.Items.AstralDamageClass;
using StarsAbove.Buffs;
using StarsAbove.Items.Prisms;
using StarsAbove.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace UnforgottenBuffed.Items
{
	public class Global
	{
		//Global variable, since the Fate Sealed stun is called in multiple methods
		public static int fateSealedStun = 0;
	}

	public class Spirit : ModItem
	{
		Mod starsAbove = ModLoader.GetMod("StarsAbove");
		public override void SetStaticDefaults() 
		{
			//Name
			DisplayName.SetDefault("Spirit Blossom"); 
			//Description
			Tooltip.SetDefault("[c/FF5858:This weapon is autoswing, and has a multitude of attacks and abilities]\n[c/8E60D1:Way of the Hunter]\nAttacks will alternate between two katanas: [c/85C4DA:Steel Wind] and [c/F10079:Spirit Azakana]\n[c/85C4DA:Steel Wind] has a 50% critical hit rate independent of other crit calculations\n[c/F10079:Spirit Azakana] has no knockback and cannot critically strike but repeats half the damage dealt as true damage\n[c/8E60D1:Mortal Steel]\nRight click to thrust forward, consuming 50 mana and dealing damage with a bonus effect depending on the preluding slash\nIf the last sword swung was [c/85C4DA:Steel Wind], the thrust will inflict [c/8E60D1:Mortal Wounds] to all targets struck, dealing damage over the next 10 seconds\nIf the last sword swung was [c/F10079:Spirit Azakana], purge all targets afflicted with [c/8E60D1:Mortal Wounds] for 2000% critical damage\n[c/8E60D1:Soul Unbound]\nHold up while pressing right click to perform [c/8E60D1:Soul Unbound]\n[c/8E60D1:Soul Unbound] will launch you forward and grant bonus movement speed for 8 seconds\nOnce [c/8E60D1:Soul Unbound] ends, all nearby foes take damage equal to 33% of the damage you inflicted with this weapon during this ability's duration\nOnce [c/8E60D1:Soul Unbound] ends, blink back to the position where [c/8E60D1:Soul Unbound] was casted\n[c/8E60D1:Fate Sealed]\nCharge right click to blink forward with blinding speed,consuming 200 mana and striking nearby targets with both swings of [c/8E60D1:Mortal Steel]\n'No fate nor destiny! Only tomorrow'");
		}

		public override void SetDefaults()
		{
			base.item.damage = 5000;
			base.item.melee = true;
			base.item.width = 65;
			base.item.height = 70;
			base.item.useTime = 20;
			base.item.useAnimation = 20;
			base.item.useStyle = 5;
			base.item.noMelee = true;
			base.item.noUseGraphic = true;
			base.item.knockBack = 4f;
			base.item.rare = 10;
			base.item.autoReuse = true;
			base.item.channel = true;
			base.item.shoot = 10;
			base.item.shootSpeed = 15f;
			base.item.value = Item.buyPrice(0, 1, 0, 0);
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			//Disables weapon shortly after casting Fate Sealed
			if (Global.fateSealedStun > 0) {
				return false;
			}
			//Removes the immunity frames given to the player after casting Fate Sealed
			else {
				player.immune = false;
			}
			//Prevents the player from using the weapon for the last 40 ticks of Soul Unbound
			for (int i = 0; i < player.CountBuffs(); i++) {
				if (player.buffType[i] == ModContent.BuffType<SoulUnbound>() && player.buffTime[i] <= 40) {
					return false;
				}
			}
			//Soul Unbound
			if (player.altFunctionUse == 2 && player.controlUp) {
				//Does not allow the player to cast Soul Unbound if on cooldown or if already active
				if (Main.LocalPlayer.HasBuff(ModContent.BuffType<SoulUnbound>()) || Main.LocalPlayer.HasBuff(ModContent.BuffType<SoulUnboundCooldown>())) {
					return false;
				}
				//Initial burst of speed
				Vector2 leap = Vector2.Normalize(player.DirectionTo(player.GetModPlayer<StarsAbovePlayer>().playerMousePos) * (float)Main.rand.Next(20, 22)) * 15f;
				player.velocity = leap;
				player.AddBuff(ModContent.BuffType<SoulUnbound>(), 480, true);
				player.GetModPlayer<StarsAbovePlayer>().soulUnboundLocation = new Vector2(player.Center.X, player.Center.Y - 5f);
				this.vector32 = new Vector2(player.Center.X, player.Center.Y - 5f);
				Projectile.NewProjectile(this.vector32.X, this.vector32.Y, 0f, 0f, ModContent.ProjectileType<SoulMarker>(), 0, 0f, player.whoAmI, 0f, 0f);
				player.GetModPlayer<StarsAbovePlayer>().soulUnboundActive = true;
			}
			return true;
		}

		public override void HoldItem(Player player)
		{
			//Projectile speed and directon calculations
			float launchSpeed2 = 12f;
			Vector2 vector2 = Vector2.Normalize(Main.MouseWorld - player.Center);
			Vector2 arrowVelocity2 = vector2 * launchSpeed2;
			//Makes the player immune shortly after Soul Unbound is cast
			if (Global.fateSealedStun > 0) {
				Global.fateSealedStun--;
				player.immune = true;
			}
			if (player.altFunctionUse == 2 && !player.controlUp) {
				base.item.useTime = 0;
				base.item.useAnimation = 0;
				player.GetModPlayer<StarsAbovePlayer>().bowChargeActive = true;
				player.GetModPlayer<StarsAbovePlayer>().bowCharge += 4f;
				//Visually sets the charge meter to 1 when the player lacks enough mana to cast the thrust
				if (player.statMana < 50) {
					player.GetModPlayer<StarsAbovePlayer>().bowCharge = 1f;
				}
				//Particle Effects
				float bowCharge = player.GetModPlayer<StarsAbovePlayer>().bowCharge;
				if (player.GetModPlayer<StarsAbovePlayer>().bowCharge == 96f) {
					for (int d = 0; d < 88; d++) {
						Dust.NewDust(player.Center, 0, 0, 43, 0f + (float)Main.rand.Next(-12, 12), 0f + (float)Main.rand.Next(-12, 12), 150, default(Color), 0.8f);
					}
				}
				//Particle Effects
				if (player.GetModPlayer<StarsAbovePlayer>().bowCharge < 100f) {
					if (this.currentSwing == 1) {
						for (int i = 0; i < 30; i++) {
							Vector2 offset = default(Vector2);
							double angle = Main.rand.NextDouble() * 2.0 * 3.141592653589793;
							offset.X += (float)(Math.Sin(angle) * (double)(100f - player.GetModPlayer<StarsAbovePlayer>().bowCharge));
							offset.Y += (float)(Math.Cos(angle) * (double)(100f - player.GetModPlayer<StarsAbovePlayer>().bowCharge));
							Dust dust = Dust.NewDustPerfect(player.MountedCenter + offset, 266, new Vector2?(player.velocity), 200, default(Color), 0.5f);
							dust.fadeIn = 0.1f;
							dust.noGravity = true;
						}
					}
					else {
						for (int j = 0; j < 30; j++) {
							Vector2 offset2 = default(Vector2);
							double angle2 = Main.rand.NextDouble() * 2.0 * 3.141592653589793;
							offset2.X += (float)(Math.Sin(angle2) * (double)(100f - player.GetModPlayer<StarsAbovePlayer>().bowCharge));
							offset2.Y += (float)(Math.Cos(angle2) * (double)(100f - player.GetModPlayer<StarsAbovePlayer>().bowCharge));
							Dust dust2 = Dust.NewDustPerfect(player.MountedCenter + offset2, 57, new Vector2?(player.velocity), 200, default(Color), 0.5f);
							dust2.fadeIn = 0.1f;
							dust2.noGravity = true;
						}
					}
					Vector2 vector;
					vector = new Vector2((float)Main.rand.Next(-28, 28) * -9.88f, (float)Main.rand.Next(-28, 28) * -9.88f);
					Dust dust3 = Main.dust[Dust.NewDust(player.MountedCenter + vector, 1, 1, 43, 0f, 0f, 255, new Color(0.8f, 0.4f, 1f), 0.8f)];
					dust3.velocity = -vector / 12f;
					dust3.velocity -= player.velocity / 8f;
					dust3.noLight = true;
					dust3.noGravity = true;
				}
				else {
					Dust.NewDust(player.Center, 0, 0, 20, 0f + (float)Main.rand.Next(-5, 5), 0f + (float)Main.rand.Next(-5, 5), 150, default(Color), 0.8f);
				}
			}
			//Once right click is let go, perform an attack
			else if (Main.mouseRightRelease && player.GetModPlayer<StarsAbovePlayer>().bowCharge > 0f) {
				base.item.useTime = 20;
				base.item.useAnimation = 20;
				//Fate Sealed
				if (player.GetModPlayer<StarsAbovePlayer>().bowCharge >= 80f && player.statMana >= 200) {
					//Mana cost
					player.statMana -= 200;
					player.manaRegenDelay = 230;
					//Ensures that the player cannot teleport into 
					if (!Collision.SolidCollision(Main.MouseWorld, player.width, player.height)) {
						player.Teleport(Main.MouseWorld, 1);
					}
					//Fate Sealed first attack
					Projectile.NewProjectile(player.MountedCenter.X, player.MountedCenter.Y, arrowVelocity2.X, arrowVelocity2.Y, ModContent.ProjectileType<SteelTempestSwing3>(), base.item.damage, 3f, player.whoAmI, 0f, 0f);
					//Stuns the player for 0.5 seconds
					Global.fateSealedStun = 30;
				}
				//Mortal Steel
				else if (player.statMana >= 50){
					//Mana Cost
					player.statMana -= 50;
					player.manaRegenDelay = 230;
					//SFX
					Main.PlaySound(SoundID.Item1, player.position);
					//Changes thrust depending on last sword swung
					player.GetModPlayer<StarsAbovePlayer>().bowChargeActive = false;
					player.GetModPlayer<StarsAbovePlayer>().bowCharge = 0f;
					if (this.currentSwing == 1)
					{
						Projectile.NewProjectile(player.MountedCenter.X, player.MountedCenter.Y, arrowVelocity2.X, arrowVelocity2.Y, ModContent.ProjectileType<SteelTempestSwing4>(), base.item.damage / 2, 3f, player.whoAmI, 0f, 0f);
					}
					else
					{
						Projectile.NewProjectile(player.MountedCenter.X, player.MountedCenter.Y, arrowVelocity2.X, arrowVelocity2.Y, ModContent.ProjectileType<SteelTempestSwing3>(), base.item.damage / 2, 3f, player.whoAmI, 0f, 0f);
					}
				}
				//Debug
				player.GetModPlayer<StarsAbovePlayer>().bowChargeActive = false;
				player.GetModPlayer<StarsAbovePlayer>().bowCharge = 0f;
			}
			//Fate Sealed second attack with 0.25 second delay (15th tick)
			if (Global.fateSealedStun == 15) {
				Projectile.NewProjectile(player.MountedCenter.X, player.MountedCenter.Y, arrowVelocity2.X, arrowVelocity2.Y, ModContent.ProjectileType<SteelTempestSwing4>(), base.item.damage, 3f, player.whoAmI, 0f, 0f);
			}
			//Draws a line between the player and the Soul Unbound anchor
			if (player.GetModPlayer<StarsAbovePlayer>().soulUnboundActive) {
				for (int k = 0; k < 50; k++) {
					Dust dust4 = Dust.NewDustPerfect(Vector2.Lerp(player.Center, this.vector32, (float)k / 50f), 20, null, 240, default(Color), 0.3f);
					dust4.fadeIn = 0.3f;
					dust4.noLight = true;
					dust4.noGravity = true;
				}
			}
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			//Projectile speed and direction calculations
			float launchSpeed = 59f;
			Vector2 vector2 = Vector2.Normalize(Main.MouseWorld - player.Center);
			Vector2 arrowVelocity = vector2 * launchSpeed;
			//Alternating Autoswing
			if (player.channel && player.altFunctionUse != 2) {
				Main.PlaySound(SoundID.Item1, player.position);
				if (this.currentSwing == 1) {
					this.currentSwing++;
					Projectile.NewProjectile(player.MountedCenter.X, player.MountedCenter.Y, arrowVelocity.X, arrowVelocity.Y, ModContent.ProjectileType<SteelTempestSwing>(), base.item.damage, 3f, player.whoAmI, 0f, 0f);
				}
				else {
					this.currentSwing = 1;
					Projectile.NewProjectile(player.MountedCenter.X, player.MountedCenter.Y, arrowVelocity.X, arrowVelocity.Y, ModContent.ProjectileType<SteelTempestSwing2>(), base.item.damage, 0f, player.whoAmI, 0f, 0f);
				}
			}
			return false;
		}

		public override bool ConsumeAmmo(Player player)
		{
			return false;
		}

		public override void AddRecipes() 
		{
			//Recipe
			ModRecipe recipe = new ModRecipe(mod);
			if (starsAbove != null) {
				recipe.AddIngredient(ModContent.ItemType<TotemOfLightEmpowered>(), 1);
				recipe.AddIngredient(starsAbove.ItemType("PrismaticCore"), 10);
				recipe.AddIngredient(starsAbove.ItemType("Unforgotten"), 1);
			}
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		//Miscellaneous Variables
		private int currentSwing;
		private Vector2 vector32;
	}

	public class UnforgottenPlayer : ModPlayer
	{
		public override void PreUpdateMovement() {
			//Freezes the player right before casting Fate Sealed and for a brief moment after
			if (Global.fateSealedStun > 0) {
				player.velocity = new Vector2(0, 0);
			}
			//Bullet time
			if (player.GetModPlayer<StarsAbovePlayer>().bowCharge >= 100f && player.statMana >= 200) {
				player.velocity = new Vector2(0, 0);
			}
			else if (player.GetModPlayer<StarsAbovePlayer>().bowCharge >= 90f && player.statMana >= 200) {
				player.velocity /= 4;
			}
			else if (player.GetModPlayer<StarsAbovePlayer>().bowCharge >= 80f && player.statMana >= 200) {
				player.velocity /= 3;
			}
			else if (player.GetModPlayer<StarsAbovePlayer>().bowCharge >= 70f && player.statMana >= 200) {
				player.velocity /= 2;
			}
		}
	}
}