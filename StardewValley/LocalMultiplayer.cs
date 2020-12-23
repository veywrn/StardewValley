using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace StardewValley
{
	public class LocalMultiplayer
	{
		public delegate void StaticInstanceMethod(object staticVarsHolder);

		internal static List<FieldInfo> staticFields;

		internal static List<object> staticDefaults;

		public static Type StaticVarHolderType;

		private static DynamicMethod staticDefaultMethod;

		private static DynamicMethod staticSaveMethod;

		private static DynamicMethod staticLoadMethod;

		public static StaticInstanceMethod StaticSetDefault;

		public static StaticInstanceMethod StaticSave;

		public static StaticInstanceMethod StaticLoad;

		public static bool IsLocalMultiplayer(bool is_local_only = false)
		{
			if (is_local_only)
			{
				return Game1.hasLocalClientsOnly;
			}
			return GameRunner.instance.gameInstances.Count > 1;
		}

		public static void Initialize()
		{
			GetStaticFieldsAndDefaults();
			GenerateDynamicMethodsForStatics();
		}

		private static void GetStaticFieldsAndDefaults()
		{
			staticFields = new List<FieldInfo>();
			staticDefaults = new List<object>();
			new List<Type>();
			HashSet<string> ignored_assembly_roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Microsoft",
				"MonoGame",
				"mscorlib",
				"NetCode",
				"System",
				"xTile"
			};
			List<Type> types = new List<Type>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (!ignored_assembly_roots.Contains(assembly.GetName().Name.Split('.')[0]))
				{
					Type[] types2 = assembly.GetTypes();
					foreach (Type type in types2)
					{
						types.Add(type);
					}
				}
			}
			foreach (Type type2 in types)
			{
				if (type2.GetCustomAttributes(typeof(CompilerGeneratedAttribute), inherit: true).Length == 0)
				{
					bool include_by_default = false;
					if (type2.GetCustomAttributes(typeof(InstanceStatics), inherit: true).Length != 0)
					{
						include_by_default = true;
					}
					FieldInfo[] fields = type2.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (FieldInfo field in fields)
					{
						if (!field.IsInitOnly && field.IsStatic && !field.IsLiteral && (include_by_default || field.GetCustomAttributes(typeof(InstancedStatic), inherit: true).Length != 0) && field.GetCustomAttributes(typeof(NonInstancedStatic), inherit: true).Length == 0)
						{
							RuntimeHelpers.RunClassConstructor(field.DeclaringType.TypeHandle);
							staticFields.Add(field);
							staticDefaults.Add(field.GetValue(null));
						}
					}
				}
			}
		}

		private static void GenerateDynamicMethodsForStatics()
		{
			TypeBuilder typeBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("StardewValley.StaticInstanceVars"), AssemblyBuilderAccess.RunAndCollect).DefineDynamicModule("MainModule").DefineType("StardewValley.StaticInstanceVars", TypeAttributes.Public | TypeAttributes.AutoClass);
			foreach (FieldInfo field4 in staticFields)
			{
				typeBuilder.DefineField(field4.DeclaringType.Name + "_" + field4.Name, field4.FieldType, FieldAttributes.Public);
			}
			StaticVarHolderType = typeBuilder.CreateType();
			staticDefaultMethod = new DynamicMethod("SetStaticVarsToDefault", null, new Type[1]
			{
				typeof(object)
			}, typeof(Game1).Module, skipVisibility: true);
			ILGenerator il3 = staticDefaultMethod.GetILGenerator();
			LocalBuilder local3 = il3.DeclareLocal(StaticVarHolderType);
			il3.Emit(OpCodes.Ldarg_0);
			il3.Emit(OpCodes.Castclass, StaticVarHolderType);
			il3.Emit(OpCodes.Stloc_0);
			FieldInfo defaultsField = typeof(LocalMultiplayer).GetField("staticDefaults", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo listIndexOperator = typeof(List<object>).GetMethod("get_Item");
			for (int i = 0; i < staticFields.Count; i++)
			{
				FieldInfo field = staticFields[i];
				il3.Emit(OpCodes.Ldloc, local3.LocalIndex);
				il3.Emit(OpCodes.Ldsfld, defaultsField);
				il3.Emit(OpCodes.Ldc_I4, i);
				il3.Emit(OpCodes.Callvirt, listIndexOperator);
				if (field.FieldType.IsValueType)
				{
					il3.Emit(OpCodes.Unbox_Any, field.FieldType);
				}
				else
				{
					il3.Emit(OpCodes.Castclass, field.FieldType);
				}
				il3.Emit(OpCodes.Stfld, StaticVarHolderType.GetField(field.DeclaringType.Name + "_" + field.Name));
			}
			il3.Emit(OpCodes.Ret);
			StaticSetDefault = (StaticInstanceMethod)staticDefaultMethod.CreateDelegate(typeof(StaticInstanceMethod));
			staticSaveMethod = new DynamicMethod("SaveStaticVars", null, new Type[1]
			{
				typeof(object)
			}, typeof(Game1).Module, skipVisibility: true);
			il3 = staticSaveMethod.GetILGenerator();
			local3 = il3.DeclareLocal(StaticVarHolderType);
			il3.Emit(OpCodes.Ldarg_0);
			il3.Emit(OpCodes.Castclass, StaticVarHolderType);
			il3.Emit(OpCodes.Stloc_0);
			foreach (FieldInfo field3 in staticFields)
			{
				il3.Emit(OpCodes.Ldloc, local3.LocalIndex);
				il3.Emit(OpCodes.Ldsfld, field3);
				il3.Emit(OpCodes.Stfld, StaticVarHolderType.GetField(field3.DeclaringType.Name + "_" + field3.Name));
			}
			il3.Emit(OpCodes.Ret);
			StaticSave = (StaticInstanceMethod)staticSaveMethod.CreateDelegate(typeof(StaticInstanceMethod));
			staticLoadMethod = new DynamicMethod("LoadStaticVars", null, new Type[1]
			{
				typeof(object)
			}, typeof(Game1).Module, skipVisibility: true);
			il3 = staticLoadMethod.GetILGenerator();
			local3 = il3.DeclareLocal(StaticVarHolderType);
			il3.Emit(OpCodes.Ldarg_0);
			il3.Emit(OpCodes.Castclass, StaticVarHolderType);
			il3.Emit(OpCodes.Stloc_0);
			foreach (FieldInfo field2 in staticFields)
			{
				il3.Emit(OpCodes.Ldloc, local3.LocalIndex);
				il3.Emit(OpCodes.Ldfld, StaticVarHolderType.GetField(field2.DeclaringType.Name + "_" + field2.Name));
				il3.Emit(OpCodes.Stsfld, field2);
			}
			il3.Emit(OpCodes.Ret);
			StaticLoad = (StaticInstanceMethod)staticLoadMethod.CreateDelegate(typeof(StaticInstanceMethod));
		}

		public static void SaveOptions()
		{
			if (Game1.player != null && (bool)Game1.player.isCustomized)
			{
				if (!Game1.splitscreenOptions.ContainsKey(Game1.player.UniqueMultiplayerID))
				{
					Game1.splitscreenOptions.Add(Game1.player.UniqueMultiplayerID, Game1.options);
				}
				else
				{
					Game1.splitscreenOptions[Game1.player.uniqueMultiplayerID] = Game1.options;
				}
			}
		}
	}
}
