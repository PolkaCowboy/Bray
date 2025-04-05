using RDR2;
using RDR2.Math;
using RDR2.Native;
using RDR2.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bray {
	public class Bray : Script {
		private string _debug;
		private bool showDebug = true;
		private List<Keys> pressedKeys = new List<Keys>();
		private string _brayString = "CS_AberdeenPigFarmer";
		private List<string> log = new List<string>();
		Random rand = new Random();
		Ped _theBray = null;
		uint _braylationship = 0;


		public Bray() {

			//World.SetRelationshipBetweenGroups(eRelationshipType.Hate, playerGroup, _braylationship);


			Tick += OnTick;
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			Interval = 1;
		}

		private void OnTick(object sender, EventArgs evt) {
			_debug = string.Empty;



			if (_theBray != null) {
				AddDebugMessage(() => $"Relationship Group: {_theBray.RelationshipGroup}\n");
				AddDebugMessage(() => $"Relationship With Ped Bray->Me: {_theBray.GetRelationshipWithPed(Game.Player.Ped)}\n");
				AddDebugMessage(() => $"Relationship With Ped Me->Bray: {Game.Player.Ped.GetRelationshipWithPed(_theBray)}\n");
				AddDebugMessage(() => $"Ped Ids: {PLAYER.PLAYER_PED_ID()}, {Game.Player.Ped.Handle}\n");
				AddDebugMessage(() => $"Bray Handle: {_theBray.Handle}\n");
				AddDebugMessage(() => $"In Combat: {PED.IS_PED_IN_COMBAT(_theBray.Handle, PLAYER.PLAYER_PED_ID())}\n");
			} else {
				AddDebugMessage(() => "There is no Bray");
			}

			foreach (var l in log) {
				AddDebugMessage(() => l.ToString() + "\n");
			}

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
			if (e.KeyCode == Keys.F14) {
				log = new List<string>();
				if(_theBray == null) { 
					_theBray = World.GetClosestPed(Game.Player.Ped.Position);
				}
				Settings.SetValue("Health", "Points", _theBray.Attributes.Health.Points);
				Settings.SetValue("Health", "Max Points", _theBray.Attributes.Health.MaxPoints);
				Settings.SetValue("Health", "Max Rank", _theBray.Attributes.Health.MaxRank);


				for (int i = 0; i <= 70; i++) {
					Settings.SetValue("Combat Float", i.ToString(), PED.GET_COMBAT_FLOAT(_theBray, i));
				}
				foreach (ePedScriptConfigFlags flag in Enum.GetValues(typeof(ePedScriptConfigFlags))) {
					Settings.SetValue("Config Flags", flag.ToString(), _theBray.GetConfigFlag(flag));
				}

				for (int i = 0; i <= 134; i++) {
					Settings.SetValue("Combat Attributes", i.ToString(), PED._GET_PED_COMBAT_ATTRIBUTE(_theBray.Handle, i));
				}

				Settings.SetValue("Relationship", "Relationship", _theBray.GetRelationshipWithPed(Game.Player.Ped));

				Settings.Save();
				RDR2.UI.Screen.DisplaySubtitle($"Saved?");
			}

			if (e.KeyCode == Keys.F16) {
				
				//TASK.TASK_COMBAT_HATED_TARGETS(_theBray.Handle, 100);
				

			}
		}

		private void OnKeyUp(object sender, KeyEventArgs e) {
			pressedKeys.RemoveAll(p => p == e.KeyCode);
		}

		public void CreateBray() {

			_braylationship = World.AddRelationshipGroup($"Braylationship");
			var playerGroup = Game.Player.Ped.RelationshipGroup;


			_theBray = World.CreatePed(PedHash.cs_aberdeenpigfarmer, Game.Player.Ped.Position, 0);
			_theBray.RelationshipGroup = _braylationship;
			_theBray.AddBlip(BlipType.EnemyPink);
			World.SetRelationshipBetweenGroups(eRelationshipType.Hate, _braylationship, playerGroup);
			PED.REGISTER_TARGET(_theBray.Handle, Game.Player.Ped.Handle, false);
			TASK.TASK_COMBAT_HATED_TARGETS_AROUND_PED(_theBray.Handle, 100, 33554432, 16);
			//PED._SET_PED_COMBAT_STYLE(_theBray.Handle, MISC.GET_HASH_KEY("SituationEscalate_AttackClose"), 1, -1);
			//PED._SET_PED_COMBAT_STYLE_MOD(_theBray.Handle, MISC.GET_HASH_KEY("GetUpClose"), 1);
			//PED.SET_PED_COMBAT_MOVEMENT(_theBray.Handle, 3);

			//World.SetRelationshipBetweenGroups(eRelationshipType.Hate, _braylationship, playerGroup);
			//World.SetRelationshipBetweenGroups(eRelationshipType.Hate, playerGroup, _braylationship);w



		}

		public void AddDebugMessage(Func<string> message) {
			if (showDebug) {
				_debug += message();
			}
		}

	}


}
