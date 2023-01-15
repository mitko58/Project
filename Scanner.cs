using System;
using System.IO;
using System.Text;

namespace scsc
{
	public class Scanner
	{
		const char EOF = '\u001a';
		const char CR = '\r';
		const char LF = '\n';
		
    	static readonly string keywords = " scanf printf return zoom ";
    	static readonly string specialSymbols1 = "{}();~";
    	static readonly string specialSymbols2 = "%/!&|+-<=>";
    	static readonly string specialSymbols2Pairs = " += -= *= /= %= != && || ++ -- <= == >= ";
    	
		private TextReader reader;
		
		private char ch;
		private int line, column;
		private bool skipComments = true;	

		public bool SkipComments {
			get { return skipComments; }
			set { skipComments = value; }
		}
		
		public Scanner(TextReader reader)
		{
			this.reader = reader;
			this.line = 1;
			this.column = 0;
			ReadNextChar();
		}
		
		public void ReadNextChar()
		{
			int ch1 = reader.Read();
			column++;
			ch = (ch1<0) ? EOF : (char)ch1;
			if (ch==CR) {
				line++;
				column = 0;
			} else if (ch==LF) {
				column = 0;
			}
		}
		
		public Token Next()
		{
			int start_column;
			int start_line;

			while (true) 
			{
				start_column = column;
				start_line = line;
				if ((ch >= 'a' && ch <= 'z') ||       // IdentToken check
					(ch >= 'A' && ch <= 'Z')) 
				{
					StringBuilder s = new StringBuilder();

					while ((ch >= 'a' && ch <= 'z') ||
						   (ch >= 'A' && ch <= 'Z') ||
						   (ch >= '0' && ch <= '9') ||
						   (ch == '_' || ch == '.')) 
					{
						s.Append(ch);
						ReadNextChar();
					}

					// KeywordToken check
					string id = s.ToString();
  					if (keywords.Contains(string.Format(" {0} ", id))) 
					{
						return new KeywordToken(start_line, start_column, id);
					}
					return new IdentToken(start_line, start_column, id);
				} 

				//NumberTokenCheck
				else if (ch>='0' && ch<='9') 
				{
					StringBuilder s = new StringBuilder();
					while (ch>='0' && ch<='9') 
					{
						s.Append(ch);
						ReadNextChar();
					}
					long value = Convert.ToInt64(s.ToString());
					return new NumberToken(start_line, start_column, value);
				}

				//Specialsymbol check
				if (specialSymbols1.Contains(ch.ToString()))
				{
					char ch1 = ch;
					ReadNextChar();
					return new SpecialSymbolToken(start_line, start_column, ch1.ToString());
				}
				if (specialSymbols2.Contains(ch.ToString()))
				{
					char ch1 = ch;
					ReadNextChar();
					char ch2 = ch;
					if (specialSymbols2Pairs.Contains(" " + ch1 + ch2 + " "))
					{
						ReadNextChar();
						return new SpecialSymbolToken(start_line, start_column, ch1.ToString() + ch2);
					}
					return new SpecialSymbolToken(start_line, start_column, ch1.ToString());
				}

				//Spaces check
				else if (ch==' ' || ch=='\t' || ch==CR || ch==LF) 
				{
					ReadNextChar();
					continue;
				} 

				

				else if (ch==EOF) 
				{
					return new EOFToken(start_line, start_column);
				} else 
					{
					string s = ch.ToString();
					ReadNextChar();
					return new OtherToken(start_line, start_column, s);
					}
			}
		}
	}
}
