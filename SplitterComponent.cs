#if !Info
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Tinertia {
#if !Info
	public class SplitterComponent : IComponent {
		public TimerModel Model { get; set; }
#else
	public class SplitterComponent {
#endif
		public string ComponentName { get { return "Tinertia Autosplitter"; } }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private static string LOGFILE = "_Tinertia.log";
		internal static string[] keys = { "CurrentSplit", "State", "Loading", "Paused", "Mode", "World", "Level", "Checkpoint", "Complete", "TimerState", "LevelSelect" };
		private SplitterMemory mem;
		private int currentSplit = -1, state = 0, lastLogCheck = 0;
		private bool hasLog = false;
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
#if !Info
		private bool lastComplete, lastCheckpoint, lastWorldFinished, lastShowLevelSelect;
		public SplitterComponent(LiveSplitState state) {
#else
		public SplitterComponent() {
#endif
			mem = new SplitterMemory();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

#if !Info
			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;
			}
#endif
		}

		public void GetValues() {
			if (!mem.HookProcess()) { return; }

#if !Info
			if (Model != null) {
				HandleSplits();
			}
#endif

			LogValues();
		}
#if !Info
		private void HandleSplits() {
			bool shouldSplit = false;
			bool isLoading = mem.IsLoading();
			bool complete = mem.SceneFinished();
			bool checkpoint = mem.SectionFinished();
			bool worldFinished = complete && checkpoint;
			bool showLevelSelect = mem.ShowLevelSelect();

			if (currentSplit < Model.CurrentState.Run.Count) {
				if (currentSplit < 0) {
					shouldSplit = mem.Mode() == TinertiaGameMode.SpeedRun && !showLevelSelect && lastShowLevelSelect;
				} else if (Model.CurrentState.Run.Count == 7) {
					shouldSplit = worldFinished && !lastWorldFinished;
				} else {
					shouldSplit = checkpoint && !lastCheckpoint;
				}
			}

			Model.CurrentState.IsGameTimePaused = isLoading;

			lastCheckpoint = checkpoint;
			lastComplete = complete;
			lastWorldFinished = worldFinished;
			lastShowLevelSelect = showLevelSelect;

			HandleSplit(shouldSplit);
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit < 0) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
#endif
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = "", curr = "";
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "State": curr = state.ToString(); break;
						case "Loading": curr = mem.IsLoading().ToString(); break;
						case "World": curr = mem.World().ToString(); break;
						case "Level": curr = mem.Level().ToString(); break;
						case "Checkpoint": curr = mem.SectionFinished().ToString(); break;
						case "Paused": curr = mem.IsPaused().ToString(); break;
						case "Mode": curr = mem.Mode().ToString(); break;
						case "Complete": curr = mem.SceneFinished().ToString(); break;
						case "TimerState": curr = mem.TimerState().ToString(); break;
						case "LevelSelect": curr = mem.ShowLevelSelect().ToString(); break;
						default: curr = ""; break;
					}

					if (!prev.Equals(curr)) {
						WriteLogWithTime(key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (!Console.IsOutputRedirected) {
					Console.WriteLine(data);
				}
				if (hasLog) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				}
			}
		}
		private void WriteLogWithTime(string data) {
#if !Info
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null && Model.CurrentState.CurrentTime.RealTime.HasValue ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + data);
#else
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + ": " + data);
#endif
		}

#if !Info
		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			//Remove duplicate autosplitter componenets
			IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
			bool hasAutosplitter = false;
			for (int i = components.Count - 1; i >= 0; i--) {
				ILayoutComponent component = components[i];
				if (component.Component is SplitterComponent) {
					if ((invalidator == null && width == 0 && height == 0) || hasAutosplitter) {
						components.Remove(component);
					}
					hasAutosplitter = true;
				}
			}

			GetValues();
		}

		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			state = 0;
			Model.CurrentState.IsGameTimePaused = true;
			Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(0));
			WriteLog("---------New Game-------------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			state = 0;
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
		}
		public Control GetSettingsControl(LayoutMode mode) { return null; }
		public void SetSettings(XmlNode document) { }
		public XmlNode GetSettings(XmlDocument document) { return document.CreateElement("Settings"); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
#endif
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() { }
	}
}