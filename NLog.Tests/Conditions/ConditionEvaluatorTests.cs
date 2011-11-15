using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using NLog.Conditions;
using NLog.Config;
using NLog.Common;

namespace NLog.UnitTests.Conditions
{
	[TestFixture]
	public class ConditionEvaluatorTests : NLogTestBase
	{
		[Test]
		public void BooleanOperatorTest()
		{
			AssertEvaluationResult(false, "false or false");
			AssertEvaluationResult(true, "false or true");
			AssertEvaluationResult(true, "true or false");
			AssertEvaluationResult(true, "true or true");
			AssertEvaluationResult(false, "false and false");
			AssertEvaluationResult(false, "false and true");
			AssertEvaluationResult(false, "true and false");
			AssertEvaluationResult(true, "true and true");
			AssertEvaluationResult(false, "not true");
			AssertEvaluationResult(true, "not false");
			AssertEvaluationResult(false, "not not false");
			AssertEvaluationResult(true, "not not true");
		}

		[Test]
		public void ConditionMethodsTest()
		{
			AssertEvaluationResult(true, "starts-with('foobar','foo')");
			AssertEvaluationResult(false, "starts-with('foobar','bar')");
			AssertEvaluationResult(true, "ends-with('foobar','bar')");
			AssertEvaluationResult(false, "ends-with('foobar','foo')");
			AssertEvaluationResult(0, "length('')");
			AssertEvaluationResult(4, "length('${level}')");
			AssertEvaluationResult(false, "equals(1, 2)");
			AssertEvaluationResult(true, "equals(3.14, 3.14)");
			AssertEvaluationResult(true, "contains('foobar','ooba')");
			AssertEvaluationResult(false, "contains('foobar','oobe')");
			AssertEvaluationResult(false, "contains('','foo')");
			AssertEvaluationResult(true, "contains('foo','')");
		}

		[Test]
		public void ConditionMethodNegativeTest1()
		{
			try
			{
				AssertEvaluationResult(true, "starts-with('foobar')");
				Assert.Fail("Expected exception");
			}
			catch (ConditionParseException ex)
			{
				Assert.AreEqual("Cannot resolve function 'starts-with'", ex.Message);
				Assert.IsNotNull(ex.InnerException);
				Assert.AreEqual("Condition method 'starts-with' expects 2 parameters, but passed 1.", ex.InnerException.Message);
			}
		}

		[Test]
		public void ConditionMethodNegativeTest2()
		{
			try
			{
				AssertEvaluationResult(true, "starts-with('foobar','baz','qux','zzz')");
				Assert.Fail("Expected exception");
			}
			catch (ConditionParseException ex)
			{
				Assert.AreEqual("Cannot resolve function 'starts-with'", ex.Message);
				Assert.IsNotNull(ex.InnerException);
				Assert.AreEqual("Condition method 'starts-with' expects 2 parameters, but passed 4.", ex.InnerException.Message);
			}
		}

		[Test]
		public void LiteralTest()
		{
			AssertEvaluationResult(null, "null");
			AssertEvaluationResult(0, "0");
			AssertEvaluationResult(3, "3");
			AssertEvaluationResult(3.1415, "3.1415");
			AssertEvaluationResult(-1, "-1");
			AssertEvaluationResult(-3.1415, "-3.1415");
			AssertEvaluationResult(true, "true");
			AssertEvaluationResult(false, "false");
			AssertEvaluationResult(string.Empty, "''");
			AssertEvaluationResult("x", "'x'");
			AssertEvaluationResult("d'Artagnan", "'d''Artagnan'");
		}

		[Test]
		public void LogEventInfoPropertiesTest()
		{
			AssertEvaluationResult(LogLevel.Warn, "level");
			AssertEvaluationResult("some message", "message");
			AssertEvaluationResult("MyCompany.Product.Class", "logger");
		}

		[Test]
		public void RelationalOperatorTest()
		{
			AssertEvaluationResult(true, "1 < 2");
			AssertEvaluationResult(false, "1 < 1");

			AssertEvaluationResult(true, "2 > 1");
			AssertEvaluationResult(false, "1 > 1");

			AssertEvaluationResult(true, "1 <= 2");
			AssertEvaluationResult(false, "1 <= 0");

			AssertEvaluationResult(true, "2 >= 1");
			AssertEvaluationResult(false, "0 >= 1");

			AssertEvaluationResult(true, "2 == 2");
			AssertEvaluationResult(false, "2 == 3");

			AssertEvaluationResult(true, "2 != 3");
			AssertEvaluationResult(false, "2 != 2");

			AssertEvaluationResult(false, "1 < null");
			AssertEvaluationResult(true, "1 > null");

			AssertEvaluationResult(true, "null < 1");
			AssertEvaluationResult(false, "null > 1");

			AssertEvaluationResult(true, "null == null");
			AssertEvaluationResult(false, "null != null");

			AssertEvaluationResult(false, "null == 1");
			AssertEvaluationResult(false, "1 == null");

			AssertEvaluationResult(true, "null != 1");
			AssertEvaluationResult(true, "1 != null");
		}

