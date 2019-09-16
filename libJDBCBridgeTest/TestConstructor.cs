using System;
using NUnit.Framework;

namespace libJDBCBridge {

	public static class TestConstructor {
		public static T testBuildObject<T>(Func<T> ctor) {
			try { return ctor.Invoke(); } 
			catch (Exception e) {
				string tName = typeof(T).Name;
				throw new InconclusiveException(
					message: $"Could not initialize a {tName}",
					inner: e
				);
			}
			
		}

		[Test]
		public static void testTestBuildObject() {
			String t1 = testBuildObject(() => "Test1");
			Assert.AreEqual("Test1", t1);

			String t2 = null;
			Exception e = Assert.Catch(() => {
				t2 = testBuildObject(() => {
					throw new Exception("Test");
					return "Test2";
				});
			});
			Assert.IsNull(t2);
			Assert.IsNotNull(e);
			Assert.IsInstanceOf<InconclusiveException>(e);
			Assert.IsInstanceOf<Exception>(e.InnerException);
			Assert.AreEqual("Test", e.InnerException.Message);
			Assert.That(e.Message, Contains.Substring(typeof(string).Name));

		}
	}
}