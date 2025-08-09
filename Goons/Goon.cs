using Bray.Goons;
using PolkaUtilils;
using RDR2;
using RDR2.Math;
using RDR2.Native;
using System;

namespace Bray {
	public class Goon {
		public Goon FollowerOf { get; set; }
		public PedHash Model { get; set; }
		public Ped Ped { get; set; }
		public GoonTypes GoonType { get; set; }
		internal BlipType _blipType = BlipType.BLIP_STYLE_ENEMY_SEVERE;
		internal int _defaultMinSpawnDistance = 35;
		internal int _defaultMaxSpawnDistance = 65;
		
		internal Random rand = new Random();
		internal uint _braylastionshipGroup;
		public bool _truce = false;
		public string Name;

		//Set to true to drop the goon from any goon collections. Used after all post death shit is done.
		public bool CanRemove = false;
		public virtual void OnTick() {
			
		}
		public Goon(GoonTypes goonType, uint relationShipGroup) {
			GoonType = goonType;
			_braylastionshipGroup = relationShipGroup;
		}

		public Vector3 GetSpawnPoint(int minDistance, int maxDistance) {
			var x = Game.Player.Ped.IsInTrain ? rand.Next(1, 2) : rand.Next(minDistance, maxDistance);
			x = rand.Next(0, 2) == 0 ? x * -1 : x;
			var y = Game.Player.Ped.IsInTrain ? rand.Next(1, 2) : rand.Next(minDistance, maxDistance);
			y = rand.Next(0, 2) == 0 ? y * -1 : y;

			return new Vector3(
					Game.Player.Ped.Position.X + x,
					Game.Player.Ped.Position.Y + y,
					Game.Player.Ped.IsInTrain ? Game.Player.Ped.Position.Z : -200
				);
		}

		public void ActivateGoonCam() {
			CAM._FORCE_CINEMATIC_DEATH_CAM_ON_PED(Ped.Handle);
			RDR2.UI.Screen.StopAllEffects();
		}

		public void AddRelationshipGroup(uint relationShipGroup) {
			Ped.RelationshipGroup = relationShipGroup;
		}

		public bool IsInCombat() {
			return PED.IS_PED_IN_COMBAT(Ped.Handle, PLAYER.PLAYER_PED_ID());
		}

		public virtual void SetBlip(BlipType blipStyle = BlipType.BLIP_STYLE_ENEMY_PASSIVE_THREAT) {
			var blip = Ped.GetBlip;
			MAP._BLIP_SET_STYLE(blip, (uint)blipStyle);
		}

		public virtual void Hunt(float searchRadius) { }
		public void Truce() {
			_truce = true;
			World.SetRelationshipBetweenGroups(eRelationshipType.Like, _braylastionshipGroup, Game.Player.Ped.RelationshipGroup);
			TASK.CLEAR_PED_TASKS_IMMEDIATELY(Ped.Handle, true, true);
			SetBlip(BlipType.BLIP_STYLE_NEUTRAL);
		}


		//Relocate the ped to a random aread around the player
		public void Relocate(int minDistance, int maxDistance) {
			Ped.Position = GetSpawnPoint(minDistance, maxDistance);
		}

		public void Skedaddle() {
			Truce();
			PED.REMOVE_RELATIONSHIP_GROUP(_braylastionshipGroup);
			Ped.Task.WanderAround();
			Ped.GetBlip.Delete();
			CanRemove = true;
		}
	}
}
