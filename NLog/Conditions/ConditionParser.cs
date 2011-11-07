
namespace NLog.Conditions
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using NLog.Config;
	using NLog.Internal;
	using NLog.Layouts;

	/// <summary>
	/// Condition parser. Turns a string representation of condition expression
	/// into an expression tree.
	/// </summary>
	public class ConditionParser
	{
		private readonly ConditionTokenizer tokenizer;
		private readonly ConfigurationItemFactory configurationItemFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionParser"/> class.
		/// </summary>
		/// <param name="stringReader">The string reader.</param>
		/// <param name="configurationItemFactory">Instance of <see cref="ConfigurationItemFactory"/> used to resolve references to condition methods and layout renderers.</param>
		private ConditionParser(SimpleStringReader stringReader, ConfigurationItemFactory configurationItemFactory)
		{
			this.configurationItemFactory = configurationItemFactory;
			this.tokenizer = new ConditionTokenizer(stringReader);
		}

		/// <summary>
		/// Parses the specified condition string and turns it into
		/// <see cref="ConditionExpression"/> tree.
		/// </summary>
		/// <param name="expressionText">The expression to be parsed.</param>
		/// <returns>The root of the expression syntax tree which can be used to get the value of the condition in a specified context.</returns>
		public static ConditionExpression ParseExpression(string expressionText)
		{
			return ParseExpression(expressionText, ConfigurationItemFactory.Default);
		}

		/// <summary>
		/// Parses the specified condition string and turns it into
		/// <see cref="ConditionExpression"/> tree.
		/// </summary>
		/// <param name="expressionText">The expression to be parsed.</param>
		/// <param name="configurationItemFactories">Instance of <see cref="ConfigurationItemFactory"/> used to resolve references to condition methods and layout renderers.</param>
		/// <returns>The root of the expression syntax tree which can be used to get the value of the condition in a specified context.</returns>
		public static ConditionExpression ParseExpression(string expressionText, ConfigurationItemFactory configurationItemFactories)
		{
			if (expressionText == null)
			{
				return null;
			}

			var parser = new ConditionParser(new SimpleStringReader(expressionText), configurationItemFactories);
			ConditionExpression expression = parser.ParseExpression();
			if (!parser.tokenizer.IsEOF())
			{
				throw new ConditionParseException("Unexpected token: " + parser.tokenizer.TokenValue);
			}

			return expression;
		}

		/// <summary>
		/// Parses the specified condition string and turns it into
		/// <see cref="ConditionExpression"/> tree.
		/// </summary>
		/// <param name="stringReader">The string reader.</param>
		/// <param name="configurationItemFactories">Instance of <see cref="ConfigurationItemFactory"/> used to resolve references to condition methods and layout renderers.</param>
		/// <returns>
		/// The root of the expression syntax tree which can be used to get the value of the condition in a specified context.
		/// </returns>
		internal static ConditionExpression ParseExpression(SimpleStringReader stringReader, ConfigurationItemFactory configurationItemFactories)
		{
			var parser = new ConditionParser(stringReader, configurationItemFactories);
			ConditionExpression expression = parser.ParseExpression();
		
			return expression;
		}

		private ConditionMethodExpression ParsePredicate(string functionName)
		{
			var par = new List<ConditionExpression>();

			while (!this.tokenizer.IsEOF() && this.tokenizer.TokenType != ConditionTokenType.RightParen)
			{
				par.Add(ParseExpression());
				if (this.tokenizer.TokenType != ConditionTokenType.Comma)
				{
					break;
				}

				this.tokenizer.GetNextToken();
			}

			this.tokenizer.Expect(ConditionTokenType.RightParen);

			try
			{
				var methodInfo = this.configurationItemFactory.ConditionMethods.CreateInstance(functionName);
				return new ConditionMethodExpression(functionName, methodInfo, par);
			}
			catch (Exception exception)
			{
				if (exception.MustBeRethrown())
				{
					throw;
				}

				throw new ConditionParseException("Cannot resolve function '" + functionName + "'", exception);
			}
		}

		private ConditionExpression ParseLiteralExpression()
		{
			if (this.tokenizer.IsToken(ConditionTokenType.LeftParen))
			{
				this.tokenizer.GetNextToken();
				ConditionExpression e = this.ParseExpression();
				this.tokenizer.Expect(ConditionTokenType.RightParen);
				return e;
			}

			if (this.tokenizer.IsToken(ConditionTokenType.Minus))
			{
				this.tokenizer.GetNextToken();
				if (!this.tokenizer.IsNumber())
				{
					throw new ConditionParseException("Number expected, got " + this.tokenizer.TokenType);
				}

				string numberString = this.tokenizer.TokenValue;
				this.tokenizer.GetNextToken();
				if (numberString.IndexOf('.') >= 0)
				{
					return new ConditionLiteralExpression(-double.Parse(numberString, CultureInfo.InvariantCulture));
				}

				return new ConditionLiteralExpression(-int.Parse(numberString, CultureInfo.InvariantCulture));
			}

			if (this.tokenizer.IsNumber())
			{
				string numberString = this.tokenizer.TokenValue;
				this.tokenizer.GetNextToken();
				if (numberString.IndexOf('.') >= 0)
				{
					return new ConditionLiteralExpression(double.Parse(numberString, CultureInfo.InvariantCulture));
				}

				return new ConditionLiteralExpression(int.Parse(numberString, CultureInfo.InvariantCulture));
			}

			if (this.tokenizer.TokenType == ConditionTokenType.String)
			{
				ConditionExpression e = new ConditionLayoutExpression(new SimpleLayout(tokenizer.StringTokenValue, configurationItemFactory));
				this.tokenizer.GetNextToken();
				return e;
			}

			if (this.tokenizer.TokenType == ConditionTokenType.Keyword)
			{
				string keyword = this.tokenizer.EatKeyword();

				if (0 == string.Compare(keyword, "level", StringComparison.OrdinalIgnoreCase))
				{
					return new ConditionLevelExpression();
				}

				if (0 == string.Compare(keyword, "logger", StringComparison.OrdinalIgnoreCase))
				{
					return new ConditionLoggerNameExpression();
				}

				if (0 == string.Compare(keyword, "message", StringComparison.OrdinalIgnoreCase))
				{
					return new ConditionMessageExpression();
				}

				if (0 == string.Compare(keyword, "loglevel", StringComparison.OrdinalIgnoreCase))
				{
					this.tokenizer.Expect(ConditionTokenType.Dot);
					return new ConditionLiteralExpression(LogLevel.FromString(this.tokenizer.EatKeyword()));
				}

				if (0 == string.Compare(keyword, "true", StringComparison.OrdinalIgnoreCase))
				{
					return new ConditionLiteralExpression(true);
				}

				if (0 == string.Compare(keyword, "false", StringComparison.OrdinalIgnoreCase))
				{
					return new ConditionLiteralExpression(false);
				}

				if (0 == string.Compare(keyword, "null", StringComparison.OrdinalIgnoreCase))
				{
					return new ConditionLiteralExpression(null);
				}

				if (this.tokenizer.TokenType == ConditionTokenType.LeftParen)
				{
					this.tokenizer.GetNextToken();

					ConditionMethodExpression predicateExpression = this.ParsePredicate(keyword);
					return predicateExpression;
				}
			}

			throw new ConditionParseException("Unexpected token: " + this.tokenizer.TokenValue);
		}

		private ConditionExpression ParseBooleanRelation()
		{
			ConditionExpression e = this.ParseLiteralExpression();

			if (this.tokenizer.IsToken(ConditionTokenType.EqualTo))
			{
				this.tokenizer.GetNextToken();
				return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Equal);
			}

			if (this.tokenizer.IsToken(ConditionTokenType.NotEqual))
			{
				this.tokenizer.GetNextToken();
				return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.NotEqual);
			}

			if (this.tokenizer.IsToken(ConditionTokenType.LessThan))
			{
				this.tokenizer.GetNextToken();
				return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Less);
			}

			if (this.tokenizer.IsToken(ConditionTokenType.GreaterThan))
			{
				this.tokenizer.GetNextToken();
				return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.Greater);
			}

			if (this.tokenizer.IsToken(ConditionTokenType.LessThanOrEqualTo))
			{
				this.tokenizer.GetNextToken();
				return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.LessOrEqual);
			}

			if (this.tokenizer.IsToken(ConditionTokenType.GreaterThanOrEqualTo))
			{
				this.tokenizer.GetNextToken();
				return new ConditionRelationalExpression(e, this.ParseLiteralExpression(), ConditionRelationalOperator.GreaterOrEqual);
			}

			return e;
		}

		private ConditionExpression ParseBooleanPredicate()
		{
			if (this.tokenizer.IsKeyword("not") || this.tokenizer.IsToken(ConditionTokenType.Not))
			{
				this.tokenizer.GetNextToken();
				return new ConditionNotExpression(this.ParseBooleanPredicate());
			}

			return this.ParseBooleanRelation();
		}

		private ConditionExpression ParseBooleanAnd()
		{
			ConditionExpression expression = this.ParseBooleanPredicate();

			while (this.tokenizer.IsKeyword("and") || this.tokenizer.IsToken(ConditionTokenType.And))
			{
				this.tokenizer.GetNextToken();
				expression = new ConditionAndExpression(expression, this.ParseBooleanPredicate());
			}

			return expression;
		}

		private ConditionExpression ParseBooleanOr()
		{
			ConditionExpression expression = this.ParseBooleanAnd();

			while (this.tokenizer.IsKeyword("or") || this.tokenizer.IsToken(ConditionTokenType.Or))
			{
				this.tokenizer.GetNextToken();
				expression = new ConditionOrExpression(expression, this.ParseBooleanAnd());
			}

			return expression;
		}

		private ConditionExpression ParseBooleanExpression()
		{
			return this.ParseBooleanOr();
		}

		private ConditionExpression ParseExpression()
		{
			return this.ParseBooleanExpression();
		}
	}
}