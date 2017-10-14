#if !Info
using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;
namespace LiveSplit.Tinertia {
	public class SplitterFactory : IComponentFactory {
		public string ComponentName { get { return "Tinertia Autosplitter v" + this.Version.ToString(); } }
		public string Description { get { return "Autosplitter for Tinertia"; } }
		public ComponentCategory Category { get { return ComponentCategory.Control; } }
		public IComponent Create(LiveSplitState state) { return new SplitterComponent(state); }
		public string UpdateName { get { return this.ComponentName; } }
		public string UpdateURL { get { return "https://raw.githubusercontent.com/ShootMe/LiveSplit.Tinertia/master/"; } }
		public string XMLURL { get { return this.UpdateURL + "Components/Updates.xml"; } }
		public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
	}
}
#endif