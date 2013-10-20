using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Principal;
using System.Web.Security;

namespace Vius.Authentication
{
	/// <summary>
	/// Capabilities.
	/// </summary>
	/// <remarks>
	/// $Id$
	/// $URL$
	/// </remarks>
	public class Capabilities
	{
		private static CapabilityProvider Instance {
			get {
				return CapabilityProvider.Instance;
			}
		}

		public static ReadOnlyCollection<string> AllCapabilities {
			get {
				return Instance.AllCapabilities;
			}
		}

		public static bool Exists(string capability) {
			return Instance.AllCapabilities.Contains(capability);
		}

		public static void AddCapabilityToRole (string capability, string role)
		{
			var roles = new string[] {role};
			var capabilities = new string[] {capability};
			AddCapabilityToRole(capabilities,roles);
		}

		public static void AddCapabilityToRole (string capability, string[] roles)
		{
			var capabilities = new string[] {capability};
			AddCapabilityToRole(capabilities,roles);
		}

		public static void AddCapabilityToRole (string[] capabilities, string role)
		{
			var roles = new string[] {role};
			AddCapabilityToRole(capabilities, roles);
		}

		public static void AddCapabilityToRole (string[] capability, string[] role)
		{
			throw new NotImplementedException();
		}

		public static bool CheckAll (ICollection<string> permissions, ICollection<string> requirements)
		{
			foreach (var r in requirements) {
				if(!permissions.Contains(r)){
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks that any of the given permissions are present
		/// </summary>
		/// <returns>
		/// results of search (true/false)
		/// </returns>
		/// <param name='permissions'>
		/// If set to <c>true</c> permissions.
		/// </param>
		/// <param name='requirements'>
		/// If set to <c>true</c> requirements.
		/// </param>
		public static bool CheckAny (ICollection<string> permissions, ICollection<string> requirements)
		{
			foreach (var r in requirements) {
				if(permissions.Contains(r)){
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Check the specified permissions against the given requirements.
		/// </summary>
		/// <param name='permissions'>
		/// If set to <c>true</c> permissions.
		/// </param>
		/// <param name='requirements'>
		/// If set to <c>true</c> requirements.
		/// </param>
		public static bool Check (ICollection<string> permissions, ICollection<string> requirements)
		{
			return CheckAll(permissions, requirements);
		}

		/// <summary>
		/// Check the specified permissions against the given requirements.
		/// </summary>
		/// <param name='permission'>
		/// If set to <c>true</c> permission.
		/// </param>
		/// <param name='requirements'>
		/// If set to <c>true</c> requirements.
		/// </param>
		public static bool Check (string permission, ICollection<string> requirements)
		{
			var permissions = new List<string>(){permission};
			return Check(permissions, requirements);
		}

		/// <summary>
		/// Check the specified user's permissions against the given requirements.
		/// </summary>
		/// <param name='user'>
		/// If set to <c>true</c> user.
		/// </param>
		/// <param name='requirements'>
		/// If set to <c>true</c> requirements.
		/// </param>
		public static bool Check (IPrincipal user, ICollection<string> requirements)
		{
			var permissions = GetUserCapabilities(user);
			return Check(permissions, requirements);
		}

		/// <summary>
		/// Check the specified requirements against the current user's permission
		/// </summary>
		/// <param name='requirements'>
		/// If set to <c>true</c> requirements.
		/// </param>
		public static bool Check (ICollection<string> requirements){
			return Check(System.Web.HttpContext.Current.User,requirements);
		}

		/// <summary>
		/// Check the specified requirements against the current user's permission
		/// </summary>
		/// <param name='requirements'>
		/// If set to <c>true</c> requirements.
		/// </param>
		public static bool Check (string requirement){
			var requirements = new List<string>(){requirement};
			return Check(System.Web.HttpContext.Current.User,requirements);
		}

		//object pool to contain last couple of users
		//TODO: turn this into a pool rather than a single instance
		private static Pair<string,List<string>> getUserCapabilities;
		private static object getUserCapabilitiesLocker = new object();

		/// <summary>
		/// Gets the user capabilities for the specified user
		/// </summary>
		/// <returns>
		/// The user capabilities.
		/// </returns>
		/// <param name='user'>
		/// User.
		/// </param>
		public static ReadOnlyCollection<string> GetUserCapabilities (System.Security.Principal.IPrincipal user)
		{
			return GetUserCapabilities(user.Identity.Name);
		}

		/// <summary>
		/// Gets the capabilities of the current user
		/// </summary>
		/// <returns>
		/// The user capabilities.
		/// </returns>
		public static ReadOnlyCollection<string> GetUserCapabilities ()
		{
			return(GetUserCapabilities(System.Web.HttpContext.Current.User));
		}

		/// <summary>
		/// Gets the user capabilities for the specified user
		/// </summary>
		/// <returns>
		/// The user capabilities.
		/// </returns>
		/// <param name='username'>
		/// Username.
		/// </param>
		public static ReadOnlyCollection<string> GetUserCapabilities (string username)
		{
			lock (getUserCapabilitiesLocker) {
				//if we don't have this person, mark it for initialization
				if (getUserCapabilities != null){
				    if(getUserCapabilities.Key != username) {
						getUserCapabilities = null;
					} else if(string.IsNullOrEmpty(getUserCapabilities.Key)){
						getUserCapabilities =null;
					}
				}

				//create the object if necessary
				if (getUserCapabilities == null) {
					getUserCapabilities = new Pair<string, List<string>>();
					getUserCapabilities.Key = username;
					getUserCapabilities.Value = new List<string>();

					var roles = Roles.GetRolesForUser(username);
					foreach (var rolename in roles) {
						var role = Role.GetRole(rolename);

						//add unique capabilities
						//usercapabilities.AddRange(role);
						foreach (var capability in role) {
							if (!getUserCapabilities.Value.Contains(capability)) {
								getUserCapabilities.Value.Add(capability);
							}

						}
					}
				}
				return getUserCapabilities.Value.AsReadOnly();
			}
		}
	}
}

