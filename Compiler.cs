using System;
using System.Collections.Generic;
using System.IO;

namespace scsc
{
	public static class Compiler
	{
		private static List<string> references = new List<string>();
		
		public static bool Compile(string file, string assemblyName)
		{
			return Compiler.Compile(file, assemblyName, new DefaultDiagnostics());
		}
		
		public static bool Compile(string file, string assemblyName, Diagnostics diag)
		{
			TextReader reader = new StreamReader(file);
			Scanner scanner = new Scanner(reader);
			Table symbolTable = new Table(references);
			Emit emit = new Emit(assemblyName, symbolTable);
			Parser parser = new Parser(scanner, emit, symbolTable, diag);
			
			diag.BeginSourceFile(file);
			emit.Initialize();
			bool isProgram = parser.Parse();
			diag.EndSourceFile();
			
			if (isProgram) {
				emit.Finally();
				Console.WriteLine("Програмата е компилирана! \nСтартирайте example.exe");
				return true;
			}
			else {
				return false;
			}
			Token t = scanner.Next();
			while (!(t is EOFToken))
			{
				Console.WriteLine(t.ToString());
				t = scanner.Next();
			}
		}
		
		public static void AddReferences(List<string> references)
		{
			Compiler.references.AddRange(references);
		}
	}
}
