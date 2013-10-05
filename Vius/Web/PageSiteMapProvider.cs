using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using Vius.Data;

namespace Vius.Web
{
	public class PageSiteMapNode: Vius.Web.SiteMapNode
	{
		public PageSiteMapNode(PageSiteMapProvider provider, string key)
			:base(provider, key)
		{

		}

		public new PageSiteMapProvider Provider {
			get {
				return base.Provider as PageSiteMapProvider;
			}
		}

	}

	/// <summary>
	/// Site map provider based on all web pages found
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class PageSiteMapProvider:System.Web.SiteMapProvider
	{
		private List<SiteMapNode> allnodes = null;
		private SiteMapNode root = null;
		private object locker = new object();

		internal List<SiteMapNode> AllNodes {
			get {
				lock(locker){
					if(allnodes == null){
						allnodes = new List<SiteMapNode>();

						var allpages = SearchAssemblies();

						foreach(var pagename in allpages){
							Vius.Web.SiteMapNode node = Type
									.GetType(pagename)
									.GetProperty("MenuInformation")
									.GetValue(null,null) as SiteMapNode;
							if(node != null){
								allnodes.Add(node);
							}
						}
					}
					return allnodes;
				}
			}
		}

		protected static ReadOnlyCollection<string> SearchAssemblies (List<Assembly> assemblies = null, List<Assembly> skip = null)
		{
			var rtn = new List<string>();
			if (assemblies == null) {
				assemblies = new List<Assembly>();
				assemblies.AddRange((ICollection<Assembly>)BuildManager.GetReferencedAssemblies());
			}
			if (skip == null) {
				skip = new List<Assembly>();
			}

			foreach (Assembly child in assemblies) {
				if(skip.Contains(child)){
					continue;
				}
				try{
					var lst = SearchAssemblies(child,skip);
					if(lst.Count > 0){
						rtn.AddRange(lst);
					}
				} catch {
					System.Console.Error.WriteLine("Failed Load (1): " + child.FullName);
				}
			}

			return rtn.Distinct().ToList().AsReadOnly(); 
		}

		protected static ReadOnlyCollection<string> SearchAssemblies (Assembly assembly, List<Assembly> skip = null)
		{
			var rtn = new List<string>();

			if (skip == null) {
				skip = new List<Assembly>();
			}
			if (skip.Contains(assembly)) {
				return rtn.AsReadOnly();
			}

			//get all of the items in this assembly
			foreach (var type in assembly.GetTypes()) {
				if (type.IsSubclassOf(typeof(Vius.Web.Page))) {
					rtn.Add(type.FullName);
				}
			}
			// we've checked this assembly, add it to the skip list to ensure
			// we don't check it again
			skip.Add(assembly);

			//this assembly may reference assemblies we weren't aware of before
			//loop through it's list of assemblies
			foreach (AssemblyName child in assembly.GetReferencedAssemblies()) {
				var ass = Assembly.Load(child);
				//if we are not already aware of the assembly... we need to take action
				if(!skip.Contains(ass)){
					try {
						//recursively search this assembly
						var lst = SearchAssemblies(ass,skip);
						//if it returned stuff, add it to the list
						if(lst.Count > 0){
							rtn.AddRange(lst);
						}
					} catch {
						System.Console.Error.WriteLine("Failed Load (2): " + child.FullName);
					}
				}
			}

			//we only want distinct items (not sure why we get duplicates to begin with)
			return rtn.Distinct().ToList().AsReadOnly(); 
		}


		public PageSiteMapProvider ()
		{
		}

		public override System.Web.SiteMapNode FindSiteMapNode (string rawUrl)
		{
			//don't use linq when only looking for one item
			foreach(var node in AllNodes){
				if(node.Url == rawUrl){
					//get out as soon as we have found a match
					return node;
				}
			}
			return null;
		}

		public override SiteMapNodeCollection GetChildNodes (System.Web.SiteMapNode node)
		{
			throw new NotImplementedException();
		}

		public override System.Web.SiteMapNode GetParentNode (System.Web.SiteMapNode node)
		{
			return node.ParentNode;
		}

		protected override System.Web.SiteMapNode GetRootNodeCore ()
		{
			lock (locker) {
				if(root == null){
					root = new PageSiteMapNode(this,"");
					root.Url = "";
					root.ParentNode = null;
				}
				return root;
			}
		}

	}
}

