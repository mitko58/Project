using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace scsc
{
	public class Emit
	{
		private AssemblyBuilder assembly;
		private ModuleBuilder module;
		private TypeBuilder program;
		private MethodBuilder method;
		private ConstructorBuilder cctor;
		private ILGenerator il;


		private bool haveMainMethod;

		private string executableName;
		private Table symbolTable;
		
		public Emit(string name, Table symbolTable)
		{
			this.symbolTable = symbolTable;
			executableName = name;

			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = Path.GetFileNameWithoutExtension(name);
			string dir = Path.GetDirectoryName(name);

			string moduleName = Path.GetFileName(name);
			
			assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, dir);
			module = assembly.DefineDynamicModule(assemblyName + "Module", moduleName);
			haveMainMethod = true;
		}
	
		#region Initialize & Finally
		public void Initialize()
		{
			// Създава клас
			program = module.DefineType("CSClass");

			// Създава Конструктор
			cctor = program.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { });
			ILGenerator ctorIL = cctor.GetILGenerator();
			ctorIL.Emit(OpCodes.Ret);

			// Създава мain
			method = program.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static);
			method.InitLocals = true;
			il = method.GetILGenerator();
			symbolTable.BeginScope();
		}

		public Type Finally()
		{
			symbolTable.EndScope();
			il.Emit(OpCodes.Ret);
			Type returnValue = program.CreateType();
			assembly.SetEntryPoint(method);
			assembly.Save(Path.GetFileName(executableName));

			return returnValue;
		}
		#endregion


		public void AddMethodCall(MethodInfo methodInfo)
		{
			il.Emit(OpCodes.Call, methodInfo);
		}
		public void AddFieldAssigment(FieldInfo fieldInfo)
		{
			il.Emit(OpCodes.Stsfld, fieldInfo);
		}
		
		public LocalBuilder AddLocalVar(string localVarName, Type localVarType)
		{
			LocalBuilder result = il.DeclareLocal(localVarType);
			if (!localVarType.IsValueType)
			{
				
				il.Emit(OpCodes.Newobj, localVarType);
				il.Emit(OpCodes.Stloc, result);
			}
			return result;
		}

		public void AddGetLocalVar(LocalVariableInfo localVariableInfo)
		{
			il.Emit(OpCodes.Ldloc, (LocalBuilder)localVariableInfo);
		}

		public void AddLocalVarAssigment(LocalVariableInfo localVariableInfo)
		{
			il.Emit(OpCodes.Stloc, (LocalBuilder)localVariableInfo);
		}

		public void AddGetNumber(long value)
		{
			if (value>=Int32.MinValue && value<=Int32.MaxValue) {
				il.Emit(OpCodes.Ldc_I4, (Int32)value);
			} else {
				il.Emit(OpCodes.Ldc_I8, value);
			}
		}

		

		public void AddDuplicate()
		{
			il.Emit(OpCodes.Dup);
		}
		public void AddPop()
		{
			il.Emit(OpCodes.Pop);
		}

		#region Operators
		public void AddPlus()
		{
			il.Emit(OpCodes.Add);
		}

		public void AddMinus()
		{
			il.Emit(OpCodes.Sub);
		}

		public void AddMul()
		{
			il.Emit(OpCodes.Mul);
		}

		public void AddDiv()
		{
			il.Emit(OpCodes.Div);
		}

		// %
		public void AddRem()
		{
			il.Emit(OpCodes.Rem);
		}

		// & &&
		public void AddAnd()
		{
			il.Emit(OpCodes.And);
		}

		// | ||
		public void AddOr()
		{
			il.Emit(OpCodes.Or);
		}

		// ^
		public void AddXor()
		{
			il.Emit(OpCodes.Xor);
		}

		// ~ !
		public void AddNot()
		{
			il.Emit(OpCodes.Not);
		}

		// no operator
		public void AddNop()
		{
			il.Emit(OpCodes.Nop);
		}
		public void EmitReLn()
		{
			MethodInfo readLineM = typeof(Console).GetMethod("ReadLine", new Type[0]);
			MethodInfo convertInt32M = typeof(Convert).GetMethod("ToInt32", new Type[] { typeof(string) });
			il.EmitCall(OpCodes.Call, readLineM, null);
			il.EmitCall(OpCodes.Call, convertInt32M, null);
		}

		public void EmitNop()
		{
			MethodInfo writeM = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });
			il.EmitCall(OpCodes.Call, writeM, null);
		}

		public void EmitWrLn()
		{
			MethodInfo writeM = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) });
			il.EmitCall(OpCodes.Call, writeM, null);
		}
	}
}
#endregion