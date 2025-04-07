using RDR2;
using RDR2.Math;
using RDR2.Native;
using RDR2.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
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
		int _playerGroup = 0;

		private int _corpseBombTimer = 2000;
		private int _corpseBombAt = 0;

		//TODO: Bray with no blips randomly spawns every 5-20 minutes during free road
		//TODO: Add a law cat with the Wanted spawns

		public Bray() {

			_braylationship = World.AddRelationshipGroup($"Braylationship");

			//var playerGroup = PED.CREATE_GROUP(0);
			//PED.SET_PED_AS_GROUP_MEMBER(Game.Player.Ped.Handle, playerGroup);
			//PED.SET_PED_AS_GROUP_LEADER(Game.Player.Ped.Handle, playerGroup, true);


			Tick += OnTick;
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			Interval = 1;
		}

		private void OnTick(object sender, EventArgs evt) {
			_debug = string.Empty;

			if (_theBray == null && (MISC.GET_MISSION_FLAG() || Game.Player.IsWanted)) {
				CreateBray();
			}

			if (_theBray != null) {

				if (pressedKeys.Contains(Keys.F10) || pressedKeys.Contains(Keys.F14)) {
					//CAM.FORCE_CINEMATIC_RENDERING_THIS_UPDATE(true);
					//CAM.CINEMATIC_LOCATION_OVERRIDE_TARGET_ENTITY_THIS_UPDATE("Cam", _theBray.Handle);
					CAM._FORCE_CINEMATIC_DEATH_CAM_ON_PED(_theBray.Handle);
					RDR2.UI.Screen.StopAllEffects();
				}

				if (!_truce && !PED.IS_PED_IN_COMBAT(_theBray.Handle, PLAYER.PLAYER_PED_ID())) {
					Hunt(2000);
				}

				if (MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, _theBray.Position, false) > 150) {
					ComeToDaddy(20);
				}

				if (Game.Player.IsDead) {
					SetBrayMaxHealth();
				}

				//AddDebugMessage(() => $"Relationship Group: {_theBray.RelationshipGroup}\n");
				//AddDebugMessage(() => $"Relationship With Ped Bray->Me: {_theBray.GetRelationshipWithPed(Game.Player.Ped)}\n");
				//AddDebugMessage(() => $"Relationship With Ped Me->Bray: {Game.Player.Ped.GetRelationshipWithPed(_theBray)}\n");
				//AddDebugMessage(() => $"Ped Ids: {PLAYER.PLAYER_PED_ID()}, {Game.Player.Ped.Handle}\n");
				//AddDebugMessage(() => $"Bray Handle: {_theBray.Handle}\n");
				AddDebugMessage(() => $"In Combat: {PED.IS_PED_IN_COMBAT(_theBray.Handle, PLAYER.PLAYER_PED_ID())}\n");
				AddDebugMessage(() => $"Position: {_theBray.Position.X}, {_theBray.Position.Y}, {_theBray.Position.Z}\n");
				AddDebugMessage(() => $"Distance: {MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, _theBray.Position, false)}");
				if (_corpseBombAt > 0) { AddDebugMessage(() => $"Bomb in {_corpseBombAt - Game.GameTime}\n"); }


				if (_theBray.IsDead && _corpseBombAt == 0) {
					_corpseBombAt = Game.GameTime + _corpseBombTimer;
				}
				if (Game.GameTime >= _corpseBombAt && _corpseBombAt > 0) {
					//27 Explode and body burns
					World.AddExplosion(_theBray.Position, 27, 15f, 1.5f);
					_theBray = null;
					_corpseBombAt = 0;
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

			if (e.KeyCode == Keys.F13 && _theBray == null) {
				CreateBray();
			}

			if (e.KeyCode == Keys.F14) { }

			if (e.KeyCode == Keys.F16) {
				Hunt(300);
			}

			if (e.KeyCode == Keys.F15) {
				ComeToDaddy(3);
			}

			if (e.KeyCode == Keys.F18) {
				EndTheHate();
			}

			if (e.KeyCode == Keys.F17) {
				Skedaddle();
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

		public void ComeToDaddy(int maxDistance) {
			_theBray.Position = new Vector3 {
				X = Game.Player.Ped.Position.X + rand.Next(-1 * maxDistance, maxDistance),
				Y = Game.Player.Ped.Position.Y + rand.Next(-1 * maxDistance, maxDistance),
				Z = -200
			};
		}

		public void CreateBray() {

			var spawnPoint = new Vector3 {
				X = Game.Player.Ped.Position.X + rand.Next(-50, 50),
				Y = Game.Player.Ped.Position.Y + rand.Next(-50, 50),
				Z = -200
			};
			//log.Add($"Player Position: {Game.Player.Ped.Position.X}, {Game.Player.Ped.Position.Y}, {Game.Player.Ped.Position.Z}");
			//log.Add($"Spawn Position: {spawnPoint.X}, {spawnPoint.Y}, {spawnPoint.Z}");


			_theBray = World.CreatePed(PedHash.cs_aberdeenpigfarmer, spawnPoint, 0);
			_theBray.RelationshipGroup = _braylationship;
			SetBrayMaxHealth();
			_theBray.AddBlip(BlipType.BLIP_STYLE_NEUTRAL);

			//PED.SET_PED_AS_GROUP_MEMBER(_theBray.Handle, _playerGroup);

			Hunt(300);

		}

		public void Hunt(float searchRadius) {
			World.SetRelationshipBetweenGroups(eRelationshipType.Hate, _braylationship, Game.Player.Ped.RelationshipGroup);
			PED.REGISTER_TARGET(_theBray.Handle, Game.Player.Ped.Handle, false);
			TASK.TASK_COMBAT_HATED_TARGETS_AROUND_PED(_theBray.Handle, searchRadius, 33554432, 16);
			var blip = _theBray.GetBlip;
			MAP._BLIP_SET_STYLE(blip, (uint)BlipType.BLIP_STYLE_ENEMY_SEVERE);
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

		public void AddDebugMessage(Func<string> message) {
			if (showDebug) {
				_debug += message();
			}
		}

	}


}
