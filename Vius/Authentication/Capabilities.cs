using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Web.Security;

namespace Vius
{
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
		public static bool CheckAny (ICollection<string> permissions, ICollection<string> requirements)
		{
			foreach (var r in requirements) {
				if(permissions.Contains(r)){
					return true;
				}
			}
			return false;
		}
		public static bool Check (ICollection<string> permissions, ICollection<string> requirements)
		{
			return CheckAll(permissions, requirements);
		}
		public static bool Check (string permission, ICollection<string> requirements)
		{
			var permissions = new List<string>(){permission};
			return Check(permissions, requirements);
		}

		//object pool to contain last couple of users
		private static Pair<string,List<string>> getUserCapabilities;
		private static object getUserCapabilitiesLocker = new object();
		public static ReadOnlyCollection<string> GetUserCapabilities (System.Security.Principal.IPrincipal user)
		{
			return GetUserCapabilities(user.Identity.Name);
		}
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
							if (getUserCapabilities.Value.Contains(capability)) {
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

