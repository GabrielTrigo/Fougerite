using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Fougerite.Tools
{
    /// <summary>
    /// Provides high-performance shallow cloning for objects of type <typeparamref name="T"/> 
    /// by generating MSIL at runtime via <see cref="DynamicMethod"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to clone.</typeparam>
    public static class Cloner<T>
    {
        /// <summary>
        /// Cached delegate containing the IL implementation for cloning the specific type.
        /// </summary>
        private static readonly Func<T, T> ClonerFunc = CreateCloner();

        /// <summary>
        /// Generates a <see cref="DynamicMethod"/> that instantiates a new object and 
        /// performs a field-by-field copy from the source.
        /// </summary>
        /// <returns>A compiled delegate for cloning objects of type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="T"/> does not have a parameterless constructor.</exception>
        private static Func<T, T> CreateCloner()
        {
            DynamicMethod cloneMethod = new DynamicMethod("CloneImplementation", typeof(T), new[] { typeof(T) }, true);
            ConstructorInfo defaultCtor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new Type[] { }, null);

            ILGenerator generator = cloneMethod.GetILGenerator();

            LocalBuilder loc1 = generator.DeclareLocal(typeof(T));

            // Instantiate the new object
            generator.Emit(OpCodes.Newobj, defaultCtor);
            generator.Emit(OpCodes.Stloc, loc1);

            // Iterate through all instance fields (public and private) and copy values
            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                generator.Emit(OpCodes.Ldloc, loc1);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Stfld, field);
            }

            generator.Emit(OpCodes.Ldloc, loc1);
            generator.Emit(OpCodes.Ret);

            return ((Func<T, T>)cloneMethod.CreateDelegate(typeof(Func<T, T>)));
        }

        /// <summary>
        /// Performs a shallow copy of the specified object.
        /// </summary>
        /// <param name="myObject">The source object to clone.</param>
        /// <returns>A new instance of <typeparamref name="T"/> with fields copied from the source.</returns>
        public static T Clone(T myObject)
        {
            return ClonerFunc(myObject);
        }
    }
}