using Unity.Collections.LowLevel.Unsafe;
using Unity.Sentis;
using UnityEngine.Assertions;

namespace Unity.DeepPose.Core
{
    /// <summary>
    /// Can be used to manipulate tensors with varying sequence length while keeping the same pre-allocated buffer
    /// It handles 1D, 2D, 3D and 4D tensors
    /// </summary>
    class SequenceTensorFloat
    {
        public int SequenceLength => m_SequenceLength;
        public TensorShape Shape => m_TensorShape;

        TensorShape m_TensorShape;
        float[] m_Buffer;

        int m_BatchSize;
        int m_SequenceLength;
        int m_MaxSequenceLength;
        int m_FirstSize;
        int m_SecondSize;

        /// <summary>
        /// Creates a new instance for a 1D tensor
        /// </summary>
        /// <param name="sequenceLength">The initial length of the sequence dimension (tensor dim 0)</param>
        /// <param name="maxSequenceLength"></param>
        public SequenceTensorFloat(int sequenceLength, int maxSequenceLength) : this(-1, sequenceLength, maxSequenceLength)
        {
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="bachSize">The size of the batch dimension (tensor dim 0). Leave to -1 for a 1D tensor</param>
        /// <param name="sequenceLength">The initial length of the sequence dimension (tensor dim 1)</param>
        /// <param name="firstSize">The size of the first feature dimension (tensor dim 2). Leave to -1 for a 2D tensor</param>
        /// <param name="secondSize">The size of the second feature dimension (tensor dim 3). Leave to -1 for a 2D or 3D tensor</param>
        /// <param name="maxSequenceLength"></param>
        public SequenceTensorFloat(int bachSize, int sequenceLength, int maxSequenceLength, int firstSize = -1, int secondSize = -1)
        {
            Assert.IsTrue(maxSequenceLength >= sequenceLength, "Initial sequence length is longer than the maximum sequence length");

            if (bachSize == -1)
                Assert.AreEqual(-1, firstSize, "A 1D tensor must have both firstSize and secondSize set to -1");

            if (firstSize == -1)
                Assert.AreEqual(-1, secondSize, "A 2D tensor must have both firstSize and secondSize set to -1");

            m_BatchSize = bachSize;
            m_SequenceLength = sequenceLength;
            m_MaxSequenceLength = maxSequenceLength;
            m_FirstSize = firstSize;
            m_SecondSize = secondSize;

            AllocateBuffer();
            UpdateShape();
        }

        /// <summary>
        /// Allocate a new tensor using the pre-allocated buffer
        /// The caller is responsible for disposing that tensor
        /// </summary>
        /// <returns>A newly allocated tensor using this instance buffer</returns>
        public Tensor<float> AllocateTensor()
        {
            var tensor = new Tensor<float>(m_TensorShape, m_Buffer);
            return tensor;
        }

        /// <summary>
        /// Inserts a new sample at the given sequence index. This will shift all existing samples to preserve data.
        /// </summary>
        /// <param name="sequenceIdx">The index at which to insert the sample</param>
        public void InsertSample(int sequenceIdx)
        {
            Assert.IsTrue(m_SequenceLength < m_MaxSequenceLength - 1, "Max sequence length reached");
            Assert.IsTrue(sequenceIdx >= 0 && sequenceIdx <= m_SequenceLength, "Index out of range");

            var sourceOffset = GetOffset(sequenceIdx);
            var destinationOffset = GetOffset(sequenceIdx + 1);
            var copyCount = m_Buffer.Length - destinationOffset;

            unsafe
            {
                fixed (void* dstPtr = &m_Buffer[destinationOffset], srcPtr = &m_Buffer[sourceOffset])
                {
                    UnsafeUtility.MemMove(dstPtr, srcPtr, sizeof(float) * copyCount);
                }
            }

            m_SequenceLength++;
            UpdateShape();
        }

        /// <summary>
        /// Removes a given sample at a given sequence index. This will shift all existing samples to preserve data.
        /// </summary>
        /// <param name="sequenceIdx">The index at which to remove the sample</param>
        public void RemoveSample(int sequenceIdx)
        {
            Assert.IsTrue(sequenceIdx >= 0 && sequenceIdx < m_SequenceLength, "Index out of range");

            var sourceOffset = GetOffset(sequenceIdx + 1);
            var destinationOffset = GetOffset(sequenceIdx);
            var copyCount = m_Buffer.Length - sourceOffset;

            unsafe
            {
                fixed (void* dstPtr = &m_Buffer[destinationOffset], srcPtr = &m_Buffer[sourceOffset])
                {
                    UnsafeUtility.MemMove(dstPtr, srcPtr, sizeof(float) * copyCount);
                }
            }

            m_SequenceLength--;
            UpdateShape();
        }

        /// <summary>
        /// Resize to a given length
        /// </summary>
        /// <param name="sequenceLength">The new length</param>
        public void Resize(int sequenceLength)
        {
            Assert.IsTrue(sequenceLength <= m_MaxSequenceLength, "Max sequence length reached");
            m_SequenceLength = sequenceLength;
            UpdateShape();
        }

        /// <summary>
        /// Value setter and getter for 1D tensor
        /// </summary>
        /// <param name="sequence">The sequence index (first dimension)</param>
        public float this[int sequence]
        {
            get
            {
                Assert.IsTrue(m_BatchSize <= 0, "This is not a 1D tensor.");
                var idx = GetIndex(0, sequence);
                return m_Buffer[idx];
            }

            set
            {
                Assert.IsTrue(m_BatchSize <= 0, "This is not a 1D tensor.");
                var idx = GetIndex(0, sequence);
                m_Buffer[idx] = value;
            }
        }

        /// <summary>
        /// Value setter and getter for 2D tensor
        /// </summary>
        /// <param name="batch">The batch index (first dimension)</param>
        /// <param name="sequence">The sequence index (second dimension)</param>
        public float this[int batch, int sequence]
        {
            get
            {
                Assert.IsTrue(m_FirstSize <= 0, "This is not a 2D tensor.");
                var idx = GetIndex(batch, sequence);
                return m_Buffer[idx];
            }

            set
            {
                Assert.IsTrue(m_FirstSize <= 0, "This is not a 2D tensor.");
                var idx = GetIndex(batch, sequence);
                m_Buffer[idx] = value;
            }
        }

        /// <summary>
        /// Value setter and getter for 3D tensor
        /// </summary>
        /// <param name="batch">The batch index (first dimension)</param>
        /// <param name="sequence">The sequence index (second dimension)</param>
        /// <param name="first">The first feature index (third dimension)</param>
        public float this[int batch, int sequence, int first]
        {
            get
            {
                Assert.IsTrue(m_SecondSize <= 0, "This is not a 3D tensor.");
                var idx = GetIndex(batch, sequence, first);
                return m_Buffer[idx];
            }

            set
            {
                Assert.IsTrue(m_SecondSize <= 0, "This is not a 3D tensor.");
                var idx = GetIndex(batch, sequence, first);
                m_Buffer[idx] = value;
            }
        }

        /// <summary>
        /// Value setter and getter for 4D tensor
        /// </summary>
        /// <param name="batch">The batch index (first dimension)</param>
        /// <param name="sequence">The sequence index (second dimension)</param>
        /// <param name="first">The first feature index (third dimension)</param>
        /// <param name="second">The second feature index (fourth dimension)</param>
        public float this[int batch, int sequence, int first, int second]
        {
            get
            {
                Assert.IsTrue(m_FirstSize > 0 && m_SecondSize > 0, "This is not a 4D tensor.");
                var idx = GetIndex(batch, sequence, first, second);
                return m_Buffer[idx];
            }

            set
            {
                Assert.IsTrue(m_FirstSize > 0 && m_SecondSize > 0, "This is not a 4D tensor.");
                var idx = GetIndex(batch, sequence, first, second);
                m_Buffer[idx] = value;
            }
        }

        int GetIndex(int batch, int sequence, int first = 0, int second = 0)
        {
            int index;
            if (m_BatchSize <= 0)
            {
                Assert.AreEqual(0, batch);
                Assert.AreEqual(0, first);
                Assert.AreEqual(0, second);
                index = sequence;
            }
            else if (m_FirstSize <= 0)
            {
                Assert.AreEqual(0, first);
                Assert.AreEqual(0, second);
                index = batch * m_SequenceLength + sequence;
            }
            else if (m_SecondSize <= 0)
            {
                Assert.AreEqual(0, second);
                index = batch * m_SequenceLength * m_FirstSize + sequence * m_FirstSize + first;
            }
            else
            {
                index = batch * m_SequenceLength * m_FirstSize * m_SecondSize + sequence * m_FirstSize * m_SecondSize + first * m_SecondSize + second;
            }

            return index;
        }

        void AllocateBuffer()
        {
            var size = GetOffset(m_MaxSequenceLength);
            m_Buffer = new float[size];
        }

        void UpdateShape()
        {
            if (m_BatchSize == -1)
            {
                m_TensorShape = new TensorShape(m_SequenceLength);
            }
            else if (m_FirstSize == -1)
            {
                m_TensorShape = new TensorShape(m_BatchSize, m_SequenceLength);
            }
            else if (m_SecondSize == -1)
            {
                m_TensorShape = new TensorShape(m_BatchSize, m_SequenceLength, m_FirstSize);
            }
            else
            {
                m_TensorShape = new TensorShape(m_BatchSize, m_SequenceLength, m_FirstSize, m_SecondSize);
            }
        }

        int GetOffset(int sequenceIdx)
        {
            var offset = sequenceIdx;
            if (m_BatchSize < 0)
                return offset;

            offset *= m_BatchSize;
            if (m_FirstSize <= 0)
                return offset;

            offset *= m_FirstSize;
            if (m_SecondSize <= 0)
                return offset;

            offset *= m_SecondSize;
            return offset;
        }
    }
}