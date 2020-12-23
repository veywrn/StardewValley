using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Force.DeepCloner.Helpers
{
	internal static class TypeCreationHelper
	{
		private static ModuleBuilder _moduleBuilder;

		internal static ModuleBuilder GetModuleBuilder()
		{
			if (_moduleBuilder == null)
			{
				AssemblyName aName = new AssemblyName("DeepClonerCode");
				_moduleBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run).DefineDynamicModule(aName.Name);
			}
			return _moduleBuilder;
		}
	}
}