		[Test]
		[ExpectedException(typeof(ConditionEvaluationException))]
		public void UnsupportedRelationalOperatorTest()
		{
			var cond = new ConditionRelationalExpression("true", "true", (ConditionRelationalOperator)(-1));
			cond.Evaluate(LogEventInfo.CreateNullEvent());
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void UnsupportedRelationalOperatorTest2()
		{
			var cond = new ConditionRelationalExpression("true", "true", (ConditionRelationalOperator)(-1));
			cond.ToString();
		}

		[Test]
		public void MethodWithLogEventInfoTest()
		{
			var factories = SetupConditionMethods();
			LoggingConfiguration cfg = new LoggingConfiguration(factories);
			Assert.AreEqual(true, ConditionParser.ParseExpression("IsValid()", cfg).Evaluate(CreateWellKnownContext()));
		}

		[Test]
		public void TypePromotionTest()
		{
			var factories = SetupConditionMethods();
			LoggingConfiguration cfg = new LoggingConfiguration(factories);

			AssertTypePromotionTest(true, "ToDateTime('2010/01/01') == '2010/01/01'", cfg);
			AssertTypePromotionTest(true, "ToInt64(1) == ToInt32(1)", cfg);
			AssertTypePromotionTest(true, "'42' == 42", cfg);
			AssertTypePromotionTest(true, "42 == '42'", cfg);
			AssertTypePromotionTest(true, "ToDouble(3) == 3", cfg);
			AssertTypePromotionTest(true, "3 == ToDouble(3)", cfg);
			AssertTypePromotionTest(true, "ToSingle(3) == 3", cfg);
			AssertTypePromotionTest(true, "3 == ToSingle(3)", cfg);
			AssertTypePromotionTest(true, "ToDecimal(3) == 3", cfg);
			AssertTypePromotionTest(true, "3 == ToDecimal(3)", cfg);
			AssertTypePromotionTest(true, "ToInt32(3) == ToInt16(3)", cfg);
			AssertTypePromotionTest(true, "ToInt16(3) == ToInt32(3)", cfg);
			AssertTypePromotionTest(true, "true == ToInt16(1)", cfg);
			AssertTypePromotionTest(true, "ToInt16(1) == true", cfg);

			AssertTypePromotionTest(false, "ToDateTime('2010/01/01') == '2010/01/02'", cfg);
			AssertTypePromotionTest(false, "ToInt64(1) == ToInt32(2)", cfg);
			AssertTypePromotionTest(false, "'42' == 43", cfg);
			AssertTypePromotionTest(false, "42 == '43'", cfg);
			AssertTypePromotionTest(false, "ToDouble(3) == 4", cfg);
			AssertTypePromotionTest(false, "3 == ToDouble(4)", cfg);
			AssertTypePromotionTest(false, "ToSingle(3) == 4", cfg);
			AssertTypePromotionTest(false, "3 == ToSingle(4)", cfg);
			AssertTypePromotionTest(false, "ToDecimal(3) == 4", cfg);
			AssertTypePromotionTest(false, "3 == ToDecimal(4)", cfg);
			AssertTypePromotionTest(false, "ToInt32(3) == ToInt16(4)", cfg);
			AssertTypePromotionTest(false, "ToInt16(3) == ToInt32(4)", cfg);
			AssertTypePromotionTest(false, "false == ToInt16(4)", cfg);
			AssertTypePromotionTest(false, "ToInt16(1) == false", cfg);
		}

		private void AssertTypePromotionTest(bool expected, string text, LoggingConfiguration cfg)
		{
			ConditionExpression condition = ConditionParser.ParseExpression(text, cfg);
			condition.Initialize(cfg);
			LogEventInfo context = CreateWellKnownContext();
			object actualResult = condition.Evaluate(context);
			Assert.AreEqual(expected, actualResult);
		}



		[Test]
		[ExpectedException(typeof(ConditionEvaluationException))]
		public void TypePromotionNegativeTest1()
		{
			var factories = SetupConditionMethods();
			LoggingConfiguration cfg = new LoggingConfiguration(factories);

			Assert.AreEqual(true, ConditionParser.ParseExpression("ToDateTime('2010/01/01') == '20xx/01/01'", cfg).Evaluate(CreateWellKnownContext()));
		}

		[Test]
		[ExpectedException(typeof(ConditionEvaluationException))]
		public void TypePromotionNegativeTest2()
		{
			var factories = SetupConditionMethods();
			LoggingConfiguration cfg = new LoggingConfiguration(factories);

			Assert.AreEqual(true, ConditionParser.ParseExpression("GetGuid() == ToInt16(1)", cfg).Evaluate(CreateWellKnownContext()));
		}

		[Test]
		public void ExceptionTest1()
		{
			var ex1 = new ConditionEvaluationException();
			Assert.IsNotNull(ex1.Message);
		}

		[Test]
		public void ExceptionTest2()
		{
			var ex1 = new ConditionEvaluationException("msg");
			Assert.AreEqual("msg", ex1.Message);
		}

		[Test]
		public void ExceptionTest3()
		{
			var inner = new InvalidOperationException("f");
			var ex1 = new ConditionEvaluationException("msg", inner);
			Assert.AreEqual("msg", ex1.Message);
			Assert.AreSame(inner, ex1.InnerException);
		}

		[Test]
		public void ExceptionTest4()
		{
			var inner = new InvalidOperationException("f");
			var ex1 = new ConditionEvaluationException("msg", inner);
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, ex1);
			ms.Position = 0;
			Exception ex2 = (Exception)bf.Deserialize(ms);

			Assert.AreEqual("msg", ex2.Message);
			Assert.AreEqual("f", ex2.InnerException.Message);
		}

