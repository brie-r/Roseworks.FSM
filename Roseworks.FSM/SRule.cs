using System;
using System.Collections;
using System.Collections.Generic;
namespace Roseworks
{
	public struct SRule
	{
		/// <summary>For associating rules with states.</summary>
		public Type ComType;

		/// <summary>The state to switch from if EndCnd returns true. If -1, Callback "From" requirement automatically met.</summary>
		public int From;
		/// <summary>The state to switch to if EndCnd returns true. If -1, Callback "To" requirement automatically met, but state will never change to -1.</summary>
		public int To;

		/// <summary>Invoked when state changed, To/From requirements met, and CallbackCnd returns true or is null.</summary>
		public Action<int, int> Callback;
		public Func<bool> CallbackCnd;

		public float Duration;
		/// <summary>If From == State or -1, to != -1, and EndCnd returns true (or is null? double check), change state to To.</summary>
		public Func<bool> EndCnd;

		public bool Active;

		/* For callback preallocation
		public ECallback? Callback;
		public ECallbackCnds? CallbackCnd;
		*/

		public SRule(Type comType, int comID = -1, int from = -1, int to = -1, Action<int, int> callback = null, Func<bool> callbackCnd = null, float duration = -1, int timerID = -1, Func<bool> endCnd = null, FSM.EActive activeChecks = FSM.EActive.None, bool active = true)
		{
			ComType = comType;
			From = from;
			To = to;
			Callback = callback;
			CallbackCnd = callbackCnd;
			Duration = duration;
			EndCnd = endCnd;
			Active = active;
		}
		public static void Clear(ref SRule obj)
		{
			obj.ComType = null;

			obj.From = -1;
			obj.To = -1;

			obj.Callback = null;
			obj.CallbackCnd = null;

			obj.Duration = -1;

			obj.EndCnd = null;
			obj.Active = true;
		}
		public static void Move(ref SRule from, ref SRule to)
		{
			to.ComType = from.ComType;

			to.From = from.From;
			to.To = from.To;

			to.Callback = from.Callback;
			to.CallbackCnd = from.CallbackCnd;

			to.Duration = from.Duration;

			to.EndCnd = from.EndCnd;

			to.Active = from.Active;
		}
	}
}