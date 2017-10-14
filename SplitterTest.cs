using System;
using System.Threading;
namespace LiveSplit.Tinertia {
	public class SplitterTest {
		private static SplitterComponent comp = new SplitterComponent(null);
		public static void Main(string[] args) {
			Thread t = new Thread(GetVals);
			t.IsBackground = true;
			t.Start();
			System.Windows.Forms.Application.Run();
		}
		private static void GetVals() {
			while (true) {
				try {
					comp.GetValues();
					Thread.Sleep(5);
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					Thread.Sleep(1000);
				}
			}
		}
	}
}