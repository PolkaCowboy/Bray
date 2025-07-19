using RDR2;
using RDR2.Math;
using RDR2.Native;
using RDR2.NaturalMotion;
using RDR2.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Bray {
	public class Bray : Script {
		private string _debug;
		private bool showDebug = false;
		private List<Keys> pressedKeys = new List<Keys>();
		private string _brayString = "CS_AberdeenPigFarmer";
		private List<string> log = new List<string>();
		Random rand = new Random();
		Ped _theBray = null;
		uint _braylationship = 0;
		private bool _truce = false;
		private bool _brayCanSpawn = true;

		private int _defaultMinSpawnDistance = 35;
		private int _defaultMaxSpawnDistance = 65;

		private int _corpseBombTimerMax = 4000;
		private int _corpseBombTimerMin = 0;
		private float _corpseBombRadius = 0;
		private int _corpseBombAt = 0;

		private int _lastCorpseBombDelay = 0;
		private string _lastCorpseBombType = "";

		private int _nextStealthSpawn = 0;
		private int _stealthSpawnMinMinutes = 6;
		private int _stealthSpawnMaxMinutes = 15;

		/* Ideas */
		//TODO: Add a law cat with the Wanted spawns
		//TODO: Can I add random hats?
		//TODO: Can the mod log bray & arthur deaths per mission?

		public Bray() {

			_braylationship = World.AddRelationshipGroup($"Braylationship");

			Tick += OnTick;
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			Interval = 1;
		}

		private void OnTick(object sender, EventArgs evt) {
			_debug = string.Empty;

			if (_nextStealthSpawn <= 0) {
				_nextStealthSpawn = Game.GameTime + rand.Next(_stealthSpawnMinMinutes * 60000, _stealthSpawnMaxMinutes * 60000);
			}

			AddDebugMessage(() => $"Game Time: {Game.GameTime}\n");
			AddDebugMessage(() => $"Next Stealth Spawn: {_nextStealthSpawn}\n");
			AddDebugMessage(() => $"Stealth Spawn In: {(_nextStealthSpawn - Game.GameTime) / 600}\n");

			AddDebugMessage(() => $"In Mission: {MISC.GET_MISSION_FLAG()}\n");
			AddDebugMessage(() => $"In Combat: {Game.Player.Ped.IsInCombat}\n");
			//AddDebugMessage(() => $"Player On Train: {Game.Player.Ped.IsInTrain}\n");
			AddDebugMessage(() => $"Last Bomb Delay: {_lastCorpseBombDelay}\n");
			AddDebugMessage(() => $"Last Bomb Radius: {_corpseBombRadius}\n");
			AddDebugMessage(() => $"Last Bomb Type: {_lastCorpseBombType}\n");

			if (_theBray == null && MISC.GET_MISSION_FLAG()) {
				CreateBray(
					Game.Player.Ped.IsInTrain ? 1 : _defaultMinSpawnDistance,
					Game.Player.Ped.IsInTrain ? 2 : _defaultMaxSpawnDistance
				);
			} else if (_theBray == null && Game.Player.IsWanted) {
				CreateBray(
					Game.Player.Ped.IsInTrain ? 1 : _defaultMinSpawnDistance,
					Game.Player.Ped.IsInTrain ? 2 : _defaultMaxSpawnDistance,
					displayName: "The LAW Bray"
				);
			} else if (_theBray == null && Game.GameTime > _nextStealthSpawn && !MISC.GET_MISSION_FLAG()) {
				CreateBray(_defaultMinSpawnDistance, _defaultMaxSpawnDistance, true);
			}

			if (Game.GameTime > _nextStealthSpawn) {
				_nextStealthSpawn = 0;
			}

			if (_theBray != null) {
				//AddDebugMessage(() => $"Relationship Group: {_theBray.RelationshipGroup}\n");
				//AddDebugMessage(() => $"Relationship With Ped Bray->Me: {_theBray.GetRelationshipWithPed(Game.Player.Ped)}\n");
				//AddDebugMessage(() => $"Relationship With Ped Me->Bray: {Game.Player.Ped.GetRelationshipWithPed(_theBray)}\n");
				//AddDebugMessage(() => $"Ped Ids: {PLAYER.PLAYER_PED_ID()}, {Game.Player.Ped.Handle}\n");
				//AddDebugMessage(() => $"Bray Handle: {_theBray.Handle}\n");

				AddDebugMessage(() => $"Bray In Combat: {PED.IS_PED_IN_COMBAT(_theBray.Handle, PLAYER.PLAYER_PED_ID())}\n");
				AddDebugMessage(() => $"Position: {_theBray.Position.X}, {_theBray.Position.Y}, {_theBray.Position.Z}\n");
				AddDebugMessage(() => $"Distance: {MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, _theBray.Position, false)}\n");

				

				if (pressedKeys.Contains(Keys.F10)) {
					CAM._FORCE_CINEMATIC_DEATH_CAM_ON_PED(_theBray.Handle);
					RDR2.UI.Screen.StopAllEffects();
				}

				if (_theBray.IsAlive) {
					AddDebugMessage(() => $"Bray is Alive\n");

					if (!_truce && !PED.IS_PED_IN_COMBAT(_theBray.Handle, PLAYER.PLAYER_PED_ID())) {
						Hunt(2000);
					}

					if (MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, _theBray.Position, false) > 150) {
						ComeToDaddy(
							Game.Player.Ped.IsInTrain ? 1 : 15,
							Game.Player.Ped.IsInTrain ? 1 : 30
						);
					}

					if (Game.Player.IsDead) {
						SetBrayMaxHealth();
					}
				} else {
					AddDebugMessage(() => $"Bray is Dead\n");

					if (_corpseBombAt > 0) { AddDebugMessage(() => $"Bomb in {_corpseBombAt - Game.GameTime}\n"); }

					if (_corpseBombAt == 0) {
						_lastCorpseBombDelay = rand.Next(_corpseBombTimerMin, _corpseBombTimerMax);
						_corpseBombRadius = GetRandomFloat(1f, 2f) + (_lastCorpseBombDelay / 1000);
						_corpseBombAt = Game.GameTime + _lastCorpseBombDelay;
					}
					if (Game.GameTime >= _corpseBombAt && _corpseBombAt > 0) {

						DetonateBomb();
						//World.AddExplosion(_theBray.Position, (int)ExplosionTypes.SmallLoudSound, 5f, 1.5f);
						//World.AddExplosion(_theBray.Position, (int)ExplosionTypes.OnlySound, 1f, 0f);
						//World.AddExplosion(_theBray.Position, (int)ExplosionTypes.FireWork, 1f, 0f);

						_theBray = null;
						_corpseBombAt = 0;
					}
				}

			} else {
				AddDebugMessage(() => "There is no Bray\n");
			}



			foreach (var l in log) {
				AddDebugMessage(() => l.ToString() + "\n");
			}

			AddDebugMessage(() => $"Keys: {string.Join(", ", pressedKeys)}\n");

			if (showDebug) {
				TextElement textElement = new TextElement($"{_debug}", new PointF(100.0f, 100.0f), 0.35f);
				textElement.Draw();
			}

		}
		private void OnKeyDown(object sender, KeyEventArgs e) {
			if (!pressedKeys.Contains(e.KeyCode)) {
				pressedKeys.Add(e.KeyCode);
			}

			/* Debug Functions */

			if (e.KeyCode == Keys.F13 && _theBray == null) {
				CreateBray(
					Game.Player.Ped.IsInTrain ? 1 : _defaultMinSpawnDistance,
					Game.Player.Ped.IsInTrain ? 2 : _defaultMaxSpawnDistance,
					false
				);
			}

			if (e.KeyCode == Keys.F15) {
				ComeToDaddy(5, 10);
			}

			if (e.KeyCode == Keys.F19) {
				showDebug = !showDebug;
			}

			/* Mod Tools */

			if (e.KeyCode == Keys.F14) {
				ToggleBray();
			}

			if (e.KeyCode == Keys.F16) {
				Hunt(300);
			}

			if (e.KeyCode == Keys.F18) {
				EndTheHate();
			}

			if (e.KeyCode == Keys.F17) {
				Skedaddle();
			}

			if (e.KeyCode == Keys.F20) {
				//World.AddExplosion(Game.Player.Ped.Position, (int)ExplosionTypes.BigSloMo, 5f, 1.5f);
			}

		}

		private void OnKeyUp(object sender, KeyEventArgs e) {
			pressedKeys.RemoveAll(p => p == e.KeyCode);
		}

		public void Skedaddle() {
			EndTheHate();
			//PED.REMOVE_PED_FROM_GROUP(_theBray.Handle);
			PED.REMOVE_RELATIONSHIP_GROUP(_braylationship);
			_theBray.Task.WanderAround();
			//TASK.TASK_FLEE_PED(_theBray.Handle, Game.Player.Ped.Handle, (int)eFleeStyle.AnnoyedRetreat, 0, -1f, -1, 0);
			var blip = _theBray.GetBlip;
			blip.Delete();

			_theBray = null;
			_truce = false;
		}

		public void ComeToDaddy(int minDistance, int maxDistance) {
			_theBray.Position = GetSpawnPoint(minDistance, maxDistance);
		}

		public void CreateBray(int minDistance, int maxDistance, bool stealthSpawn = false, string displayName = "The Bray") {
			if (!_brayCanSpawn) {
				return;
			}
			//log.Add($"Player Position: {Game.Player.Ped.Position.X}, {Game.Player.Ped.Position.Y}, {Game.Player.Ped.Position.Z}");
			//log.Add($"Spawn Position: {spawnPoint.X}, {spawnPoint.Y}, {spawnPoint.Z}");


			_theBray = World.CreatePed(PedHash.cs_aberdeenpigfarmer, GetSpawnPoint(minDistance, maxDistance), 0);
			_theBray.RelationshipGroup = _braylationship;
			SetBrayMaxHealth();
			if (!stealthSpawn) {
				_theBray.AddBlip(BlipType.BLIP_STYLE_NEUTRAL);
			}
			_theBray.SetPedPromptName(displayName);
			//PED.SET_PED_AS_GROUP_MEMBER(_theBray.Handle, _playerGroup);

			Hunt(300, stealthSpawn);

		}

		public Vector3 GetSpawnPoint(int minDistance, int maxDistance) {
			var x = rand.Next(minDistance, maxDistance);
			x = rand.Next(0, 2) == 0 ? x * -1 : x;
			var y = rand.Next(minDistance, maxDistance);
			y = rand.Next(0, 2) == 0 ? y * -1 : y;

			log = new List<string> {
				$"Offset: {x}, {y}"
			};

			return new Vector3(
				Game.Player.Ped.Position.X + x,
				Game.Player.Ped.Position.Y + y,
				Game.Player.Ped.IsInTrain ? Game.Player.Ped.Position.Z : -200
			);

		}

		public void Hunt(float searchRadius, bool stealth = false) {
			World.SetRelationshipBetweenGroups(eRelationshipType.Hate, _braylationship, Game.Player.Ped.RelationshipGroup);

			//33% chance Bray will target any gang member instead of just Arthur/John
			if (rand.Next(0, 3) == 0 && !stealth) {
				//TASK.CLEAR_PED_TASKS_IMMEDIATELY(_theBray.Handle, true, true);
			} else {
				PED.REGISTER_TARGET(_theBray.Handle, Game.Player.Ped.Handle, false);
			}

			TASK.TASK_COMBAT_HATED_TARGETS_AROUND_PED(_theBray.Handle, searchRadius, 33554432, 16);
			if (!stealth) {
				var blip = _theBray.GetBlip;
				MAP._BLIP_SET_STYLE(blip, (uint)BlipType.BLIP_STYLE_ENEMY_SEVERE);
			}
		}

		public void EndTheHate() {
			_truce = true;
			World.SetRelationshipBetweenGroups(eRelationshipType.Like, _braylationship, Game.Player.Ped.RelationshipGroup);
			TASK.CLEAR_PED_TASKS_IMMEDIATELY(_theBray.Handle, true, true);
			var blip = _theBray.GetBlip;
			MAP._BLIP_SET_STYLE(blip, (uint)BlipType.BLIP_STYLE_NEUTRAL);
		}

		public void SetBrayMaxHealth() {
			_theBray.MaxHealth = 225;
			_theBray.Health = 225;
		}

		public void ToggleBray() {
			_brayCanSpawn = !_brayCanSpawn;
			if (_brayCanSpawn) {
				RDR2.UI.Screen.DisplaySubtitle($"Bray can spawn.");
			} else {
				RDR2.UI.Screen.DisplaySubtitle($"Bray can NOT spawn.");
			}
		}

		public void AddDebugMessage(Func<string> message) {
			if (showDebug) {
				_debug += message();
			}
		}

		public static float GetRandomFloat(float min = 0.5f, float max = 2.0f) {
			Random rand = new Random();
			double range = max - min;
			return (float)(min + rand.NextDouble() * range);
		}

		private void DetonateBomb() {
			if (_lastCorpseBombDelay <= 1250) {
				//If timer under 1.25 seconds, lean towards a smaller kaboom

				//Roll for safe explosion
				if (rand.Next(0, 10) == 0) {
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.SparksAndFire, _corpseBombRadius, 0.25f);
					_lastCorpseBombType = "SparksAndFire";
					return;
				}

				//Roll for Motatov style explosions
				if (rand.Next(0, 2) == 0) {
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.MolotovPlusFire, _corpseBombRadius, 1.5f);
					_lastCorpseBombType = "MolotovPlusFire";
					return;
				}

				//Roll for small explosion that can catch fire
				if (rand.Next(0, 1) == 0) {
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.SmallerRockExplosion2, _corpseBombRadius, 1.5f);
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.OnlySound, 1f, 0f);
					_lastCorpseBombType = "SmallerRockExplosion2";
					return;
				}

				//Roll for small explosion that is kind of not lethal and doesn't seem to catch fire
				if (rand.Next(0, 1) == 0) {
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.SmallLoudSound, _corpseBombRadius, 1.5f);
					_lastCorpseBombType = "SmallLoudSound";
					return;
				}

				//Surprise! Medium boom
				World.AddExplosion(_theBray.Position, (int)ExplosionTypes.BigExplosion2, _corpseBombRadius, 1.5f);
				_lastCorpseBombType = "BigExplosion2";
				return;

			} else if (_lastCorpseBombDelay > 1250) {
				//Roll for gigachad blast if long fuse delay
				//No gigachad outside of missions, I do have some respect for my horse
				if (_lastCorpseBombDelay > 3000 && rand.Next(0,4) == 0 && MISC.GET_MISSION_FLAG()) {
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.BigExplosion3, _corpseBombRadius, 1.5f);
					World.AddExplosion(_theBray.Position, (int)ExplosionTypes.OnlySound, 1f, 0f);
					_lastCorpseBombType = "GigaChad";
					//Make it sparkle if it is really big
					if (_corpseBombRadius >= 4f) {
						World.AddExplosion(_theBray.Position, (int)ExplosionTypes.FireWork, 1f, 0f);
					}
					return;
				}

				World.AddExplosion(_theBray.Position, (int)ExplosionTypes.BigExplosion3, _corpseBombRadius, 1.5f);
				_lastCorpseBombType = "Classic Braysplosion";
				return;
			}
			
		}

	}


}
