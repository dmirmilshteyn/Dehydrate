﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Runtime.CompilerServices;

namespace Dehydrate
{
    public class Dehydrator
    {
        public void Dehydrate(IReadOnlyList<string> targetAssemblies, string outputDirectory) {
            if (!Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory(outputDirectory);
            }

            foreach (var assembly in targetAssemblies) {
                ProcessAssembly(assembly, Path.Combine(outputDirectory, Path.GetFileName(assembly)));
            }
        }

        private void ProcessAssembly(string assemblyPath, string outputPath) {
            using (ModuleDefMD module = ModuleDefMD.Load(assemblyPath)) {
                TypeRef attrRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "ReferenceAssemblyAttribute");
                var ctorRef = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), attrRef);

                module.Assembly.CustomAttributes.Add(new CustomAttribute(ctorRef));

                foreach (var type in EnumerateTypes(module.Types)) {
                    if (type.IsInterface) {
                        continue;
                    }
                    // Rules (for each type in the module):
                    // * Remove all private fields
                    // * Remove all private methods
                    // * Clear the bodies for all public members
                    // * Remove all private property getters
                    // * Remove all private property setters
                    // * Remove all properties without either a getter or setter 

                    // Process fields
                    for (int i = type.Fields.Count - 1; i >= 0; i--) {
                        if (!type.Fields[i].IsPublic) {
                            type.Fields.RemoveAt(i);
                        }
                    }

                    // Process methods
                    for (int i = type.Methods.Count - 1; i >= 0; i--) {
                        if (type.Methods[i].IsPublic) {
                            ClearMethodBody(type.Methods[i]);
                        } else {
                            type.Methods.RemoveAt(i);
                        }
                    }

                    // Process properties
                    for (int i = type.Properties.Count - 1; i >= 0; i--) {
                        bool remove = true;

                        // Go through all getter methods
                        for (int g = type.Properties[i].GetMethods.Count - 1; g >= 0; g--) {
                            if (type.Properties[i].GetMethods[g].IsPublic) {
                                remove = false;
                                ClearMethodBody(type.Properties[i].GetMethods[g]);
                            } else {
                                type.Properties[i].GetMethods.RemoveAt(g);
                            }
                        }

                        // Go through all setter methods
                        for (int s = type.Properties[i].SetMethods.Count - 1; s >= 0; s--) {
                            if (type.Properties[i].SetMethods[s].IsPublic) {
                                remove = false;
                                ClearMethodBody(type.Properties[i].SetMethods[s]);
                            } else {
                                type.Properties[i].SetMethods.RemoveAt(s);
                            }
                        }

                        // Remove this property if no methods were found
                        if (remove) {
                            type.Properties.RemoveAt(i);
                        }
                    }
                }

                module.Write(outputPath);
            }
        }

        private IEnumerable<TypeDef> EnumerateTypes(IEnumerable<TypeDef> types) {
            foreach (var type in types) {
                yield return type;
                foreach (var nestedType in EnumerateTypes(type.NestedTypes)) {
                    yield return nestedType;
                }
            }
        }

        private void ClearMethodBody(MethodDef method) {
            if (method.Body != null) {
                method.Body.ExceptionHandlers.Clear();
                method.Body.Variables.Clear();
                method.Body.Instructions.Clear();
                method.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                method.Body.InitLocals = false;
            }
        }
    }
}