		[Test]
		public void ExceptionTest11()
		{
			var ex1 = new ConditionParseException();
			Assert.IsNotNull(ex1.Message);
		}

		[Test]
		public void ExceptionTest12()
		{
			var ex1 = new ConditionParseException("msg");
			Assert.AreEqual("msg", ex1.Message);
		}

		[Test]
		public void ExceptionTest13()
		{
			var inner = new InvalidOperationException("f");
			var ex1 = new ConditionParseException("msg", inner);
			Assert.AreEqual("msg", ex1.Message);
			Assert.AreSame(inner, ex1.InnerException);
		}

		[Test]
		public void ExceptionTest14()
		{
			var inner = new InvalidOperationException("f");
			var ex1 = new ConditionParseException("msg", inner);
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, ex1);
			ms.Position = 0;
			Exception ex2 = (Exception)bf.Deserialize(ms);

			Assert.AreEqual("msg", ex2.Message);
			Assert.AreEqual("f", ex2.InnerException.Message);
		}

		private static ConfigurationItemFactory SetupConditionMethods()
		{
			var factories = new ConfigurationItemFactory();
			factories.ConditionMethods.RegisterDefinition("GetGuid", typeof(MyConditionMethods).GetMethod("GetGuid"));
			factories.ConditionMethods.RegisterDefinition("ToInt16", typeof(MyConditionMethods).GetMethod("ToInt16"));
			factories.ConditionMethods.RegisterDefinition("ToInt32", typeof(MyConditionMethods).GetMethod("ToInt32"));
			factories.ConditionMethods.RegisterDefinition("ToInt64", typeof(MyConditionMethods).GetMethod("ToInt64"));
			factories.ConditionMethods.RegisterDefinition("ToDouble", typeof(MyConditionMethods).GetMethod("ToDouble"));
			factories.ConditionMethods.RegisterDefinition("ToSingle", typeof(MyConditionMethods).GetMethod("ToSingle"));
			factories.ConditionMethods.RegisterDefinition("ToDateTime", typeof(MyConditionMethods).GetMethod("ToDateTime"));
			factories.ConditionMethods.RegisterDefinition("ToDecimal", typeof(MyConditionMethods).GetMethod("ToDecimal"));
			factories.ConditionMethods.RegisterDefinition("IsValid", typeof(MyConditionMethods).GetMethod("IsValid"));
			return factories;
		}

		private void AssertEvaluationResult(object expectedResult, string conditionText)
		{
			ConditionExpression condition = ConditionParser.ParseExpression(conditionText, CommonCfg);
			condition.Initialize(CommonCfg);
			LogEventInfo context = CreateWellKnownContext();
			object actualResult = condition.Evaluate(context);
			Assert.AreEqual(expectedResult, actualResult);
		}

		private static LogEventInfo CreateWellKnownContext()
		{
			var context = new LogEventInfo
			{
				Level = LogLevel.Warn,
				Message = "some message",
				LoggerName = "MyCompany.Product.Class"
			};

			return context;
		}

		/// <summary>
		/// Conversion methods helpful in covering type promotion logic
		/// </summary>
		public class MyConditionMethods
		{
			public static Guid GetGuid()
			{
				return new Guid("{40190B01-C9C0-4F78-AA5A-615E413742E1}");
			}

			public static short ToInt16(object v)
			{
				return Convert.ToInt16(v, CultureInfo.InvariantCulture);
			}

			public static int ToInt32(object v)
			{
				return Convert.ToInt32(v, CultureInfo.InvariantCulture);
			}

			public static long ToInt64(object v)
			{
				return Convert.ToInt64(v, CultureInfo.InvariantCulture);
			}

			public static float ToSingle(object v)
			{
				return Convert.ToSingle(v, CultureInfo.InvariantCulture);
			}

			public static decimal ToDecimal(object v)
			{
				return Convert.ToDecimal(v, CultureInfo.InvariantCulture);
			}

			public static double ToDouble(object v)
			{
				return Convert.ToDouble(v, CultureInfo.InvariantCulture);
			}

			public static DateTime ToDateTime(object v)
			{
				return Convert.ToDateTime(v, CultureInfo.InvariantCulture);
			}

			public static bool IsValid(LogEventInfo context)
			{
				return true;
			}
		}
	}
}