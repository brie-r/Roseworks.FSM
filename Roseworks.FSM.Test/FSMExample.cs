using System;
using System.Collections;
using System.Collections.Generic;
using Roseworks;

public class FSMExample: Behavior
{
	public System.Type[] Dependencies { get; set; }
	public bool ShouldTick { get; set; } = false;
	public float Time;
	public enum States : byte {A, B, C, D, E, F };
	public void Init()
	{
	}
	public int InitCom(int comID, int entID)
	{
		return -1;
	}
	public bool TestCnd()
	{
		return true;
	}
}