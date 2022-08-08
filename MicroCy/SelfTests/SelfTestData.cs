using System;

namespace DIOS.Core.SelfTests
{
	public class SelfTestData
	{
		internal bool ResultReady
		{
			get
			{
				return _pressure != null
					   && _startupPressure != null
					   && _motorX != null
					   && _motorY != null
					   && _motorZ != null;
			}
		}

		public float? StartupPressure
		{
			get
			{
				if (_startupPressure > 1.0)
					return _startupPressure;
				return null;
			}
			private set
			{
				_startupPressure = value;
				Console.WriteLine($"Startup pressure: {value}");
			}
		}

		public float? Pressure
		{
			get
			{
				if (_pressure > _device.MaxPressure)
					return _pressure;
				return null;
			}
			private set
			{
				_pressure = value;
				Console.WriteLine($"Pressure: {value}");
			}
		}

		public float? MotorX
		{
			get
			{
				if (_motorX < 450 || _motorX > 505)
					return _motorX;
				return null;
			}
			set
			{
				_motorX = value;
				Console.WriteLine($"Motor X: {value}");
			}
		}

		public float? MotorY
		{
			get
			{
				if (_motorY < 450 || _motorY > 505)
					return _motorY;
				return null;
			}
			set
			{
				_motorY = value;
				Console.WriteLine($"Motor Y: {value}");
			}
		}

		public float? MotorZ
		{
			get
			{
				if (_motorZ < 16 || _motorZ > 21)
					return _motorZ;
				return null;
			}
			set
			{
				_motorZ = value;
				Console.WriteLine($"Motor Z: {value}");
			}
		}

		private float? _motorX;
		private float? _motorY;
		private float? _motorZ;
		private float? _pressure;
		private float? _startupPressure;
		private bool _startup = true;
		private Device _device;

		internal SelfTestData(Device device)
		{
			_device = device;
		}

		internal void SetPressure(float pressure)
		{
			if (_startup)
			{
				StartupPressure = pressure;
				_startup = false;
				return;
			}
			Pressure = pressure;
		}

	}
}
