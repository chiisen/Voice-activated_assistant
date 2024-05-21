// See https://aka.ms/new-console-template for more information
using Voice_activated_assistant;
using Whisper.net;
using Whisper.net.Ggml;

// 指定輸出為 UTF8
Console.OutputEncoding = System.Text.Encoding.UTF8;

string currentDirectory = Environment.CurrentDirectory;
Console.WriteLine($"目前的工作目錄: {currentDirectory}");

var modelName = "ggml-base-q5_1.bin";
if (File.Exists(modelName))
{
    Console.WriteLine($"✅ {modelName} 檔案已經存在，不須下載模型");
}
else
{
    Console.WriteLine($"🈚 {modelName} 檔案不存在，準備下載檔案");

    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
    using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
}

using var whisperFactory = WhisperFactory.FromPath(modelName);
using var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")// auto、zh-TW、zh-CN、zh
    .Build();

var recorder = new AudioRecorder();
while (true)
{
    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
    {
        break;
    }

    recorder.StartRecording();
    Thread.Sleep(10000); // 等待一段時間，例如10秒
    recorder.StopRecording();

    // 使用當前的時間戳來創建一個唯一的檔案名稱
    if (File.Exists(recorder.outputFilePath))
    {
        //Console.WriteLine($"✅ {recorder.outputFilePath} 有錄音檔案");

        using var fileStream = File.OpenRead(recorder.outputFilePath);
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
        }
    }    
}

Console.WriteLine("✅ 程式已結束!");
Console.ReadLine();
