using System;
using System.Collections.Generic;
using System.Text;
using Roseworks;

namespace RoseworksTest
{
	public static class TestUtil
	{
		public static void Reset()
		{
			typeof(ECS).TypeInitializer?.Invoke(null, null);
			typeof(Input).TypeInitializer?.Invoke(null, null);
			typeof(FSM).TypeInitializer?.Invoke(null, null);
			typeof(Roseworks.Timer).TypeInitializer?.Invoke(null, null);
		}
		public static void Init()
		{
			Reset();
			MonoSimulator ms = new MonoSimulator();
			ECS.InitScene(ms);
		}
	}
}
