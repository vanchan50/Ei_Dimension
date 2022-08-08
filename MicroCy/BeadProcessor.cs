namespace DIOS.Core
{
	internal class BeadProcessor
	{
		public NormalizationSettings Normalization { get; } = new NormalizationSettings();
		private float _greenMin;
		private float _greenMaj;
		private readonly Device _device;
		private readonly ClassificationMap _classificationMap = new ClassificationMap();
		private CustomMap _map;

		public BeadProcessor(Device device)
		{
			_device = device;
		}

		public void SetMap(CustomMap map)
		{
			_map = map;
			_classificationMap.ConstructClassificationMap(_map);
		}

		public ProcessedBead CalculateBeadParams(in RawBead rawBead)
		{
			//The order of operations matters here
			AssignSensitivityChannels(in rawBead);
			var compensated = CalculateCompensatedCoordinates(in rawBead);
			var outBead = new ProcessedBead
			{
				cl1 = compensated.cl1,
				cl2 = compensated.cl2,
				//fsc = (float)Math.Pow(10, rawBead.fsc),
				region = (ushort)_classificationMap.ClassifyBeadToRegion(in rawBead),
				zone = (ushort)ClassifyBeadToZone(in rawBead),
				reporter = CalculateReporter(in rawBead)
			};
			return outBead;
		}

		private void AssignSensitivityChannels(in RawBead rawBead)
		{
			//greenMaj is the hi dyn range channel,
			//greenMin is the high sensitivity channel(depends on filter placement)
			if (_device.SensitivityChannel == HiSensitivityChannel.GreenB)
			{
				_greenMaj = rawBead.greenC;
				_greenMin = rawBead.greenB;
				return;
			}
			_greenMaj = rawBead.greenB;
			_greenMin = rawBead.greenC;
		}

		private (float cl0, float cl1, float cl2, float cl3) CalculateCompensatedCoordinates(in RawBead outbead)
		{
			var cl1comp = _greenMaj * _device.Compensation / 100;
			var cl2comp = cl1comp * 0.26f;
			return (
			  outbead.cl0,
			  outbead.cl1 - cl1comp,  //Compensation
			  outbead.cl2 - cl2comp,  //Compensation
			  outbead.cl3
			);
		}

		private float CalculateReporter(in RawBead rawBead)
		{
			var basicReporter = _greenMin > _device.HdnrTrans ? _greenMaj * _device.HDnrCoef : _greenMin;
			var scaledReporter = (basicReporter / _device.ReporterScaling);

			if (!Normalization.IsEnabled || rawBead.region == 0)
				return scaledReporter;

			var rep = _map.GetFactorizedNormalizationForRegion(rawBead.region);
			scaledReporter -= rep;
			if (scaledReporter < 0)
				return 0;

			return scaledReporter;
		}

		private int ClassifyBeadToZone(in RawBead bead)
		{
			if (!_map.CL0ZonesEnabled)
				return 0;
			//for the sake of robustness. Going from right to left;
			//checks if the value is higher than zone's left boundary.
			//if yes, no need to check other zones
			//check if it falls into the right boundary. else out of any zone
			for (var i = _map.zones.Count - 1; i < 0; i--)
			{
				var zone = _map.zones[i];
				if (bead.cl0 >= zone.Start)
				{
					if (bead.cl0 <= zone.End)
						return zone.Number;

					return 0;
				}
			}
			return 0;
		}
	}
}