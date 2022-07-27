using System;
using System.Collections;
using System.Collections.Generic;
namespace Roseworks
{
	public struct SJoin
	{
		public int StateID;
		public int RuleID;
		public int ComID;
		public int TimerID;
		public FSM.EActive ActiveChecks;
		public static void Clear(ref SJoin obj)
		{
			obj.StateID = -1;
			obj.RuleID = -1;
			obj.ComID = -1;
			obj.TimerID = -1;
			obj.ActiveChecks = 0;
		}
		public static void Move(ref SJoin from, ref SJoin to)
		{
			to.StateID = from.StateID;
			to.RuleID = from.RuleID;
			to.ComID = from.ComID;
			to.TimerID = from.TimerID;
			to.ActiveChecks = from.ActiveChecks;
		}
	}
}