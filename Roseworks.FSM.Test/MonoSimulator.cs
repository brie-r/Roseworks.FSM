using System;
using System.Collections.Generic;
using System.Text;
using Roseworks;

namespace RoseworksTest
{
	public class MonoSimulator : IMono
	{
		public object[] GetEditorEnts()
		{
			return new object[] { };
		}
		public Behavior Instantiate(object parent, Type behavior)
		{
			Behavior b = (Behavior) Activator.CreateInstance(behavior);
			return b;
		}
	}
}
