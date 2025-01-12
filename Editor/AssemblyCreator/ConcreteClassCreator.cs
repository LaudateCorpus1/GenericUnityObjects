﻿namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Emits and saves an assembly of a concrete class that inherits from a specific generic UnityEngine.Object
    /// </summary>
    internal static class ConcreteClassCreator
    {
        /// <summary>
        /// Not supposed to be used directly. Instead, use <see cref="AssemblyCreator.CreateConcreteClass{TObject}"/>.
        /// </summary>
        public static void CreateConcreteClass<TObject>(string assemblyName, Type genericTypeWithArgs, string assemblyGUID)
            where TObject : Object
        {
            string concreteClassName = $"ConcreteClass_{assemblyGUID}";

            AssemblyBuilder assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericTypeWithArgs);

            AssemblyCreatorHelper.AddChildrenAttributes(typeBuilder, genericTypeWithArgs);

            if (typeof(TObject) == typeof(MonoBehaviour))
            {
                AssemblyCreatorHelper.AddComponentMenuAttribute(typeBuilder, genericTypeWithArgs);
            }

            typeBuilder.CreateType();
            assemblyBuilder.Save($"{assemblyName}.dll");
        }
    }
}