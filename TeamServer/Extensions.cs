using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using dnlib.DotNet;

namespace TeamServer
{
    public static class Extensions
    {
        private static readonly JsonSerializerOptions Options = new() {PropertyNameCaseInsensitive = true};

        public static T Deserialize<T>(this byte[] bytes)
            => JsonSerializer.Deserialize<T>(bytes, Options);

        public static T Deserialize<T>(this string json)
            => JsonSerializer.Deserialize<T>(json, Options);

        public static byte[] Serialize<T>(this T data)
            => JsonSerializer.SerializeToUtf8Bytes(data, Options);

        public static string ToShortGuid(this Guid guid)
            => guid.ToString().Replace("-", "")[..10];

        public static TypeDef GetType(this IEnumerable<TypeDef> types, string name)
            => types.FirstOrDefault(t => t.Name == name);

        public static MethodDef GetMethod(this IEnumerable<MethodDef> methods, string name)
            => methods.FirstOrDefault(m =>
                m.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        public static MethodDef GetConstructor(this IEnumerable<MethodDef> methods)
            => methods.GetMethod(".ctor");

        public static ModuleDefMD ConvertModuleToExe(this ModuleDefMD module)
        {
            module.Kind = ModuleKind.Console;

            var program = module.Types.GetType("Program");
            var main = program?.Methods.GetMethod("Main");

            module.EntryPoint = main;

            return module;
        }
        
        public static ModuleDefMD AddUnmanagedExport(this ModuleDefMD module, string exportName)
        {
            var program = module.Types.GetType("Program");
            var execute = program?.Methods.GetMethod("Execute");
            if (execute is null) return module;
            
            execute.ExportInfo = new MethodExportInfo(exportName);
            execute.IsUnmanagedExport = true;
            
            var type = execute.MethodSig.RetType;
            type = new CModOptSig(module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CallConvStdcall"), type);
            execute.MethodSig.RetType = type;

            return module;
        }
    }
}