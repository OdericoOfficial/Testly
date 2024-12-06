using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Testly.Reflection
{
    public static unsafe class FieldInfoExtensions
    {
        [StructLayout(LayoutKind.Explicit)]
        internal unsafe ref struct FieldDesc
        {
            [FieldOffset(0)]
            private readonly void* m_pMTOfEnclosingClass;
            [FieldOffset(8)]
            private readonly nint m_dword1;
            [FieldOffset(12)]
            private readonly nint m_dword2;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetFieldOffset(FieldInfo fieldInfo)
                => (int)(((FieldDesc*)(fieldInfo.FieldHandle.Value))->m_dword2 & 0x7FFFFFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetFieldValueUnsafe<TValue>(this FieldInfo fieldInfo, object target)
        {
            var offset = FieldDesc.GetFieldOffset(fieldInfo);
            var targetPtr = *(nint**)Unsafe.AsPointer(ref target);
            targetPtr += 1 + offset / 8;
            return Unsafe.AsRef<TValue>(targetPtr);
        }
    }
}