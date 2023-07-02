using System;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Entities.CodeGeneratedJobForEach;



namespace CellEngine.Utilities
{
    public static class EntitiesExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetSingleton<T>(this ForEachLambdaJobDescription description)
        {
            if (description.TryGetSingleton(out T singleton)) {
                return singleton;
            }

            throw new Exception($"Singleton of type {nameof(T)} does not exist!");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSingleton<T>(this ForEachLambdaJobDescription description, out T singleton, bool errorMultiple = true)
        {
            T    value = default;
            bool set   = false;
            description.WithAll<T>().ForEach((T v) =>
            {
                if (set && errorMultiple) { throw new Exception($"Multiple entities with component of type {nameof(T)} exist!"); }

                set   = true;
                value = v;
            }).WithoutBurst().Run();

            singleton = value;
            return set;
        }
    }
}
