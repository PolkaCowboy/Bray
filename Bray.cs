using Bray.Goons;
using PolkaUtilils;
using RDR2;
using RDR2.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Bray {
	public class Bray : Script {
		private PolkaUtililty _utils = new PolkaUtililty();
		Random rand = new Random();
		TheBray _theBray = null;
		private IList<Goon> _goons = new List<Goon>();
		uint _braylationship = 0;
		private bool _brayCanSpawn = true;
		private BattleModes _currentBattleMode = BattleModes.Regular;

		private int _braytosisCount = 0;
		//Prevent game crashes
		private int _hardGoonLimit = 14;

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
			Interval = 3;
		}

		private void OnTick(object sender, EventArgs evt) {
			_utils.ClearDebug();

			if (_nextStealthSpawn <= 0) {
				_nextStealthSpawn = Game.GameTime + rand.Next(_stealthSpawnMinMinutes * 60000, _stealthSpawnMaxMinutes * 60000);
			}

			_utils.AddDebugMessage(() => $"Game Time: {Game.GameTime}\n");
			_utils.AddDebugMessage(() => $"Next Stealth Spawn: {_nextStealthSpawn}\n");
			_utils.AddDebugMessage(() => $"Stealth Spawn In: {(_nextStealthSpawn - Game.GameTime) / 600}\n");

			_utils.AddDebugMessage(() => $"In Mission: {MISC.GET_MISSION_FLAG()}\n");
			_utils.AddDebugMessage(() => $"In Combat: {Game.Player.Ped.IsInCombat}\n");
			//AddDebugMessage(() => $"Player On Train: {Game.Player.Ped.IsInTrain}\n");

			if (_theBray == null && _brayCanSpawn) {
				if (MISC.GET_MISSION_FLAG()) {
					_theBray = new TheBray(GoonTypes.TheBray, _braylationship, _braytosisCount++);
				} else if (Game.Player.IsWanted) {
					_theBray = new TheBray(GoonTypes.LawBray, _braylationship);
				} else if (Game.GameTime > _nextStealthSpawn && !MISC.GET_MISSION_FLAG()) {
					_theBray = new TheBray(GoonTypes.StealthBray, _braylationship);
				}
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


				_utils.AddDebugMessage(() => $"Bray In Combat: {PED.IS_PED_IN_COMBAT(_theBray.Ped.Handle, PLAYER.PLAYER_PED_ID())}\n");
				_utils.AddDebugMessage(() => $"Position: {_theBray.Ped.Position.X}, {_theBray.Ped.Position.Y}, {_theBray.Ped.Position.Z}\n");



				if (_utils.pressedKeys.Contains(Keys.F10)) {
					_theBray.ActivateGoonCam();
				}

				_theBray.OnTick();

				if (_theBray.Ped.IsAlive) {
					_utils.AddDebugMessage(() => $"Bray is Alive - {_theBray.Name} - ({MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, _theBray.Ped.Position, false).ToString("0.0")}) Combat: {_theBray.IsInCombat()} {_theBray.Ped.RelationshipGroup}\n");
				} else {
					_utils.AddDebugMessage(() => $"Bray is Dead\n");
				}


				if (_theBray.CanRemove) {
					_theBray = null;
					//Add one to Goon stack if The Bray goes down during mitosis. 2nd will respawn as The Bray
					if (_currentBattleMode == BattleModes.Braytosis && _goons.Count < _hardGoonLimit) {
						_goons.Add(new TheBray(GoonTypes.TheBray, _braylationship, _braytosisCount++, 2));
					}
				}

			} else {
				_utils.AddDebugMessage(() => "There is no Bray\n");
			}

			_utils.AddDebugMessage(() => $"{_goons.Count} Goons Alive\n");
			if (_goons.Any()) {
				foreach (var goon in _goons.Where(g => g.CanRemove).ToList()) {
					if (goon.GoonType == GoonTypes.TheBray && _currentBattleMode == BattleModes.Braytosis && (goon as TheBray).BraytosisLevel > 0) {
						if (_goons.Count < _hardGoonLimit - 1) {
							_goons.Add(new TheBray(GoonTypes.TheBray, _braylationship, _braytosisCount++, (goon as TheBray).BraytosisLevel - 1));
							_goons.Add(new TheBray(GoonTypes.TheBray, _braylationship, _braytosisCount++, (goon as TheBray).BraytosisLevel - 1));
						}
					}
					_goons.Remove(goon);
				}

				foreach (var g in _goons) {
					g.OnTick();
					_utils.AddDebugMessage(() => $"{g.Name} ({MISC.GET_DISTANCE_BETWEEN_COORDS(Game.Player.Ped.Position, g.Ped.Position, false).ToString("0.0")}) Combat: {g.IsInCombat()}, Lvl {(g as TheBray).BraytosisLevel}\n");
				}
			}


			_utils.ShowDebugMessage();
		}
		private void OnKeyDown(object sender, KeyEventArgs e) {
			_utils.LogPressedKey(e);

			/* Debug Functions */

			if (e.KeyCode == Keys.F13 && _theBray == null) {
				_theBray = new TheBray(GoonTypes.TheBray, _braylationship);
			}

			//if (e.KeyCode == Keys.F13 && _theBray == null) {
			//	_goons.Add(new TheBray(GoonTypes.TheBray, _braylationship, _braytosisCount++));
			//}

			if (e.KeyCode == Keys.F15) {
				_theBray.Relocate(5, 10);
			}

			if (e.KeyCode == Keys.F19) {
				_utils.ToggleDebug();
			}

			/* Mod Tools */

			if (e.KeyCode == Keys.F14) {
				ToggleBray();
			}

			if (e.KeyCode == Keys.F16) {
				_theBray.Hunt(300);
				foreach (var g in _goons) { g.Hunt(300); }
			}

			if (e.KeyCode == Keys.F18) {
				EndTheHate();
			}

			if (e.KeyCode == Keys.F17) {
				_theBray.Skedaddle();
				foreach (var g in _goons) { g.Skedaddle(); }
			}

			if (e.KeyCode == Keys.F20) {
				//World.AddExplosion(Game.Player.Ped.Position, (int)ExplosionTypes.BigSloMo, 5f, 1.5f);
			}

			if (e.KeyCode == Keys.F21) {
				if (_currentBattleMode == BattleModes.Braytosis) {
					_currentBattleMode = BattleModes.Regular;
					_braytosisCount = 0;
					RDR2.UI.Screen.DisplaySubtitle($"Braytosis Deactivated.");
				} else {
					_currentBattleMode = BattleModes.Braytosis;
					_braytosisCount = 1;
					RDR2.UI.Screen.DisplaySubtitle($"Braytosis Activated.");
				}
			}

		}

		private void OnKeyUp(object sender, KeyEventArgs e) {
			_utils.UnpressKey(e);
		}

		public void EndTheHate() {
			_theBray.Truce();
			foreach (var g in _goons) { g.Truce(); }
		}

		public void ToggleBray() {
			_brayCanSpawn = !_brayCanSpawn;
			if (_brayCanSpawn) {
				RDR2.UI.Screen.DisplaySubtitle($"Bray can spawn.");
			} else {
				RDR2.UI.Screen.DisplaySubtitle($"Bray can NOT spawn.");
			}
		}

	}


}
