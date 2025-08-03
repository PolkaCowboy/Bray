using PolkaUtilils;
using RDR2;
using RDR2.Native;

namespace Bray.Goons {
	public class TheBray : Goon {

		private int _corpseBombTimerMax = 4000;
		private int _corpseBombTimerMin = 0;
		private float _corpseBombRadius = 0;
		
		private int _corpseBombFuseLength = 0;
		private int _corpseBombAt = 0;

		public TheBray(GoonTypes GoonType, uint relationShipGroup) : base(GoonType, relationShipGroup) {
			_blipType = BlipType.BLIP_STYLE_ENEMY_SEVERE;
			Ped = World.CreatePed(PedHash.cs_aberdeenpigfarmer, GetSpawnPoint(_defaultMinSpawnDistance, _defaultMaxSpawnDistance), 0);
			Ped.SetPedPromptName(GoonType == GoonTypes.LawBray ? "The LAW Bray" : "The Bray");
			SetBrayMaxHealth();
			SetBlip();
			Ped.AddBlip(_blipType);
			AddRelationshipGroup(_braylastionshipGroup);
			/* Prep Bomb stats */
			_corpseBombFuseLength = rand.Next(_corpseBombTimerMin, _corpseBombTimerMax);
			_corpseBombRadius = PolkaUtililty.GetRandomFloat(1f, 2f) + (_corpseBombFuseLength / 1000);
		}

		public new void OnTick() {
			if (Ped != null) {
				if (Ped.IsAlive) {
					if (!_truce && !IsInCombat()) {
						Hunt(2000);
					}

					if (MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, Ped.Position, false) > 150) {
						Relocate(15, 30);
					}
					if (Game.Player.IsDead) {
						SetBrayMaxHealth();
					}
				} else {

					if (_corpseBombAt == 0) {
						_corpseBombAt = Game.GameTime + _corpseBombFuseLength;
					}

					if (Game.GameTime >= _corpseBombAt && _corpseBombAt > 0) {
						DetonateBomb();
						_corpseBombAt = 0;
						//Ped = null;
						CanRemove = true;
					}
				}
			}
		}
		public void SetBrayMaxHealth() {
			Ped.MaxHealth = 225;
			Ped.Health = 225;
		}
		public new void SetBlip(BlipType blipStyle = BlipType.BLIP_STYLE_ENEMY_SEVERE) {
			if (GoonType != GoonTypes.StealthBray) {
				var blip = Ped.GetBlip;
				MAP._BLIP_SET_STYLE(blip, (uint)blipStyle);
			}
		}

		public new void Hunt(float searchRadius) {
			_truce = false;
			World.SetRelationshipBetweenGroups(eRelationshipType.Hate, Ped.RelationshipGroup, Game.Player.Ped.RelationshipGroup);

			//33% chance Bray will target any gang member instead of just Arthur/John
			if (rand.Next(0, 3) > 0 && GoonType != GoonTypes.StealthBray) {
				PED.REGISTER_TARGET(Ped.Handle, Game.Player.Ped.Handle, false);
			}
			TASK.TASK_COMBAT_HATED_TARGETS_AROUND_PED(Ped.Handle, searchRadius, 33554432, 16);
			SetBlip(_blipType);
		}

		private void DetonateBomb() {
			if (_corpseBombFuseLength <= 1250) {
				//If timer under 1.25 seconds, lean towards a smaller kaboom

				//Roll for safe explosion
				if (rand.Next(0, 10) == 0) {
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.SparksAndFire, _corpseBombRadius, 0.25f);
					//_lastCorpseBombType = "SparksAndFire";
					return;
				}

				//Roll for Motatov style explosions
				if (rand.Next(0, 2) == 0) {
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.MolotovPlusFire, _corpseBombRadius, 1.5f);
					//_lastCorpseBombType = "MolotovPlusFire";
					return;
				}

				//Roll for small explosion that can catch fire
				if (rand.Next(0, 1) == 0) {
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.SmallerRockExplosion2, _corpseBombRadius, 1.5f);
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.OnlySound, 1f, 0f);
					//_lastCorpseBombType = "SmallerRockExplosion2";
					return;
				}

				//Roll for small explosion that is kind of not lethal and doesn't seem to catch fire
				if (rand.Next(0, 1) == 0) {
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.SmallLoudSound, _corpseBombRadius, 1.5f);
					//_lastCorpseBombType = "SmallLoudSound";
					return;
				}

				//Surprise! Medium boom
				World.AddExplosion(Ped.Position, (int)ExplosionTypes.BigExplosion2, _corpseBombRadius, 1.5f);
				//_lastCorpseBombType = "BigExplosion2";
				return;

			} else if (_corpseBombFuseLength > 1250) {
				//Roll for gigachad blast if long fuse delay
				//No gigachad outside of missions, I do have some respect for my horse
				if (_corpseBombFuseLength > 3000 && rand.Next(0, 4) == 0 && MISC.GET_MISSION_FLAG()) {
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.BigExplosion3, _corpseBombRadius, 1.5f);
					World.AddExplosion(Ped.Position, (int)ExplosionTypes.OnlySound, 1f, 0f);
					//_lastCorpseBombType = "GigaChad";
					//Make it sparkle if it is really big
					if (_corpseBombRadius >= 4f) {
						World.AddExplosion(Ped.Position, (int)ExplosionTypes.FireWork, 1f, 0f);
					}
					return;
				}

				World.AddExplosion(Ped.Position, (int)ExplosionTypes.BigExplosion3, _corpseBombRadius, 1.5f);
				//_lastCorpseBombType = "Classic Braysplosion";
				return;
			}

		}
	}

}
