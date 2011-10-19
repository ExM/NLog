
#if !NUNIT

namespace NUnit.Framework
{
	using System;
	using MSAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

	internal class Assert
	{
		public static void AreSame(object expected, object actual)
		{
			MSAssert.AreSame(expected, actual);
		}

		public static void AreSame(object expected, object actual, string message)
		{
			MSAssert.AreSame(expected, actual, message);
		}

		public static void AreNotSame(object expected, object actual)
		{
			MSAssert.AreNotSame(expected, actual);
		}

		public static void AreNotSame(object expected, object actual, string message)
		{
			MSAssert.AreNotSame(expected, actual, message);
		}

		public static void AreEqual(object expected, object actual)
		{
			MSAssert.AreEqual(expected, actual);		  
		}

		public static void AreEqual(object expected, object actual, string message)
		{
			MSAssert.AreEqual(expected, actual, message);
		}

		public static void AreNotEqual(object expected, object actual)
		{
			MSAssert.AreNotEqual(expected, actual);
		}

		public static void AreNotEqual(object expected, object actual, string message)
		{
			MSAssert.AreNotEqual(expected, actual, message);
		}

		public static void IsNull(object value)
		{
			MSAssert.IsNull(value);
		}

		public static void IsNull(object value, string message)
		{
			MSAssert.IsNull(value, message);
		}

		public static void IsNotNull(object value)
		{
			MSAssert.IsNotNull(value);
		}

		public static void IsNotNull(object value, string message)
		{
			MSAssert.IsNotNull(value, message);
		}

		public static void IsTrue(bool value)
		{
			MSAssert.IsTrue(value);
		}

		public static void IsTrue(bool value, string message)
		{
			MSAssert.IsTrue(value, message);
		}

		public static void IsInstanceOfType(Type type, object value)
		{
			MSAssert.IsInstanceOfType(value, type);
		}

		public static void Fail(string errorMessage)
		{
			MSAssert.Fail(errorMessage);
		}

		public static void IsFalse(bool value)
		{
			MSAssert.IsFalse(value);
		}

		public static void IsFalse(bool value, string message)
		{
			MSAssert.IsFalse(value, message);
		}
	}
}

#endif