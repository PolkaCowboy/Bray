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

namespace Bray.Goons {
	public class TheBray : Goon {

		private int _corpseBombTimerMax = 4000;
		private int _corpseBombTimerMin = 0;
		private float _corpseBombRadius = 0;
		private int _corpseBombAt = 0;

		public TheBray(GoonTypes GoonType) : base(GoonType) {
			Ped = World.CreatePed(PedHash.cs_aberdeenpigfarmer, GetSpawnPoint(3, 20), 0);
			Ped.SetPedPromptName(GetName());
		}
	}
}
