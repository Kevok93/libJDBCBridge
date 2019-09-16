using System;
using System.IO;
using System.Runtime.InteropServices;

namespace libJDBCBridge.JavaBridge {
	public static class NativeCodeLocator {
		
		public static T BindMethod<T>(
			this PlatformLocator loc, 
			IntPtr library, 
			string name
		) {
			IntPtr addr = loc.FindMethod(library, name);
			return Marshal.GetDelegateForFunctionPointer<T>(addr);
		}

		public static readonly PlatformLocator platformLocator = getPlatformLocator();

		private static PlatformLocator getPlatformLocator() {
			switch (System.Environment.OSVersion.Platform) {
				case PlatformID.MacOSX: 
				case PlatformID.Unix: 
					return new Linux();
				default:
					return null;
			}
		}
		
		public interface PlatformLocator {
			IntPtr OpenLibrary(string path);
			IntPtr FindMethod(IntPtr library, string name);
		}
		
		private class Linux : PlatformLocator {
			[DllImport("libdl.so")]
			private static extern IntPtr dlopen(string filename, dlopenFlags flags);

			[DllImport("libdl.so")]
			private static extern IntPtr dlsym(IntPtr handle, string symbol);
			
			[DllImport("libdl.so")]
			private static extern string dlerror();

			public IntPtr OpenLibrary(string path) {
				IntPtr res = dlopen(
					path, 
					dlopenFlags.RTLD_NOW | dlopenFlags.RTLD_NODELETE
				);

				if (res == IntPtr.Zero) {
					string reason = dlerror();
					throw new Exception(reason);
				}
				return res;
			}

			public IntPtr FindMethod(IntPtr library, string name) {
				IntPtr res = dlsym(library, name);
				if (res == IntPtr.Zero) {
					string reason = dlerror();
					throw new Exception(reason);
				}

				return res;
			}

			
			[Flags]
			public enum dlopenFlags {
				RTLD_LAZY          = 0x0001, /* Lazy function call binding.  */
				RTLD_NOW           = 0x0002, /* Immediate function call binding.  */
				RTLD_BINDING_MASK  = 0x0003, /* Mask of binding time value.  */
				RTLD_NOLOAD        = 0x0004, /* Do not load the object.  */
				RTLD_DEEPBIND      = 0x0008, /* Use deep binding.  */
				RTLD_GLOBAL        = 0x0100,
				RTLD_LOCAL         = 0x0000,
				RTLD_NODELETE      = 0x1000
			} 
				
		}
		
	}
}