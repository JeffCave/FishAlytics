using Vius.Authentication;

namespace Fishing
{
	namespace Catch {
		public class Add                  :Capability{}
		public class Edit                 :Capability{}
		public class Delete               :Capability{}
		public class View                 :Capability{}

		namespace Own{
			public class Add                     :Capability{}
			public class Edit                    :Capability{}
			public class Delete                  :Capability{}
			public class View                    :Capability{}
		}
	}
}

