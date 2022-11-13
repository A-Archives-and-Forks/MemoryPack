﻿using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

// Array and Array-like type formatters
// T[]
// T[] where T: unmnaged
// Memory
// ReadOnlyMemory
// ArraySegment
// ReadOnlySequence

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> ArrayLikeFormatters = new Dictionary<Type, Type>(4)
        {
            // If T[], choose UnmanagedArrayFormatter or DangerousUnmanagedTypeArrayFormatter or ArrayFormatter
            { typeof(ArraySegment<>), typeof(ArraySegmentFormatter<>) },
            { typeof(Memory<>), typeof(MemoryFormatter<>) },
            { typeof(ReadOnlyMemory<>), typeof(ReadOnlyMemoryFormatter<>) },
            { typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceFormatter<>) },
        };
    }
}

namespace MemoryPack.Formatters
{
    [Preserve]
    public sealed class UnmanagedArrayFormatter<T> : MemoryPackFormatter<T[]>
        where T : unmanaged
    {
        public static readonly UnmanagedArrayFormatter<T> Default = new UnmanagedArrayFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T[]? value)
        {
            writer.WriteUnmanagedArray(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref T[]? value)
        {
            reader.ReadUnmanagedArray<T>(ref value);
        }
    }

    [Preserve]
    public sealed class DangerousUnmanagedArrayFormatter<T> : MemoryPackFormatter<T[]>
    {
        public static readonly DangerousUnmanagedArrayFormatter<T> Default = new DangerousUnmanagedArrayFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T[]? value)
        {
            writer.DangerousWriteUnmanagedArray(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref T[]? value)
        {
            reader.DangerousReadUnmanagedArray<T>(ref value);
        }
    }

    [Preserve]
    public sealed class ArrayFormatter<T> : MemoryPackFormatter<T?[]>
    {
        public static readonly ArrayFormatter<T> Default = new ArrayFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref T?[]? value)
        {
            writer.WriteArray(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref T?[]? value)
        {
            reader.ReadArray(ref value);
        }
    }

    [Preserve]
    public sealed class ArraySegmentFormatter<T> : MemoryPackFormatter<ArraySegment<T?>>
    {
        public static readonly ArraySegmentFormatter<T> Default = new ArraySegmentFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ArraySegment<T?> value)
        {
            writer.WriteSpan(value.AsMemory().Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref ArraySegment<T?> value)
        {
            var array = reader.ReadArray<T>();
            value = (array == null) ? default : (ArraySegment<T?>)array;
        }
    }

    [Preserve]
    public sealed class MemoryFormatter<T> : MemoryPackFormatter<Memory<T?>>
    {
        public static readonly MemoryFormatter<T> Default = new MemoryFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref Memory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref Memory<T?> value)
        {
            value = reader.ReadArray<T>();
        }
    }

    [Preserve]
    public sealed class ReadOnlyMemoryFormatter<T> : MemoryPackFormatter<ReadOnlyMemory<T?>>
    {
        public static readonly ReadOnlyMemoryFormatter<T> Default = new ReadOnlyMemoryFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlyMemory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlyMemory<T?> value)
        {
            value = reader.ReadArray<T>();
        }
    }

    [Preserve]
    public sealed class ReadOnlySequenceFormatter<T> : MemoryPackFormatter<ReadOnlySequence<T?>>
    {
        public static readonly ReadOnlySequenceFormatter<T> Default = new ReadOnlySequenceFormatter<T>();

        [Preserve]
        public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, scoped ref ReadOnlySequence<T?> value)
        {
            if (value.IsSingleSegment)
            {
                writer.WriteSpan(value.FirstSpan);
                return;
            }

            writer.WriteCollectionHeader(checked((int)value.Length));
            foreach (var memory in value)
            {
                writer.WriteSpanWithoutLengthHeader(memory.Span);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, scoped ref ReadOnlySequence<T?> value)
        {
            var array = reader.ReadArray<T>();
            value = (array == null) ? default : new ReadOnlySequence<T?>(array);
        }
    }
}
