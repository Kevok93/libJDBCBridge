using System;
using System.Runtime.InteropServices;

namespace libJDBCBridge.JavaBridge {
	
	public unsafe class JavaEnvWrapper {
		internal JVM* _jvm;
		internal JNI* _env;
		internal JNI_VERSION version;

		internal static JVM* JVMInstance {get; set;} 


		private JavaEnvWrapper(
			JNI_VERSION  version = JNI_VERSION.v1_6,
			int          nOptions = 0,
			IntPtr       options = default,
			byte         ignoreUnrecognized = 0,
			string       threadName = null
		) {
			lock (_JVMLinkLock) loadJavaLibrary();
			this.version = version;
			if (JVMInstance == null) {
				var args = new JavaVMInitArgs {
					version            = version,
					nOptions           = nOptions,
					options            = options,
					ignoreUnrecognized = ignoreUnrecognized
				};
				int retc = _JNI_CreateJavaVM(
					out _jvm,
					out _env,
					ref args
				);
				JVMInstance = this._jvm;
			} else {
				this._jvm = JVMInstance;
				var namePtr = Marshal.StringToHGlobalAnsi(threadName);
				var args = new JavaVMAttachArgs {
					version = version,
					name = namePtr,
					extraInfo = IntPtr.Zero
				};
				Marshal.GetDelegateForFunctionPointer<_AttachCurrentThread>(
					JVMInstance->fTable->AttachCurrentThread
				).Invoke(
						JVMInstance,
						out _env,
						ref args
				);
			}
			this._findClass         = Marshal.GetDelegateForFunctionPointer<_FindClassD         >(_env->fTable->FindClass            );
			this._GetStaticMethodID = Marshal.GetDelegateForFunctionPointer<_GetStaticMethodIDD >(_env->fTable->GetStaticMethodID    );
			this._CallStaticMethod  = Marshal.GetDelegateForFunctionPointer<_CallStaticMethodD  >(_env->fTable->CallStaticObjectMethodA);
		}

		private static object _JVMLinkLock = new object();
		internal static IntPtr? _JVMLib;
		internal static _JNI_CreateJavaVMD _JNI_CreateJavaVM;
		internal static void loadJavaLibrary(
			string path = null
		) {
			path = path ?? "/usr/lib/jvm/default-java/lib/server/libjvm.so";
			if (_JVMLib == null) {
				_JVMLib = NativeCodeLocator.platformLocator.OpenLibrary(path);
				_JNI_CreateJavaVM = NativeCodeLocator
					.platformLocator
					.BindMethod<_JNI_CreateJavaVMD>(
						library: _JVMLib.Value, 
						name: "JNI_CreateJavaVM"
				);
			}
		}		
		
		internal delegate int _JNI_CreateJavaVMD(
			out JVM*           vm,
			out JNI*           env, 
			ref JavaVMInitArgs args
		);
		


		public static JavaEnvWrapper init (
			JNI_VERSION    version            = JNI_VERSION.v1_6,
			string[]       options            = null,
			bool           ignoreUnrecognized = false
		) {
			IntPtr optionsPtr = IntPtr.Zero;
			if (options != null && options.Length > 0) {
				int optSize = Marshal.SizeOf<JavaVMOption>();
				//Leave the trash laying around cause this is a singleton
				//Also cause I don't know when java will try to read these
				optionsPtr = Marshal.AllocHGlobal(optSize * options.Length);
				var optArray = (JavaVMOption*) optionsPtr.ToPointer();
				optArray[1] = new JavaVMOption();
				for (int i = 0; i < options.Length; i++) {
					optArray[i].optionString = Marshal.StringToHGlobalAnsi(options[i]);
					optArray[i].extraInfo = IntPtr.Zero;
				}
			}
			var result = new JavaEnvWrapper(
				version: version,
				nOptions: options?.Length ?? 0,
				options: optionsPtr,
				ignoreUnrecognized: (byte) (ignoreUnrecognized ? 1 : 0)
			);
			return result;
		}

		public JavaEnvWrapper clone() {
			return new JavaEnvWrapper(
				version:this.version,
				threadName:"testThread"
			);
		}

		internal delegate int _AttachCurrentThread(
			JVM* jvm,
			out JNI* env,
			ref JavaVMAttachArgs args
		);
		
		internal delegate JavaClass _FindClassD(
			JNI* env, 
			char[] name
		);
		internal _FindClassD _findClass { get; }
		public JavaClass FindClass(
			string name
		) => this._findClass(
			this._env,
			name.ToCharArray()
		);

		internal delegate JavaMethod _GetStaticMethodIDD(
			JNI* env, 
			JavaClass clazz, 
			char[] name,
			char[] signature
		);
		internal _GetStaticMethodIDD _GetStaticMethodID { get; }
		public JavaMethod GetStaticMethodID(
			JavaClass clazz,
			string name,
			string signature
		) => this._GetStaticMethodID(
			this._env,
			clazz,
			name.ToCharArray(),
			signature.ToCharArray()
		);

		internal delegate ulong _CallStaticMethodD(
			JNI* env, 
			JavaClass clazz, 
			JavaMethod method,
			JavaValue[] args
		);
		internal _CallStaticMethodD _CallStaticMethod { get; }
		public JavaValue CallStaticMethod(
			JavaClass clazz,
			JavaMethod method,
			params JavaValue[] args
		) => this._CallStaticMethod(
			this._env,
			clazz,
			method,
			args
		);
	}
}