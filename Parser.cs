using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace scsc
{
	public class Parser
	{
		private Scanner scanner;
		private Emit emit;
		private Table symbolTable;
		private Token token;
		private Diagnostics diag;
		
		public Parser(Scanner scanner, Emit emit, Table symbolTable, Diagnostics diag)
		{
			this.scanner = scanner;
			this.emit = emit;
			this.symbolTable = symbolTable;
			this.diag = diag;
			ReadNextToken(); 
		}
		public void ReadNextToken()
		{
			token = scanner.Next();
		}
		private LocalVarSymbol GetLocalVarSymbol(Token token)
		{
			IdentToken tempIdent = (IdentToken)token;
			LocalVarSymbol localVar;

			if (!symbolTable.ExistCurrentScopeSymbol(tempIdent.value))
			{
				LocalBuilder tmpVar = emit.AddLocalVar(tempIdent.value, typeof(int));
				localVar = symbolTable.AddLocalVar(tempIdent, tmpVar);
			}
			else
			{
				localVar = (LocalVarSymbol)symbolTable.GetSymbol(tempIdent.value);
			}
			return localVar;
		}
		
		
		public bool CheckSpecialSymbol(string symbol)
		{
			bool result = (token is SpecialSymbolToken) && ((SpecialSymbolToken)token).value == symbol;
			if (result) 
				ReadNextToken();
			return result;
		}
		public bool CheckKeyword(string keyword)
		{
			bool result = (token is KeywordToken) && ((KeywordToken)token).value == keyword;
			if (result)
				ReadNextToken();
			return result;
		}

		public bool CheckIdent()
		{
			bool result = (token is IdentToken);
			if (result) 
				ReadNextToken();
			return result;
		}
		
		public bool CheckNumber()
		{
			bool result = (token is NumberToken);
			if (result) ReadNextToken();
			return result;
		}
		public bool Parse()
		{
			
			while (isStatement()) ;

			return (token is EOFToken);
		}
		private bool isStatement()
		{
			if (isExpression())
			{
				if (!CheckSpecialSymbol(";"))
				{
					Error("Очаквам специален символ ';'");
					return false;
				}

				emit.AddPop();
				return true;
			}

			if (!CheckSpecialSymbol(";"))
				return false;

			return true;
		}
		private bool isExpression()
		{
			if (isBitwiseAndExpression())
			{
				if (CheckSpecialSymbol("|"))
				{
					if (!isExpression())
					{
						Error("Bitwise OR Expression Required", token);
						return false;
					}

					emit.AddOr();
				}

				return true;
			}

			return false;
		}

		

		public void Error(string message)
		{
			diag.Error(token.line, token.column, message);
			//SkipUntilSemiColon();
		}
		
		public void Error(string message, Token token)
		{
			diag.Error(token.line, token.column, message);
			//SkipUntilSemiColon();
		}
		
		
		private bool isBitwiseAndExpression()
		{
			if (isAdditiveExpression())
			{
				if (CheckSpecialSymbol("&"))
				{
					if (!isBitwiseAndExpression())
					{
						Error("Additive Expression Required", token);
						return false;
					}

					emit.AddAnd();
				}

				return true;
			}

			return false;
		}
		private bool isAdditiveExpression()
		{
			if (isMultiplicativeExpression())
			{
				if (CheckSpecialSymbol("+"))
				{
					if (!isAdditiveExpression())
					{
						Error("Multiplicative Expression Required", token);
						return false;
					}

					emit.AddPlus();
				}
				if (CheckSpecialSymbol("-"))
				{
					if (!isAdditiveExpression())
					{
						Error("Multiplicative Expression Required", token);
						return false;
					}

					emit.AddMinus();
				}

				return true;
			}

			return false;
		}
		private bool isMultiplicativeExpression()
		{
			if (isPrimaryExpression())
			{
				if (CheckSpecialSymbol("*"))
				{
					if (!isMultiplicativeExpression())
					{
						Error("Primary Expression Required!", token);
						return false;
					}
					emit.AddMul();
				}
				if (CheckSpecialSymbol("/"))
				{
					if (!isMultiplicativeExpression())
					{
						Error("Primary Expression Required!", token);
						return false;
					}
					emit.AddDiv();
				}
				if (CheckSpecialSymbol("%"))
				{
					if (!isMultiplicativeExpression())
					{
						Error("Primary Expression Required!", token);
						return false;
					}
					emit.AddRem();
				}

				return true;
			}

			return false;
		}

		private bool isPrimaryExpression()
		{
			Token tempToken = token;
			if (CheckIdent())
			{
				LocalVarSymbol localVar = this.GetLocalVarSymbol(tempToken);

				if (CheckSpecialSymbol("="))
				{
					if (!isExpression())
					{
						Error("Expression Required!", token);
						return false;
					}

					emit.AddLocalVarAssigment(localVar.localVariableInfo);
					emit.AddGetLocalVar(localVar.localVariableInfo);
					return true;
				}

				if (CheckSpecialSymbol("++"))
				{
					emit.AddGetLocalVar(localVar.localVariableInfo);
					emit.AddDuplicate();
					emit.AddGetNumber(1);
					emit.AddPlus();
					emit.AddLocalVarAssigment(localVar.localVariableInfo);
					return true;
				}

				if (CheckSpecialSymbol("--"))
				{
					emit.AddGetLocalVar(localVar.localVariableInfo);
					emit.AddDuplicate();
					emit.AddGetNumber(1);
					emit.AddMinus();
					emit.AddLocalVarAssigment(localVar.localVariableInfo);
					return true;
				}

				emit.AddGetLocalVar(localVar.localVariableInfo);
				return true;
			}

			if (CheckSpecialSymbol("~"))
			{
				if (!isPrimaryExpression())
				{
					Error("Primary Expression Required!", token);
					return false;
				}
				emit.AddNot();
				return true;
			}

			if (CheckSpecialSymbol("++"))
			{
				tempToken = token;
				if (!CheckIdent())
				{
					Error("Ident Required!", token);
					return false;
				}

				LocalVarSymbol localVar = this.GetLocalVarSymbol(tempToken);
				emit.AddGetLocalVar(localVar.localVariableInfo);
				emit.AddGetNumber(1);
				emit.AddPlus();
				emit.AddDuplicate();
				emit.AddLocalVarAssigment(localVar.localVariableInfo);
				return true;
			}

			if (CheckSpecialSymbol("--"))
			{
				tempToken = token;
				if (!CheckIdent())
				{
					Error("Ident Required!", token);
					return false;
				}

				LocalVarSymbol localVar = this.GetLocalVarSymbol(tempToken);
				emit.AddGetLocalVar(localVar.localVariableInfo);
				emit.AddGetNumber(1);
				emit.AddMinus();
				emit.AddDuplicate();
				emit.AddLocalVarAssigment(localVar.localVariableInfo);
				return true;
			}

			if (CheckSpecialSymbol("("))
			{
				if (!isExpression())
				{
					Error("Expected Expression", token);
					return false;
				}
				if (!CheckSpecialSymbol(")"))
				{
					Error(token + ")");
					return false;
				}
				return true;
			}

			//nop
			if (CheckKeyword("nop"))
			{
				emit.EmitNop();
				return true;
			}

			if (CheckKeyword("scanf"))
			{
				if (!CheckSpecialSymbol("("))
				{
					Error(token + "(");
					return false;
				}
				if (!CheckSpecialSymbol(")"))
				{
					Error(token + ")");
					return false;
				}

				emit.EmitReLn();
				return true;
			}

			if (CheckKeyword("zoom"))
			{
				if (!CheckSpecialSymbol("("))
				{
					Error(token + "(");
					return false;
				}
				if (!isExpression())
				{
					Error("Expected Expression", token);
					return false;
				}
				else
				{
					emit.EmitWrLn();
					emit.AddGetNumber(0);
				}
				if (!CheckSpecialSymbol(")"))
				{
					Error(token + ")");
					return false;
				}
				return true;
			}


			if (CheckNumber())
			{
				emit.AddGetNumber(((NumberToken)tempToken).value);
				return true;
			}


			if (CheckKeyword("printf"))
			{
				if (!CheckSpecialSymbol("("))
				{
					Error(token + "(");
					return false;
				}
				if (!isExpression())
				{
					Error("Expected Expression", token);
					return false;
				}
				else
				{
					emit.EmitWrLn();
					emit.AddGetNumber(0);
				}
				if (!CheckSpecialSymbol(")"))
				{
					Error(token + ")");
					return false;
				}
				return true;
			}

			return false;
		}
	}
}
