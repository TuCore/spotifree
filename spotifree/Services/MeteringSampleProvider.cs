using NAudio.Wave;
using System;

namespace Spotifree.Services
{
    /// <summary>
    /// Class phụ trợ giúp đo độ lớn âm thanh (Volume Peak) từ luồng audio
    /// </summary>
    public class MeteringSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        public event Action<float>? StreamVolume;

        public MeteringSampleProvider(ISampleProvider source)
        {
            _source = source;
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);

            // Tính toán độ lớn (Peak) trong đoạn buffer này
            float max = 0;
            for (int i = 0; i < samplesRead; i++)
            {
                float abs = Math.Abs(buffer[offset + i]);
                if (abs > max) max = abs;
            }

            // Bắn sự kiện ra ngoài (giá trị từ 0.0 đến 1.0)
            StreamVolume?.Invoke(max);

            return samplesRead;
        }
    }
}