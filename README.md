# libJDBCBridge {INCOMPLETE}
Library to provide interop between Java's well supported JDBC interface for the .NET runtime.

Current features: 
- the library is currently only capable of finding the libjni.so on a linux java install, under `/usr/lib/jvm/default-java`
- Library will dynamically link to this library at runtime
- Library is capable of creating a java VM, and will automatically create new JVM threads on subsuquent creations.
- Library is capable of finding Class ID's and Static Method ID's, and is capable of calling any Static Methods.

Soon™ features:
- Call instance methods
- Call constructors
- Create interop data channels for commands and non-primitive JDBC datatypes (eg. datetime/timestamp)

