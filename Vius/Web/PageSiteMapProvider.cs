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

	/// <summary>
	/// Site map provider based on all web pages found
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class PageSiteMapProvider:System.Web.StaticSiteMapProvider
	{
		private SortedDictionary<string,PageSiteMapNode> allnodes = null;
		private SiteMapNode root = null;
		private object locker = new object();

		protected SortedDictionary<string,SiteMapNode> allnamespaces = new SortedDictionary<string,SiteMapNode>();

		#region Properties
		internal SortedDictionary<string,PageSiteMapNode> AllNodes {
			get {
				lock(locker){
					if(allnodes == null){
						allnodes = new SortedDictionary<string,PageSiteMapNode>();

						var allpages = SearchAssemblies();

						foreach(var page in allpages){
							if(allnodes.ContainsKey(page.FullName)){
								continue;
							}
							// load the menuinformation from the page
							// FIXME:
							//var menuInformation = page.GetProperty("MenuInformation",BindingFlags.Static | BindingFlags.NonPublic);
							//PageSiteMapNode node = menuInformation.GetValue(null,null) as PageSiteMapNode;
							PageSiteMapNode node = null;
							// if the node is null, that means that someone
							// didn't include the static create in the page
							// definition
							if(node == null){
								//create one for them
								//TODO: this needs to go away, it should come from the page definition
								node = new PageSiteMapNode(this,page.FullName);
								node.Title = ParseKey(page.FullName);
							}
							// add it to the list
							if(node != null){
								allnodes.Add(node.Key,node);
								// while we are here, grab the namespace 
								// we will need it later
								var parentkey = ParseParentKey(page.FullName);
								node.ParentNode = CreateNamespaceMapNode(parentkey);
							}
						}
					}
					return allnodes;
				}
			}
		}

		public override System.Web.SiteMapNode RootNode {
			get {
				return GetRootNodeCore();
			}
		}

		#endregion

		#region Initialization
		public PageSiteMapProvider ()
		{
		}

		#endregion

		#region SiteMapProvider
		public override SiteMapNodeCollection GetChildNodes (System.Web.SiteMapNode parent)
		{
			var rtn = new SiteMapNodeCollection();
			// because this is a *sorted* dictionary, all the keys will
			// be grouped sequentially. This means that once we stop
			// finding items, we have found them all and can stop searching
			var found = false;

			found = false;
			foreach (var node in AllNodes) {
				if (parent.Key == ParseParentKey(node.Value.Key)) {
					rtn.Add(node.Value);
					found = true;
				}
				//FIXME:
				//so we didn't find a match, if we had previously found
				//a match, we are outside our range and can stop searching.
//				else if(found) {
//					break;
//				}
			}


			//This needs to be done *after* AllNodes is fetched to make sure allnamespaces is initialized
			found= false;
			foreach (var node in allnamespaces) {
				if(parent.Key == ParseParentKey(node.Value.Key)){
					rtn.Add(node.Value);
					found = true;
				}
				//so we didn't find a match, if we had previously found
				//a match, we are outside our range and can stop searching.
//				else if(found) {
//					break;
//				}
			}

			//return all of the children we found
			return rtn;
		}

		public override System.Web.SiteMapNode GetParentNode (System.Web.SiteMapNode node)
		{
			var parentkey = ParseParentKey(node.Key);
			return CreateNamespaceMapNode(parentkey);
		}

		public Vius.Web.SiteMapNode GetParentNode (Vius.Web.PageSiteMapNode child)
		{
			foreach (var node in AllNodes) {
				throw new NotImplementedException();
			}
			return null;
		}

		protected override System.Web.SiteMapNode GetRootNodeCore ()
		{
			lock (locker) {
				if(root == null){
					root = new PageSiteMapNode(this,"");
					root.Url = "";
					root.Title = "Home";
					root.ParentNode = null;
				}
				return root;
			}
		}

		public override System.Web.SiteMapNode FindSiteMapNode (string rawUrl)
		{
			//don't use linq when only looking for one item
			foreach(var node in AllNodes.Values){
				if(node.Url == rawUrl){
					//get out as soon as we have found a match
					return node;
				}
			}
			return null;
		}
		#endregion

		#region "Helpers"
		protected static ReadOnlyCollection<Type> SearchAssemblies (List<Assembly> assemblies = null, List<Assembly> skip = null)
		{
			var rtn = new List<Type>();
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

		protected static ReadOnlyCollection<Type> SearchAssemblies (Assembly assembly, List<Assembly> skip = null)
		{
			var rtn = new List<Type>();

			if (skip == null) {
				skip = new List<Assembly>();
			}
			if (skip.Contains(assembly)) {
				return rtn.AsReadOnly();
			}

			//get all of the items in this assembly
			foreach (var type in assembly.GetTypes()) {
				if (type.IsSubclassOf(typeof(Vius.Web.Page))) {
					rtn.Add(type);
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

		public override System.Web.SiteMapNode BuildSiteMap ()
		{
			return RootNode;
		}

		private Vius.Web.SiteMapNode CreateNamespaceMapNode (string name)
		{
			//empty key is root node
			if (string.IsNullOrEmpty(name)) {
				return RootNode as PageSiteMapNode;
			}
			// fill the array with a dud element if necessary
			if (!allnamespaces.ContainsKey(name)) {
				lock (allnamespaces) {
					if (!allnamespaces.ContainsKey(name)) {
						allnamespaces.Add(name, null);
					}
				}
			}
			// if the array has more than a dud element, we are done
			if (allnamespaces[name] != null) {
				return allnamespaces[name];
			}

			lock (locker) {
				// need to create the element
				var node = new Vius.Web.SiteMapNode(this, name);
				node.Title = ParseKey(name);

				// need to get the parent, possibly creating it
				var parentkey = ParseParentKey(name);
				node.ParentNode = CreateNamespaceMapNode(parentkey);

				//fill the array
				allnamespaces[name] = node;
				// return the thing we created for recursive parent linking
				return node;
			}
		}

		//maintain a cache of the last calculated value
		private Pair<string,string> lastParseParentKey = new Pair<string, string>("","");
		//calculate the parent key of the current one
		private string ParseParentKey (string key)
		{
			//use the cache if we can
			if (lastParseParentKey.Key != key) {
				lastParseParentKey.Key = key;
				//get everything up to the last "."
				var index = key.LastIndexOf(".");
				if(index > 0){
					lastParseParentKey.Value = key.Substring(0, index);
				} else {
					lastParseParentKey.Value = "";
				}
			}
			//return the value
			return lastParseParentKey.Value;
		}

		//maintain a cache of the last calculated value
		private Pair<string,string> lastParseKey = new Pair<string, string>("","");
		//calculate the parent key of the current one
		private string ParseKey (string fullnamespace)
		{
			//use the cache if we can
			if (lastParseKey.Key != fullnamespace) {
				lastParseKey.Key = fullnamespace;
				//get everything up to the last "."
				var index = fullnamespace.LastIndexOf(".") + 1;
				if(index >= fullnamespace.Length || index <= 0){
					index = 0;
				}
				lastParseKey.Value = fullnamespace.Substring(index);
			}
			//return the value
			return lastParseKey.Value;
		}

		#endregion

	}
}

