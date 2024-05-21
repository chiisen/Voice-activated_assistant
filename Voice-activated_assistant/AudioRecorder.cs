using NAudio.Wave;

namespace Voice_activated_assistant
{
    /// <summary>
    /// 使用 NAudio 錄音
    /// </summary>
    public class AudioRecorder
    {
        private WaveInEvent? waveSource = null;
        private WaveFileWriter? waveFile = null;
        private bool isRecording = false;
        private readonly double threshold = 0.001; // 設定聲音偵測的閾值
        public string outputFilePath = "";

        /// <summary>
        /// 開始錄音
        /// </summary>
        /// <param name="outputFilePath"></param>
        public void StartRecording()
        {
            this.outputFilePath = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".wav";

            waveSource = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 1) // 設置錄音的格式，這裡為 CD 質量
            };

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(WaveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(WaveSource_RecordingStopped);

            //Console.WriteLine($"✅ {this.outputFilePath} 開始錄音，請說:");

            waveSource.StartRecording();
        }

        /// <summary>
        /// 可用資料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            // 計算音頻數據的振幅
            float amplitude = 0;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = BitConverter.ToInt16(e.Buffer, index);
                amplitude += Math.Abs(sample / 32768f);
            }
            amplitude /= (e.BytesRecorded / 2);

            // 如果振幅超過閾值，則開始錄音
            if (amplitude > threshold && !isRecording)
            {
                waveFile = new WaveFileWriter(this.outputFilePath, waveSource?.WaveFormat);
                isRecording = true;
            }

            // 如果正在錄音，則將音頻數據寫入檔案
            if (isRecording)
            {
                waveFile?.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile?.Flush();
            }
        }

        /// <summary>
        /// 停止錄音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            waveSource?.Dispose();
            waveSource = null;

            waveFile?.Dispose();
            waveFile = null;

            isRecording = false;
        }

        /// <summary>
        /// 停止錄音
        /// </summary>
        public void StopRecording()
        {
            waveSource?.StopRecording();
            while (isRecording)
            {
                Thread.Sleep(100); // 等待一段時間，例如0.1秒
            }
            //Console.WriteLine($"🈚 {this.outputFilePath} 錄音結束!");
        }

        /// <summary>
        /// 判斷是否在錄音中
        /// </summary>
        /// <returns></returns>
        public bool IsRecording()
        {
            return isRecording;
        }

    }
}
