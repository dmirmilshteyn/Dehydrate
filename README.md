# Dehydrate

Dehydrate is a tool that generates .NET metadata-only reference assemblies, similar to the assemblies distributed with different framework versions. The generated assemblies only contain metadata describing the types and their members, with all other IL being stripped out.

Generated assemblies are made using the following rules:
* Remove all private fields
* Remove all private methods
* Clear the bodies for all public members
* Remove all private property getters
* Remove all private property setters
* Remove all properties without either a getter or setter 

Additionally, a [ReferenceAssemblyAttribute](https://msdn.microsoft.com/en-us/library/system.runtime.compilerservices.referenceassemblyattribute.aspx) is attached to the generated assembly.

## Usage

This tool is build as a DNX console application. Clone this repository, then run:
```
cd src/dehydrate
dnx run dehydrate [options]
```

Available options:
```
-a|--assemblies   The target assemblies to dehydrate
-o|--output       Output directory to store dehydrated assemblies
```

For example, to generate a metadata-only reference assembly for ```Test.dll```, run the following command:
```
dnx run dehydrate -a "path/to/Test.dll" -o "path/to/output/directory"
```

The generated assembly will be available at ```path/to/output/directory/Test.dll```
