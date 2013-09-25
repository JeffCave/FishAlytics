using System;

namespace Vius
{
	public class Role:System.Collections.Generic.List<Type>
	{
		private string name;
		public string Name {
			get {
				return name;
			}
		}

		public Role (string name)
		{
			this.name = name;
		}

		public new void Add (Type activity)
		{
			//we are only accepting items of type Activity
			if (!activity.IsAssignableFrom(typeof(Activity))) {
				throw new ArgumentException("Added item is not a valid Activity","activity");
			}

			//if we already have it, we don't need to yell at them
			if (this.Contains(activity)) {
				return;
			}

			base.Add(activity);
		}
	}
}

