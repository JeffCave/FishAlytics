using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Security;
using Mono.Data.Sqlite;

namespace Vius
{
	/// <summary>
	/// Capability provider.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class CapabilityProvider
	{
		protected List<Assembly> _assemblies = new List<Assembly>();
		private object locker = new object();

		private static object staticlocker = new object();
		private static CapabilityProvider instance = null;

		private ReadOnlyCollection<string> allCapabilities = null;


		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>
		/// The instance.
		/// </value>
		public static CapabilityProvider Instance {
			get {
				lock(staticlocker){
					if(instance == null){
						instance = new CapabilityProvider();
					}
					return instance;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vius.CapabilityProvider"/> class.
		/// </summary>
		public CapabilityProvider ()
		{
			var assList = BuildManager.GetReferencedAssemblies();
			foreach (var ass in assList) {
				_assemblies.Add(ass as Assembly);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vius.CapabilityProvider"/> class.
		/// </summary>
		/// <param name='assembly'>
		/// Assembly.
		/// </param>
		public CapabilityProvider (Assembly assembly)
		{
			_assemblies.Add(assembly);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vius.CapabilityProvider"/> class.
		/// </summary>
		/// <param name='assemblies'>
		/// Assemblies.
		/// </param>
		public CapabilityProvider (IEnumerable<Assembly> assemblies)
		{
			_assemblies.AddRange(assemblies);
		}

		/// <summary>
		/// All capabilities.
		/// </summary>
		public ReadOnlyCollection<string> AllCapabilities {
			get {
				lock(locker){
					if(allCapabilities == null){
						allCapabilities = GetAllCapabilities(_assemblies);
					}
					return allCapabilities;
				}
			}
		}

		/// <summary>
		/// Recursively searches all assemblies for capabilities.
		/// </summary>
		/// <returns>
		/// All the capabilities.
		/// </returns>
		/// <param name='assemblies'>
		/// Assemblies to be searched
		/// </param>
		/// <param name='skip'>
		/// A list of assemblies to be ignored
		/// </param>
		protected static ReadOnlyCollection<string> GetAllCapabilities (ICollection<Assembly> assemblies, List<Assembly> skip = null)
		{
			var rtn = new List<string>();
			if (skip == null) {
				skip = new List<Assembly>();
			}

			foreach (Assembly child in assemblies) {
				if(skip.Contains(child)){
					continue;
				}
				try{
					var lst = GetAllCapabilities(child,skip);
					if(lst.Count > 0){
						rtn.AddRange(lst);
					}
				} catch {
					System.Console.Error.WriteLine("Failed Load (1): " + child.FullName);
				}
			}

			return rtn.Distinct().ToList().AsReadOnly(); 
		}

		/// <summary>
		/// Gets all capabilities.
		/// </summary>
		/// <returns>
		/// The all capabilities.
		/// </returns>
		/// <param name='assembly'>
		/// Assembly.
		/// </param>
		/// <param name='skip'>
		/// Skip.
		/// </param>
		protected static ReadOnlyCollection<string> GetAllCapabilities (Assembly assembly, List<Assembly> skip = null)
		{
			var rtn = new List<string>();

			if (skip == null) {
				skip = new List<Assembly>();
			}
			if (skip.Contains(assembly)) {
				return rtn.AsReadOnly();
			}

			foreach (var type in assembly.GetTypes()) {
				if (type.IsSubclassOf(typeof(Vius.Capability))) {
					rtn.Add(type.FullName);
				}
			}

			System.Console.Out.WriteLine("Checked Assembly: " + assembly.FullName);
			skip.Add(assembly);

			foreach (AssemblyName child in assembly.GetReferencedAssemblies()) {
				var ass = Assembly.Load(child);
				if(!skip.Contains(ass)){
					try {
						var lst = GetAllCapabilities(ass,skip);
						if(lst.Count > 0){
							rtn.AddRange(lst);
						}
					} catch {
						System.Console.Error.WriteLine("Failed Load (2): " + child.FullName);
					}
				}
			}

			return rtn.Distinct().ToList().AsReadOnly(); 
		}

		/// <summary>
		/// Clears the Capabilities listing internally, forcing a reload
		/// </summary>
		public void ResetCapabilities()
		{
			allCapabilities = null;
		}

		/// <summary>
		/// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
		/// </summary>
		/// <param name="roleName">The name of the role to search for in the data source.</param>
		/// <returns>
		/// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
		/// </returns>
		public bool Exists(string capabilityName)
		{
			return AllCapabilities.Contains(capabilityName);
		}

	}

}

