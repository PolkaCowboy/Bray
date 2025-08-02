using Bray.Goons;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Bray {
	public class Goon {
		public Goon FollowerOf { get; set; }
		public PedHash Model { get; set; }
		public Ped Ped { get; set; }
		public GoonTypes GoonType { get; set; }
		Random rand = new Random();
		public void OnTick() {

		}
		public Goon(GoonTypes goonType) {
			GoonType = goonType;
		}

		internal string GetName() {
			if (GoonType == GoonTypes.TheBray || GoonType == GoonTypes.StealthBray) {
				return "The Bray";
			}
			return "Stranger";
		}

		public Vector3 GetSpawnPoint(int minDistance, int maxDistance) {
			var x = rand.Next(minDistance, maxDistance);
			x = rand.Next(0, 2) == 0 ? x * -1 : x;
			var y = rand.Next(minDistance, maxDistance);
			y = rand.Next(0, 2) == 0 ? y * -1 : y;

			return new Vector3(
					Game.Player.Ped.Position.X + x,
					Game.Player.Ped.Position.Y + y,
					Game.Player.Ped.IsInTrain ? Game.Player.Ped.Position.Z : -200
				);
		}

		public void AddRelationshipGroup(uint relationShipGroup) {
			Ped.RelationshipGroup = relationShipGroup;
		}
	}
}
