﻿namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using System.Reflection;
    using Editor.GeneratedTypesDatabase;
    using Editor.Util;
    using NUnit.Framework;
    using UnityEngine;
    using Util;

    internal partial class BehavioursGenerationDatabaseTests
    {
        public class OnAfterDeserialize : BehavioursGenerationDatabaseTests
        {
            private static FieldInfo _instanceField;
            private static object _getterInstance;

            [OneTimeSetUp]
            public void BeforeAllTests()
            {
                FieldInfo getterField = typeof(EditorOnlySingletonSO<GenerationDatabase<MonoBehaviour>>)
                    .GetField("Getter", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                Assert.IsNotNull(getterField);

                _getterInstance = getterField.GetValue(null);
                Assert.IsNotNull(_getterInstance);

                _instanceField = typeof(InstanceGetter<GenerationDatabase<MonoBehaviour>>)
                    .GetField("_instance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                Assert.IsNotNull(_instanceField);
            }

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();

                var cleanInstance = ScriptableObject.CreateInstance<BehavioursGenerationDatabase>();
                _instanceField.SetValue(_getterInstance, cleanInstance);
                GenerationDatabase<MonoBehaviour>.Instance.Initialize();
                GenerationDatabase<MonoBehaviour>.AddGenericType(_behaviour);
                GenerationDatabase<MonoBehaviour>.Instance.AddConcreteClassImpl(_behaviour, _firstSecondArgs, AssemblyGUID);
            }

            private static void ReserializeDatabase()
            {
                GenerationDatabase<MonoBehaviour>.Instance.OnBeforeSerialize();
                GenerationDatabase<MonoBehaviour>.Instance.OnAfterDeserialize();
            }

            [Test]
            public void Restores_arguments()
            {
                ReserializeDatabase();
                Assert.IsTrue(GenerationDatabase<MonoBehaviour>.Arguments.SequenceEqual(_firstSecondArgs));
            }

            [Test]
            public void Restores_behaviours()
            {
                ReserializeDatabase();
                Assert.IsTrue(GenerationDatabase<MonoBehaviour>.GenericTypes.SequenceEqual(_expectedBehaviours));
            }

            [Test]
            public void Restores_referenced_behaviours()
            {
                ReserializeDatabase();
                GenericTypeInfo[] referencedBehaviours = GenerationDatabase<MonoBehaviour>.GetReferencedGenericTypes(_firstArg);
                Assert.IsTrue(referencedBehaviours.SequenceEqual(_expectedBehaviours));
            }

            [Test]
            public void Restores_concrete_classes()
            {
                ReserializeDatabase();
                var concreteClasses = GenerationDatabase<MonoBehaviour>.GetConcreteClasses(_behaviour);
                Assert.IsTrue(concreteClasses.Length == 1);
                Assert.IsTrue(concreteClasses[0] == _expectedConcreteClass);
            }
        }
    }
}