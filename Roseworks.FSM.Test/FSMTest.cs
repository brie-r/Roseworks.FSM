using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Roseworks;

namespace RoseworksTest
{
	[TestClass]
	public class FSMTest
	{
		SRule RuleAB = new SRule(typeof(FSMExample), from: (int) FSMExample.States.A, to: (int) FSMExample.States.B, duration: 1);
		SRule RuleBC = new SRule(typeof(FSMExample), from: (int)FSMExample.States.B, to: (int)FSMExample.States.C, duration: 2);
		SRule RuleCD = new SRule(typeof(FSMExample), from: (int)FSMExample.States.C, to: (int)FSMExample.States.D, endCnd: EndCnd);

		public static bool EndCnd()
		{
			return true;
		}
		public int InitTurnBased()
		{
			Logger.LogLineCallback = Console.WriteLine;
			Logger.LogCallback = Console.Write;
			TestUtil.Init();
			ECS.AddBehavior<FSMExample>();
			int entID = ECS.AddEnt();
			int dataID = FSM.AddState(typeof(FSMExample), entID, turnBased: true);
			FSM.AddRule(RuleAB, 0);
			FSM.AddRule(RuleBC, 0);
			FSM.AddRule(RuleCD, 0);
			return dataID;
		}
		[TestMethod]
		public void TestInitTurnBased()
		{
			int dataID = InitTurnBased();
			Assert.AreEqual(3, FSM.Joins.Count);
			Assert.AreEqual((int)FSMExample.States.A, FSM.States.AtId(dataID).State);
			// check that joins, states, and rules match up
			for (int i = 0; i < FSM.Rules.Count; i++)
			{
				Console.WriteLine("FSM.Rules[i].From == i? " + FSM.Rules[i].From + (FSM.Rules[i].From == i ? " == " : " != ") + i);
				Assert.AreEqual(i, FSM.Rules[i].From);
			}
			for (int i = 0; i < FSM.Joins.Count; i++)
			{
				Console.WriteLine("FSM.Joins[i].StateID == 0? " + FSM.Joins[i].StateID + (FSM.Joins[i].StateID == 0 ? " == " : " != ") + 0);
				Assert.AreEqual(0, FSM.Joins[i].StateID);
				Assert.AreEqual(i, FSM.Joins[i].RuleID);
				Assert.AreEqual(-1, FSM.Joins[i].TimerID);
			}
		}
		[TestMethod]
		public void TestAB()
		{
			int dataID = InitTurnBased();
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.B, FSM.States.AtId(dataID).State);
		}
		[TestMethod]
		public void TestAC()
		{
			int dataID = InitTurnBased();
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.B, FSM.States.AtId(dataID).State);
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.B, FSM.States.AtId(dataID).State);
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.C, FSM.States.AtId(dataID).State);
		}
		[TestMethod]
		public void TestAD()
		{
			int dataID = InitTurnBased();
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.B, FSM.States.AtId(dataID).State);
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.B, FSM.States.AtId(dataID).State);
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.C, FSM.States.AtId(dataID).State);
			FSM.AdvanceTurn(1);
			Assert.AreEqual((int)FSMExample.States.D, FSM.States.AtId(dataID).State);
		}
	}
}
