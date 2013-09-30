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
	}
}

