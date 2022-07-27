using System;
using System.Collections;
using System.Collections.Generic;
namespace Roseworks
{
	public struct SState
	{
		public int State;
		public Type ComType;
		public int ComID;
		public bool TurnBased;
		public float StartTime;
		public static void Clear(ref SState obj)
		{
			obj.State = default;
			obj.ComType = null;
			obj.ComID = -1;
			obj.TurnBased = false;
			obj.StartTime = -1;
		}
		public static void Move(ref SState from, ref SState to)
		{
			to.State = from.State;
			to.ComType = from.ComType;
			to.ComID = from.ComID;
			to.TurnBased = from.TurnBased;
			to.StartTime = from.StartTime;
		}
	}
}