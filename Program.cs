using System.Text;

var files = Directory.GetFiles(".", "*.wt");

files.AsParallel().ForAll(f =>
{
    Console.WriteLine("Processing {0}", f);

    var file = File.OpenRead(f);
    var buffer = new byte[file.Length];
    file.Read(buffer, 0, (int)file.Length);

    Span<Byte> wtData = buffer;

    // Header information
    uint waveSize = BitConverter.ToUInt32(wtData.Slice(4, 4));
    uint waveCount = (uint)BitConverter.ToUInt16(wtData.Slice(8, 2));
    uint flags = (uint)BitConverter.ToUInt16(wtData.Slice(10, 2));

    // Check to see if this is a sample, if so then abort
    if ((flags & 0b_1) !=0 )
        return;

    uint sampleBytes = (uint)((flags & 4) != 0 ? 2 : 4); 
  
    var targetFileName = Path.GetFileNameWithoutExtension(f);
    if(!Directory.Exists(targetFileName))
    {
        Directory.CreateDirectory(targetFileName);
    }

    var targetFolder = Path.GetDirectoryName(f);

    uint waveDataStart = 12;    // offset to the start of the wave data in the file
    uint waveLength = waveSize * sampleBytes;
    uint sampleRate = 44100; // Sample rate for playback - just picking a reasonable number

    for (int waveIdx = 0; waveIdx < waveCount; waveIdx++)
    {
        var dataSrc = wtData.Slice((int)waveDataStart, (int)waveLength);
        waveDataStart += waveLength;

        var targetFile = string.Format(@"{0}\\{1}\\{1}_{2}.wav", targetFolder, targetFileName, waveIdx);
        var target = File.OpenWrite(targetFile);
        BinaryWriter bw = new BinaryWriter(target);   
        
        // Write header information
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write((36 + waveLength));            // File data from this point
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));
        
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);                           // PCM Info Data follows
        bw.Write((ushort)1);                    // Linear quantization aka PCM default, no compression
        bw.Write((ushort)1);                    // Number of channels - always 1 for mono
        bw.Write(sampleRate);                   // Sample rate - just picking a reasonable number
        bw.Write(sampleRate * sampleBytes);     // Byte rate - SampleRate * Bytes per sample * channels (channels = 1 as mono)
        bw.Write((ushort)sampleRate);           // Block alignment - SampleRate * channels (channels = 1 as mono
        bw.Write((ushort)(sampleBytes * 8));    // Bits per sample

        // Write data info + payload
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSrc.Length);
        bw.Write(dataSrc);

        bw.Close();
    }

    var targetInfoFile = string.Format(@"{0}\\{1}\\{1}_info.txt", targetFolder, targetFileName);
    var infoFile = new StreamWriter(targetInfoFile);

    var outputReport = new StringBuilder();

    outputReport.AppendFormat("Info for : {0} {1}", Path.GetFileName(f), Environment.NewLine);
    outputReport.AppendLine("---------------------------------------");
    outputReport.AppendLine();
    outputReport.AppendFormat("Number Of Tables {0} {1}", waveCount, Environment.NewLine);
    outputReport.AppendFormat("Number Of Samples Per Table {0} {1}", waveSize, Environment.NewLine);
    outputReport.AppendFormat("Sample Rate {0} {1}", sampleRate, Environment.NewLine);

    infoFile.Write(outputReport.ToString());

    infoFile.Close();
}

);
