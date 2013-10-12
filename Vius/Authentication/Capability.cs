using System;

namespace Vius
{
	public sealed class Public:Capability{}

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

