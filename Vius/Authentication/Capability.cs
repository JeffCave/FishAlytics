using System;

public sealed class Public:Vius.Authentication.Capability{}


namespace Vius.Authentication
{
	public abstract class Capability
	{
		public static Type FromString(string activity){
			return Type.GetType(activity);
		}

		public static string ToString (Type activity)
		{
			return activity.Namespace + "." + activity.Name;
		}

		public override string ToString ()
		{
			return this.GetType().Name;
		}
	}
}

