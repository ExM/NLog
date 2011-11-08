
namespace NLog.UnitTests.Conditions
{
	using NLog.Internal;
	using NUnit.Framework;
	using NLog.Conditions;
	using NLog.Config;
	using NLog.LayoutRenderers;
	using NLog.Layouts;

	[TestFixture]
	public class ConditionParserTests : NLogTestBase
	{
		[Test]
		public void ParseNullText()
		{
			Assert.IsNull(ConditionParser.ParseExpression(null, CommonCfg.ItemFactory));
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void ParseEmptyText()
		{
			ConditionParser.ParseExpression("", CommonCfg.ItemFactory);
		}

		[Test]
		public void ImplicitOperatorTest()
		{
			ConditionExpression cond = "true and true";

			Assert.IsInstanceOf<ConditionLazyExpression>(cond);
		}

		[Test]
		public void NullLiteralTest()
		{
			Assert.AreEqual("null", ConditionParser.ParseExpression("null", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void BooleanLiteralTest()
		{
			Assert.AreEqual("True", ConditionParser.ParseExpression("true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("True", ConditionParser.ParseExpression("tRuE", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("False", ConditionParser.ParseExpression("false", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("False", ConditionParser.ParseExpression("fAlSe", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void AndTest()
		{
			Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("true and true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("tRuE AND true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("(True and True)", ConditionParser.ParseExpression("tRuE && true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("true and true && true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("tRuE AND true and true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("((True and True) and True)", ConditionParser.ParseExpression("tRuE && true AND true", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void OrTest()
		{
			Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("true or true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("tRuE OR true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("(True or True)", ConditionParser.ParseExpression("tRuE || true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("true or true || true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("tRuE OR true or true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("((True or True) or True)", ConditionParser.ParseExpression("tRuE || true OR true", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void NotTest()
		{
			Assert.AreEqual("(not True)", ConditionParser.ParseExpression("not true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("(not (not True))", ConditionParser.ParseExpression("not not true", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("(not (not (not True)))", ConditionParser.ParseExpression("not not not true", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void StringTest()
		{
			Assert.AreEqual("''", ConditionParser.ParseExpression("''", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("'Foo'", ConditionParser.ParseExpression("'Foo'", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("'Bar'", ConditionParser.ParseExpression("'Bar'", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("'d'Artagnan'", ConditionParser.ParseExpression("'d''Artagnan'", CommonCfg.ItemFactory).ToString());

			var cle = ConditionParser.ParseExpression("'${message} ${level}'", CommonCfg.ItemFactory) as ConditionLayoutExpression;
			Assert.IsNotNull(cle);
			SimpleLayout sl = cle.Layout as SimpleLayout;
			Assert.IsNotNull(sl);
			Assert.AreEqual(3, sl.Renderers.Count);
			Assert.IsInstanceOf<MessageLayoutRenderer>(sl.Renderers[0]);
			Assert.IsInstanceOf<LiteralLayoutRenderer>(sl.Renderers[1]);
			Assert.IsInstanceOf<LevelLayoutRenderer>(sl.Renderers[2]);

		}

		[Test]
		public void LogLevelTest()
		{
			var result = ConditionParser.ParseExpression("LogLevel.Info", CommonCfg.ItemFactory) as ConditionLiteralExpression;
			Assert.IsNotNull(result);
			Assert.AreSame(LogLevel.Info, result.LiteralValue);

			result = ConditionParser.ParseExpression("LogLevel.Trace", CommonCfg.ItemFactory) as ConditionLiteralExpression;
			Assert.IsNotNull(result);
			Assert.AreSame(LogLevel.Trace, result.LiteralValue);
		}

		[Test]
		public void RelationalOperatorTest()
		{
			RelationalOperatorTest("=", "==");
			RelationalOperatorTest("==", "==");
			RelationalOperatorTest("!=", "!=");
			RelationalOperatorTest("<>", "!=");
			RelationalOperatorTest("<", "<");
			RelationalOperatorTest(">", ">");
			RelationalOperatorTest("<=", "<=");
			RelationalOperatorTest(">=", ">=");
		}

		[Test]
		public void NumberTest()
		{
			Assert.AreEqual("3.141592", ConditionParser.ParseExpression("3.141592", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("42", ConditionParser.ParseExpression("42", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("-42", ConditionParser.ParseExpression("-42", CommonCfg.ItemFactory).ToString());
			Assert.AreEqual("-3.141592", ConditionParser.ParseExpression("-3.141592", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void ExtraParenthesisTest()
		{
			Assert.AreEqual("3.141592", ConditionParser.ParseExpression("(((3.141592)))", CommonCfg.ItemFactory).ToString());
		}

		[Test]
		public void MessageTest()
		{
			var result = ConditionParser.ParseExpression("message", CommonCfg.ItemFactory);
			Assert.IsInstanceOf<ConditionMessageExpression>(result);
			Assert.AreEqual("message", result.ToString());
		}

		[Test]
		public void LevelTest()
		{
			var result = ConditionParser.ParseExpression("level", CommonCfg.ItemFactory);
			Assert.IsInstanceOf<ConditionLevelExpression>(result);
			Assert.AreEqual("level", result.ToString());
		}

		[Test]
		public void LoggerTest()
		{
			var result = ConditionParser.ParseExpression("logger", CommonCfg.ItemFactory);
			Assert.IsInstanceOf<ConditionLoggerNameExpression>(result);
			Assert.AreEqual("logger", result.ToString());
		}

		[Test]
		public void ConditionFunctionTests()
		{
			var result = ConditionParser.ParseExpression("starts-with(logger, 'x${message}')", CommonCfg.ItemFactory) as ConditionMethodExpression;
			Assert.IsNotNull(result);
			Assert.AreEqual("starts-with(logger, 'x${message}')", result.ToString());
			Assert.AreEqual("StartsWith", result.MethodInfo.Name);
			Assert.AreEqual(typeof(ConditionMethods), result.MethodInfo.DeclaringType);
		}

		[Test]
		public void CustomNLogFactoriesTest()
		{
			var configurationItemFactory = new ConfigurationItemFactory();
			configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
			configurationItemFactory.ConditionMethods.RegisterDefinition("check", typeof(MyConditionMethods).GetMethod("CheckIt"));

			ConditionParser.ParseExpression("check('${foo}')", configurationItemFactory);
		}

		[Test]
		public void MethodNameWithUnderscores()
		{
			var configurationItemFactory = new ConfigurationItemFactory();
			configurationItemFactory.LayoutRenderers.RegisterDefinition("foo", typeof(FooLayoutRenderer));
			configurationItemFactory.ConditionMethods.RegisterDefinition("__check__", typeof(MyConditionMethods).GetMethod("CheckIt"));

			ConditionParser.ParseExpression("__check__('${foo}')", configurationItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnbalancedParenthesis1Test()
		{
			ConditionParser.ParseExpression("check(", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnbalancedParenthesis2Test()
		{
			ConditionParser.ParseExpression("((1)", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnbalancedParenthesis3Test()
		{
			ConditionParser.ParseExpression("(1))", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void LogLevelWithoutAName()
		{
			ConditionParser.ParseExpression("LogLevel.'somestring'", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void InvalidNumberWithUnaryMinusTest()
		{
			ConditionParser.ParseExpression("-a31", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void InvalidNumberTest()
		{
			ConditionParser.ParseExpression("-123.4a", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnclosedString()
		{
			ConditionParser.ParseExpression("'Hello world", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnrecognizedToken()
		{
			ConditionParser.ParseExpression("somecompletelyunrecognizedtoken", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnrecognizedPunctuation()
		{
			ConditionParser.ParseExpression("#", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnrecognizedUnicodeChar()
		{
			ConditionParser.ParseExpression("\u0090", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnrecognizedUnicodeChar2()
		{
			ConditionParser.ParseExpression("\u0015", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void UnrecognizedMethod()
		{
			ConditionParser.ParseExpression("unrecognized-method()", CommonCfg.ItemFactory);
		}

		[Test]
		[ExpectedException(typeof(ConditionParseException))]
		public void TokenizerEOFTest()
		{
			var tokenizer = new ConditionTokenizer(new SimpleStringReader(string.Empty));
			tokenizer.GetNextToken();
		}

		private void RelationalOperatorTest(string op, string result)
		{
			string operand1 = "3";
			string operand2 = "7";

			string input = operand1 + " " + op + " " + operand2;
			string expectedOutput = "(" + operand1 + " " + result + " " + operand2 + ")";
			var condition = ConditionParser.ParseExpression(input, CommonCfg.ItemFactory);
			Assert.AreEqual(expectedOutput, condition.ToString());
		}

		public class FooLayoutRenderer : LayoutRenderer
		{
			protected override void Append(System.Text.StringBuilder builder, LogEventInfo logEvent)
			{
				throw new System.NotImplementedException();
			}
		}

		public class MyConditionMethods
		{
			public static bool CheckIt(string s)
			{
				return s == "X";
			}
		}
	}
}