using System;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace ThemeParkResourceViewer
{
	public class MMTimer //high precision timer (WM_TIMER is limited to 15 ms)
	{
		#region Native

		[DllImport("winmm.dll")]
		static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, uint fuEvent);

		[DllImport("winmm.dll")]
		static extern uint timeGetTime();

		[DllImport("winmm.dll")]
		static extern uint timeKillEvent(uint uTimerID);

		const uint TIME_CALLBACK_FUNCTION = 0x0000;
		const uint TIME_PERIODIC = 0x0001;
		const uint TIME_KILL_SYNCHRONOUS = 0x0100;

		delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

		#endregion

		readonly TimerCallback callbackFunction; //prevent GC collecting "new TimerCallback(CallbackFunction)"
		public event EventHandler Timer;
		uint timerId;

		public MMTimer()
		{
			callbackFunction = CallbackFunction;
		}

		public void Start()
		{
			if (timerId == 0)
			{
				timerId = timeSetEvent((uint)Interval, 0, callbackFunction, UIntPtr.Zero, TIME_CALLBACK_FUNCTION | TIME_KILL_SYNCHRONOUS | TIME_PERIODIC);
			}
		}

		public void Stop()
		{
			if (timerId != 0)
			{
				timeKillEvent(timerId);
				timerId = 0;
			}
		}

		public bool Enabled => timerId != 0;

		public int Interval;

		public uint GetTime()
		{
			return timeGetTime();
		}

		void CallbackFunction(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2)
		{
			Timer(this, EventArgs.Empty);
		}
	}
}
