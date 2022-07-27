using System;
using System.Collections;
using System.Collections.Generic;
using RelaStructures;

namespace Roseworks
{
	public static class FSM
	{
		public static int DefaultState = default;
		public const int InvalidState = -1;

		// Rules are associated with states by ComType. All states have the same set of rules
		public static StructReArray<SState> States = new StructReArray<SState>(256, 1024, SState.Clear, SState.Move);
		public static StructReArray<SRule> Rules = new StructReArray<SRule>(256, 1024, SRule.Clear, SRule.Move);
		public static StructReArray<SJoin> Joins = new StructReArray<SJoin>(65536, 1048576, SJoin.Clear, SJoin.Move);
		public static int CurrentTurn = 0;

		[Flags]
		public enum EActive : int
		{
			None = 0,
			From = 1 << 0,
			FromTo = 1 << 1,
			EndCndFrom = 1 << 2,
			EndCndFromTo = 1 << 3,
			EndCbFromTo = 1 << 4,
		}

		#region Callback Preallocation (WIP)
		static List<Action<int, int>> Callbacks;
		public enum ECallback : int { };
		static List<Func<bool>> CallbackCnds;
		public enum ECallbackCnds : int { };
		#endregion

		static bool DebugPrint = true;

		/// <returns>DataID of newly added FSM</returns>
		public static int AddState(Type comType, int entID, float time = -1, bool turnBased = false)
		{
			if (time < 0)
			{
				if (turnBased)
					time = CurrentTurn;
				else
					throw new ArgumentException("State must have time specified unless turn-based.");
			}
			Console.WriteLine("AddState: " + comType);
			int stateID = States.Request();
			States.AtId(stateID).ComType = comType;
			States.AtId(stateID).TurnBased = turnBased;
			int comID = ECS.AddComToEnt(comType, entID);
			ECS.Coms.AtId(comID).DataID = stateID;
			ChangeState(time, stateID, DefaultState);
			return stateID;
		}
		public static int AddStateTurnBased(Type comType, int entID)
		{
			return AddState(comType, entID, CurrentTurn);
		}
		public static (int ruleID, int timerID) AddRule (SRule ruleData, float time)
		{
			int ruleID = Rules.Request();
			ref SRule rule = ref Rules.AtId(ruleID);
			SRule.Move(ref ruleData, ref rule);
			int timerID = TryAddJoin(ref rule, time, ruleID);
			return (ruleID, timerID);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rule"></param>
		/// <param name="time"></param>
		/// <param name="ruleID"></param>
		/// <returns>TimerID for newly-created timer. If no corresponding state found or state is turn-based, returns -1.</returns>
		private static int TryAddJoin(ref SRule rule, float time, int ruleID)
		{
			int timerID = -1;
			for (int i = 0; i < States.Count; i++)
			{
				if (rule.ComType == States[i].ComType)
				{
					ref SState state = ref States[i];
					if (state.TurnBased == false && rule.Duration > 0 && rule.To >= 0)
						InitJoinTimer(time, state.ComID, ruleID, out timerID);
					ref SJoin join = ref Joins.AtId(Joins.Request());
					join.ComID = state.ComID;
					join.RuleID = ruleID;
					join.StateID = States.IndicesToIds[i];
					join.TimerID = timerID;
					return timerID;
				}
			}
			return timerID;
		}
		private static bool ExitChangeState = false;
		// TODO: switch to to transition instead?
		public static void ChangeState(float time, int stateID, int to, int timerID = -1, bool ignoreSame = false)
		{
			ref SState state = ref States.AtId(stateID);

			if (Equals(state.State, to) && ignoreSame == true)
				return;

			ExitChangeState = false;

			UpdateActive(stateID);
			TryCallbacks(state.ComID, state.State, to);

			// if changestate runs again during this, exit
			if (ExitChangeState == true)
			{
				if (DebugPrint) Logger.WriteLine("Exiting ChangeState early. State was changed by callback");
				return;
			};
			Logger.WriteLine(state.ComType + ": " + state.State + " -> " + to);
			
			// timer handling
			if (state.TurnBased == false)
			{
				if (state.State == to && timerID >= 0)
				{
					Logger.Write("Loop timer " + timerID + ": " + DFormatTime(Timer.EndTime(timerID)) + " -> ");

					Timer.Loop(timerID);
					Logger.Write(DFormatTime(Timer.EndTime(timerID)) + ". ");
					Timer.CancelByComID(comID: state.ComID, ignoreID: timerID);
				}
				else
				{
					Timer.CancelByComID(state.ComID);
				}
				// Add timers
				for (int joinIx = 0; joinIx < Joins.Count; joinIx++)
				{
					ref SJoin join = ref Joins[joinIx];
					ref SRule rule = ref Rules.AtId(join.RuleID);

					if (join.TimerID < 0 && StateValid(rule.To) && join.ActiveChecks.HasFlag(EActive.From))
					{
						int tempTo = rule.To;
						void Callback(int timerID)
						{
							int lambdaWorkaroundDoNotRemove = tempTo;
							ChangeState(time, stateID: stateID, lambdaWorkaroundDoNotRemove, timerID: timerID);
						}
						Timer.Add(time, outTimerID: out join.TimerID, comID: join.ComID, duration: rule.Duration, callback: Callback, autoCancel: false);
					}
				}
			}
			state.State = to;
			state.StartTime = time;
			ExitChangeState = true;
		}
		public static string DFormatTime(float t)
		{
			return "@" + t.ToString("0.00");
		}
		/// <summary>
		/// Updates ID caches: ActiveIDFrom, ActiveIDFromTo, ActiveIDCndFrom, ActiveIDCndFromTo, ActiveIDCbFromTo
		/// </summary>
		/// <param name="from">The state from which FSM is transitioning.</param>
		/// <param name="to">The state to which FSM is transitioning. Optional: if not provided, UpdateActive will only update ActiveIDFrom.</param>
		private static void UpdateActiveForChangeState(int ruleDataID)
		{
			// same as UpdateActive but use to as from
		}

		private static void UpdateActive(int stateID, int to = -1)
		{

			// take in comid or dataid, update only that one based on current state

			// make alternate updateActives for other use cases
			ref SJoin join = ref Joins[0];
			ref SRule rule = ref Rules[0];

			for (int i = 0; i < Joins.Count; i++)
			{
				join = ref Joins[i];
				rule = ref Rules.AtId(join.RuleID);

				if (join.StateID == stateID)

				join.ActiveChecks = 0;

				if (StateEqualOrInvalid(rule.From, States.AtId(stateID).State))
				{
					join.ActiveChecks |= EActive.From;

					if (rule.EndCnd != null)
						join.ActiveChecks |= EActive.EndCndFrom;

					if (rule.To == to && StateValid(to) && StateValid(rule.To))
					{
						join.ActiveChecks |= EActive.FromTo;

						if (rule.EndCnd != null)
							join.ActiveChecks |= EActive.EndCndFromTo;
					}
				}
				if (StateValid(to) && StateEqualOrInvalid(rule.To, to))
					join.ActiveChecks |= EActive.EndCbFromTo;
			}
		}
		private static void UpdateActiveConditions(int stateDataID)
		{
		
		}

		private static bool TryCallbacks(int comID, int from, int to)
		{
			ref SRule rule = ref Rules[0];

			for (int i = 0; i < Joins.Count; i++)
			{
				if (Joins[i].ComID == comID)
				{
					rule = ref Rules[i];
					bool call = true;
					if (rule.CallbackCnd != null)
						call = rule.CallbackCnd.Invoke();
					if (call)
						rule.Callback?.Invoke(from, to);
				}
			}
			// if callbacks changed state, return true so we can cancel current ChangeState
			return (States.AtId(ECS.Coms.AtId(comID).DataID).State != from);
		}
		public static void TickConditionsRealtime(float time)
		{
			for (int i = 0; i < Joins.Count; i++)
			{
				ref SJoin join = ref Joins[i];
				if (States.AtId(join.StateID).TurnBased == false)
				{
					UpdateActiveConditions(States.IndicesToIds[i]);
					ref SRule rule = ref Rules.AtId(join.RuleID);
					if (
						join.TimerID < 0
						&& rule.EndCnd.Invoke() == true
						&& StateValid(rule.To))
					{
						ChangeState(time, stateID: States.IndicesToIds[i], rule.To);
					}
				}
			}
		}
		public static void TickConditionsTurnBased(int turn)
		{
			bool exit = false;
			for (int i = 0; i < Joins.Count; i++)
			{
				ref SJoin join = ref Joins[i];
				ref SState state = ref States.AtId(join.StateID);
				ref SRule rule = ref Rules.AtId(join.RuleID);
				Console.WriteLine("State " + join.StateID + ": " + state.State + "\tRule " + rule + ".From: " + rule.From);
				if (exit == false
					&& state.TurnBased == true
					&& state.State == rule.From)
				{
					//UpdateActiveConditions(States.IndicesToIds[i]);

					Console.WriteLine(
					"(CurrentTurn >= state.StartTime + rule.Duration)? " + ((CurrentTurn >= state.StartTime + rule.Duration) ? "true" : "false") + ": (" + CurrentTurn + " >= " + state.StartTime + " + " + rule.Duration + ")");

					bool endCndMet = true;
					if (rule.EndCnd != null && rule.EndCnd?.Invoke() == false)
						endCndMet = false;

					if ((CurrentTurn >= state.StartTime + rule.Duration)
						|| ( endCndMet && StateValid(rule.To)))
					{
						ChangeState(turn, stateID: States.IndicesToIds[i], rule.To);
						exit = true;
					}
				}
			}
		}
		public static void InitJoinTimer(float time, int comID, int ruleID, out int timerID)
		{
			ref SState state = ref States.AtId(ECS.Coms.AtId(comID).DataID);
			if (state.TurnBased == true)
			{
				timerID = -1;
				return;
			}
			ref SRule rule = ref Rules.AtId(ruleID);
			void Callback(int timerID)
			{
				ref SRule ruleInCb = ref Rules.AtId(ruleID);
				if (ruleInCb.EndCnd == null && ruleInCb.EndCnd?.Invoke() == true)
					ChangeState(time, stateID: ECS.Coms.AtId(comID).DataID, ruleInCb.To);
			}
			if (rule.To == state.State)
				Timer.Add(time, out timerID, comID: comID, rule.Duration, state.StartTime, Callback, false);
			else
				timerID = -1;
		}
		private static bool StatesEqualAndValid(int state1, int state2)
		{ 
			return (state1 == state2 && StateValid(state1) && StateValid(state2));
		}
		private static bool StateEqualOrInvalid(int state1, int state2)
		{
			return state1 == state2 || state1 == InvalidState;
		}
		private static bool StatesEqual(int state1, int state2, bool invalidOk1, bool invalidOk2)
		{
			if (state1 == state2 && state1 != InvalidState)
				return true;
			bool output = true;
			output &= state1 != InvalidState || (invalidOk1 && (state1 == InvalidState));
			output &= state2 != InvalidState || (invalidOk2 && (state2 == InvalidState));
			// breaks when state 1 and 2 are valid but different
			return output;
		}
		private static bool StateValid(int state)
		{
			return state != InvalidState;
		}
		public static void Invoke(ECallback e)
		{

		}
		public static void AdvanceTurn(int turns = 1)
		{
			CurrentTurn += turns;
			TickConditionsTurnBased(CurrentTurn);
		}
	}
}
