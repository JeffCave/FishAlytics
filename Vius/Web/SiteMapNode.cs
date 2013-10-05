using System;
using System.Web;
using Vius.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Vius.Web
{
	public class SiteMapNode: System.Web.SiteMapNode
	{
		private object locker = new object();

		private List<string> capabilities = new List<string>();
		private List<string> roles = null;

		/// <summary>
		/// Capability list representing reasons a user may see the page. List is an "Or" relationship.
		/// </summary>
		/// <remarks>
		/// In the context of a sitemap, we should display a menu item if the user has any valid excuse for seeing the page.
		/// </remarks>
		/// <value>
		/// The capabilities.
		/// </value>
		public List<string> Capabilities {
			get {
				return capabilities;
			}
		}

		/// <summary>
		/// Gets the roles.
		/// </summary>
		/// <value>
		/// The roles.
		/// </value>
		public new IList<string> Roles{
			get {
				lock (locker) {
					if (roles == null) {
						var allroles = System.Web.Security.Roles.GetAllRoles();
						roles = new List<string>();
						if(Capabilities.Count==0){
							//if no capabilities have been specified, we assume everybody is allowed
							roles.AddRange( allroles);
						} else {
							//we need to check each role independantly to see if it has the 
							//capabilities, its an *any* type match
							foreach (var rolename in allroles) {
								foreach (var capability in Capabilities) {
									var role = Role.GetRole(rolename);
									if (role.Contains(capability)) {
										roles.Add(role);
									}
								}
							}
						}
					}
					return roles.AsReadOnly();
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vius.Web.SiteMapNode"/> class.
		/// </summary>
		/// <param name='provider'>
		/// Provider.
		/// </param>
		/// <param name='key'>
		/// Key.
		/// </param>
		public SiteMapNode(SiteMapProvider provider, string key)
			:base(provider, key)
		{

		}

		/// <Docs>
		/// To be added.
		/// </Docs>
		/// <returns>
		/// To be added.
		/// </returns>
		/// <since version='.NET 2.0'>
		/// 
		/// </since>
		/// <summary>
		/// Determines whether this instance is accessible to the current User specified in the HttpContext
		/// </summary>
		/// <param name='ctx'>
		/// HttpContext to search
		/// </param>
		public override bool IsAccessibleToUser (HttpContext ctx)
		{
			if (!base.IsAccessibleToUser(ctx)) {
				return false;
			}
			return false;
		}
	}

}

