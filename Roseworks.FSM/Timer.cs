using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Roseworks
{
	public static class Timer
	{
		static bool DebugPrint = true;

		static System.Diagnostics.StackTrace s = new System.Diagnostics.StackTrace();
		public static float CurrentTime = 0;
		public struct STimer
		{
			public int ComID;
			public float Duration;
			public float StartTime;
			/// <summary>
			/// Parameter is TimerID
			/// </summary>
			public System.Action<int> Callback;
			public bool AutoCancel;

			/*public STimer(int comID, float duration = 0, float startTime = 0, bool realTime = true, System.Action<int> callback = null, bool autoCancel = true)
			{
				ComID = comID;
				Duration = duration;
				StartTime = (startTime <= 0) ? GetTime : startTime;
				Callback = callback;
				AutoCancel = autoCancel;
				Realtime = realTime;
			}*/
			public void Setup(ref STimer timer, float time)
			{
				ComID = timer.ComID;
				Duration = timer.Duration;
				Callback = timer.Callback;
				AutoCancel = timer.AutoCancel;
				StartTime = (timer.StartTime <= 0) ? time : timer.StartTime;
			}
			public void Setup(float time, int comID, float duration = 0, float startTime = 0, bool realtime = true, System.Action<int> callback = null, bool autoCancel = true)
			{
				ComID = comID;
				Duration = duration;
				Callback = callback;
				AutoCancel = autoCancel;
				StartTime = (startTime <= 0) ? time : startTime;
			}
		}
		private static void ClearAction(ref STimer obj)
		{
			obj.ComID = -1;
			obj.Duration = 0;
			obj.StartTime = 0;
			obj.Callback = null;
			obj.AutoCancel = true;
		}
		private static void MoveAction(ref STimer from, ref STimer to)
		{
			to.ComID = from.ComID;
			to.Duration = from.Duration;
			to.StartTime = from.StartTime;
			to.Callback = from.Callback;
			to.AutoCancel = from.AutoCancel;
		}

		public static RelaStructures.StructReArray<STimer> Timers =
			new RelaStructures.StructReArray<STimer>(100, 1000, ClearAction, MoveAction);

		public static void Update(float time, float dt)
		{
			// Check for expired timers
			// Loops backwards to prevent invalid indices after calling ReturnIx()
			int i = Timers.Count - 1;
			while (i >= 0)
			{
				ref STimer timer = ref Timers[i];
				if (time > timer.StartTime + timer.Duration - dt / 2f)
				{
					int timerCount = Timers.Count;
					timer.Callback?.Invoke(Timers.IndicesToIds[i]);
					if (timerCount != Timers.Count)
						i = Timers.Count;
					else
					{
						Logger.Write(DFormatTime(time) + "\tTimer ended: " + Timers.IndicesToIds[i] + " entID: " + timer.ComID);
						Logger.Write((timer.Callback == null ? "\tNo callback invoked" : ("\tCallback invoked: " + timer.Callback.Method)));
						if (timer.AutoCancel == true)
							Timers.ReturnIndex(i);
					}
				}
				i--;
			}
		}
		public static void Add(out int outTimerID, STimer timer, float time)
		{
			outTimerID = Timers.Request();
			Timers.AtId(outTimerID).Setup(ref timer, time);
			Logger.Write("Timer added: id " + outTimerID + ", entID " + timer.ComID + ", time " + DFormatTime(timer.StartTime) + " - " + DFormatTime(timer.StartTime + timer.Duration));
		}
		public static void Add(float time, out int outTimerID, int comID, float duration, float startTime = -1, System.Action<int> callback = null, bool autoCancel = true)
		{
			outTimerID = Timers.Request();
			Timers.AtId(outTimerID).Setup(time, comID, duration, startTime: startTime, callback: callback, autoCancel: autoCancel);
			Logger.Write("Timer added: id " + outTimerID + ", entID " + comID + ", " + DFormatTime(startTime) + " - " + DFormatTime(startTime+duration));
		}
		public static void Add(int[] outTimerIDs, STimer[] timers, float time)
		{
			// check timerIDs.Length == timer.Length
			for (int i = 0; i < timers.Length; i++)
			{
				outTimerIDs[i] = Timers.Request();
				Timers.AtId(outTimerIDs[i]).Setup(ref timers[i], time);
				Logger.WriteLine("Timer added: id " + outTimerIDs[i] + ", entID " + timers[i].ComID + ", " + DFormatTime(timers[i].StartTime) + " - " + DFormatTime(timers[i].StartTime + timers[i].Duration));
			}
		}
		public static void Add(float time, ref int[] outTimerIDs, int[] comID, float[] duration, float[] startTime, System.Action<int>[] callback = null, BitArray autoCancel = null)
		{
			// check timerIDs.Length == timer.Length
			startTime ??= new float[comID.Length];
			for (int i = 0; i < comID.Length; i++)
			{
				outTimerIDs[i] = Timers.Request();
				if (autoCancel == null)
				{
					autoCancel = new BitArray(comID.Length);
					autoCancel.SetAll(true);
				}
				Timers.AtId(outTimerIDs[i]).Setup(time, comID[i], duration[i], startTime: startTime[i], callback: callback[i], autoCancel: autoCancel[i]);
				Logger.WriteLine("Timer added: id " + outTimerIDs[i] + ", entID " + comID[i] + ", " + DFormatTime(startTime[i]) + " - " + DFormatTime(startTime[i] + duration[i]));
				
			}
		}
		public static int[] AddTimers(List<int> comID, float time, List<float> duration, List<float> startTime, List<System.Action<int>> callback = null, BitArray autoCancel = null)
		{
			int[] timerIDs = new int[comID.Count];
			if (startTime == null)
			{
				startTime = new List<float>(comID.Count);
			}
			for (int i = 0; i < startTime.Count; i++)
			{
				timerIDs[i] = Timers.Request();
				if (autoCancel == null)
				{
					autoCancel = new BitArray(comID.Count);
					autoCancel.SetAll(true);
				}
				Timers.AtId(timerIDs[i]).Setup(time, comID[i], duration[i], startTime: startTime[i], callback: callback[i], autoCancel: autoCancel[i]);
				Logger.WriteLine("Timer added: id " + timerIDs[i] + ", entID " + comID[i] + ", " + DFormatTime((float)startTime[i]) + " - " + DFormatTime((float)startTime[i] + duration[i]));
				
			}
			return timerIDs;
		}
		public static void CancelByComID(int comID, int ignoreID = -1)
		{
			Logger.WriteLine("\tCancel timers by comID: " + comID + ", ignore timerID " + ignoreID);
			for (int i = Timers.Count - 1; i >= 0; i--)
			{
				Logger.Write("Cancel " + Timers.IndicesToIds[i] + "? ");
				Logger.Write((Timers[i].ComID == comID) + " + " + (Timers.IndicesToIds[i] != ignoreID) + "\t");
				if (Timers[i].ComID == comID && Timers.IndicesToIds[i] != ignoreID)
					Timers.ReturnIndex(i);
			}
			
		}
		public static void CancelByTimerID(int timerID, int ignoreID = -1)
		{
			if (DValidTimerID(timerID) == false) return;

			if (timerID != ignoreID)
				Timers.ReturnId(timerID);
		}
		public static void CancelByIx(int timerIx, int ignoreID = -1)
		{
			if (DValidTimerIx(timerIx) == false) return;

			if (DValidTimerID(ignoreID) == false || timerIx != Timers.IdsToIndices[ignoreID])
				Timers.ReturnIndex(timerIx);
		}
		public static float StartTime(int timerID)
		{
			if (DValidTimerID(timerID) == false) return -1;

			ref STimer timer = ref Timers.AtId(timerID);
			return timer.StartTime;
		}
		public static float EndTime(int timerID)
		{
			if (DValidTimerID(timerID) == false) return -1;

			ref STimer timer = ref Timers.AtId(timerID);
			return timer.StartTime + timer.Duration;
		}
		public static float Elapsed(int timerID, float time)
		{
			if (DValidTimerID(timerID) == false) return -1;

			ref STimer timer = ref Timers.AtId(timerID);
			return time - timer.StartTime;
		}
		public static float ElapsedRatio(int timerID, float time)
		{
			if (DValidTimerID(timerID) == false) return -1;

			ref STimer timer = ref Timers.AtId(timerID);
			return Elapsed(timerID, time) / timer.Duration;
		}
		public static float Remaining(int timerID, float time)
		{
			if (DValidTimerID(timerID) == false) return -1;

			ref STimer timer = ref Timers.AtId(timerID);
			return timer.Duration - (time - timer.StartTime);
		}
		public static float RemainingRatio(int timerID, float time)
		{
			if (DValidTimerID(timerID) == false) return -1;

			ref STimer timer = ref Timers.AtId(timerID);
			return Remaining(timerID, time) / timer.Duration;
		}
		public static void Loop(int timerID)
		{
			if (DValidTimerID(timerID) == false) return;

			ref STimer timer = ref Timers.AtId(timerID);
			timer.StartTime += timer.Duration;
		}
		// debug print helper: adds @, rounds to 2 decimals on floats
		public static string DFormatTime(float time)
		{
			return "@" + ((float) time).ToString("0.00");
		}
		// checks validity of timer id, prints helpful error message
		private static bool DValidTimerID(int timerID)
		{
			if (timerID < 0)
			{
				Logger.WriteLine(s.GetFrame(1).GetMethod().Name + " Timer with id " + timerID + " not found.");
				return false;
			}
			return true;
		}
		// checks validity of timer index, prints helpful error message
		private static bool DValidTimerIx(int timerIx)
		{
			if (timerIx < 0 || timerIx >= Timers.Length)
			{
				Logger.WriteLine(s.GetFrame(1).GetMethod().Name + " Timer with index " + timerIx + " not found.");
				return false;
			}
			return true;
		}
	}
}