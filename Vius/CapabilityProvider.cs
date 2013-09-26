using System;
using System.Collections.Generic;
using System.Reflection;

namespace Vius
{
	public class CapabilityProvider
	{
		protected Assembly baseAssembly = null;
		private object locker = new object();

		public CapabilityProvider ()
		{
			baseAssembly = Assembly.GetExecutingAssembly();
		}

		public CapabilityProvider (Assembly assembly)
		{
			baseAssembly = assembly;
		}

		private List<Type> allActivities = null;
		public List<Type> AllActivities {
			get {
				return GetAllActivities();
			}
		}

		protected static List<Type> GetAllActivities (Assembly assembly)
		{
			var rtn = new List<Type>();

			foreach (var type in assembly.GetTypes()) {
				if (type.IsAssignableFrom(typeof(Capability))) {
					rtn.Add(type);
				}
			}

			foreach (AssemblyName child in assembly.GetReferencedAssemblies()) {
				var ass = Assembly.Load(child);
				var lst = GetAllActivities(ass);
				rtn.AddRange(lst);
			}

			return rtn;
		}

		/// <summary>
		/// Gets all activities.
		/// </summary>
		/// <returns>
		/// All activities.
		/// </returns>
		/// <param name='force'>
		/// Force a reload of the Activities
		/// </param>
		public List<Type> GetAllActivities (bool force = false)
		{
			if (force || allActivities == null) {
				allActivities = null;
				lock(locker){
					if(allActivities == null){
						allActivities = GetAllActivities(baseAssembly);
					}
				}
			}
			return allActivities;
		}
	}
}

