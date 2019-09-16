using System;
using NUnit.Framework;
using static libJDBCBridge.TestConstructor;

namespace libJDBCBridge.JavaBridge {
	public unsafe class JavaEnvWrapperTest {

		public JavaEnvWrapper wrapper;
		public Exception jvmInitError = null;
		[Test]
		[SetUp]
		public void initLibrary() {
			JavaEnvWrapper.loadJavaLibrary();

			Assert.IsNotNull(JavaEnvWrapper._JVMLib);
			Assert.AreNotEqual(IntPtr.Zero, JavaEnvWrapper._JVMLib);
			Assert.IsNotNull(JavaEnvWrapper._JNI_CreateJavaVM);
			
			//Do the jvm init here I guess, but don't check it until later
			try {wrapper = JavaEnvWrapper.init();} 
			catch (Exception e) {this.jvmInitError = e;}
		}
		
		[Test]
		public void initJvm() {
			if (this.jvmInitError != null) throw this.jvmInitError;
			Assert.IsNotNull(wrapper);
			Assert.IsNotNull(wrapper._findClass);
			Assert.IsNotNull(wrapper._CallStaticMethod);
			Assert.IsNotNull(wrapper._GetStaticMethodID);
			Assert.AreNotEqual(IntPtr.Zero, (IntPtr)wrapper._env);
			Assert.AreNotEqual(IntPtr.Zero, (IntPtr)wrapper._jvm);
		}
		[Test]
		public void findClass() {
			assertJVMLoaded(wrapper);
			JavaClass? clazz = null;
			Assert.DoesNotThrow(
				() => clazz = this.wrapper.FindClass("java/lang/Integer") 
			);
			Assert.That(clazz.HasValue);
			Assert.AreNotEqual(IntPtr.Zero, clazz.Value.value);
		}
		
		[Test]
		public void getStaticMethodID() {
			assertJVMLoaded(wrapper);
			JavaClass clazz = testBuildObject(() => this.wrapper.FindClass("java/lang/Integer"));
			JavaMethod? method = null;
			Assert.DoesNotThrow(
				() => method = this.wrapper.GetStaticMethodID(
					clazz,
					"sum",
					"(II)I"
				) 
			);
			Assert.That(method.HasValue);
			Assert.AreNotEqual(IntPtr.Zero, method.Value.value);
		}
		
		[Test]
		public void callStaticMethod() {
			assertJVMLoaded(wrapper);
			JavaClass   clazz  = testBuildObject(() => this.wrapper.FindClass("java/lang/Integer"));
			JavaMethod  method = testBuildObject(() => this.wrapper.GetStaticMethodID(
				clazz,
				"sum",
				"(II)I"
			));
			JavaValue? res = null;
			Assert.DoesNotThrow(
				() => res = this.wrapper.CallStaticMethod(
					clazz: clazz,
					method: method,
					5,
					7
				)
			);
			Assert.AreEqual(12, res.Value.intValue);
		}
		
		[Test]
		public void callStaticMethodWithFloats() {
			assertJVMLoaded(wrapper);
			JavaClass clazz = testBuildObject(() => this.wrapper.FindClass("java/lang/Math"));
			JavaMethod method = testBuildObject(() => this.wrapper.GetStaticMethodID(
				clazz,
				"floor",
				"(D)D"
			));
			JavaValue? res = null;
			Assert.DoesNotThrow(
				() => res = this.wrapper.CallStaticMethod(
					clazz: clazz,
					method: method,
					5.5
				)
			);
			Assert.AreEqual(5, res.Value.doubleValue, 0.1);
		}
		
		[Test]
		public void callStaticMethodWithChars() {
			assertJVMLoaded(wrapper);
			JavaClass clazz = testBuildObject(() => this.wrapper.FindClass("java/lang/Character"));
			JavaMethod method = testBuildObject(() => this.wrapper.GetStaticMethodID(
				clazz,
				"forDigit",
				"(II)C"
			));
			JavaValue? res = null;
			Assert.DoesNotThrow(
				() => res = this.wrapper.CallStaticMethod(
					clazz: clazz,
					method: method,
					10,
					16
				)
			);
			Assert.AreEqual('a', res.Value.charValue);
		}
		
		public void assertJVMLoaded(JavaEnvWrapper env) {
			if (env == null) throw new InconclusiveException(
				message: "Java env did not init, marking inconclusive",
				inner:jvmInitError
			);
		}
		
		
	}
}