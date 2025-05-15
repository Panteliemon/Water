using System;
using System.Collections.Generic;
using System.IO;

namespace IconMaker;

internal class Program
{
    record LoadedFile(int ImgSize, byte[] Data);

    static void Main(string[] args)
    {
        const string SrcPathBase = @"D:\Bn\Projects\water\favicon-";
        const string SrcExt = ".png";
        const string OutputPath = @"D:\Bn\Src\Water\WaterServer\WaterServer\wwwroot\favicon.ico";

        int[] sizes = [16, 32, 48, 64, 128, 256];

        List<LoadedFile> files = new();
        foreach (int size in sizes)
        {
            string fName = $"{SrcPathBase}{size}{SrcExt}";
            if (File.Exists(fName))
            {
                byte[] data = File.ReadAllBytes(fName);
                files.Add(new LoadedFile(size, data));
            }
        }

        if ((files.Count > 0) && (!File.Exists(OutputPath)))
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((short)0); // fixed
            bw.Write((short)1); // "ico"
            bw.Write((short)files.Count);
            // 6 for header

            int prevFileLengthSum = 0;
            for (int i = 0; i < files.Count; i++)
            {
                bw.Write((byte)files[i].ImgSize); // W
                bw.Write((byte)files[i].ImgSize); // H
                bw.Write((byte)0); // "more than 256 colors"
                bw.Write((byte)0); // fixed
                bw.Write((short)0); // "color planes". What the fuck are color planes? ✈️
                bw.Write((short)24); // bits per pixel
                bw.Write(files[i].Data.Length);
                bw.Write(6 + 16 * files.Count + prevFileLengthSum);
                // 16 for directory entry

                prevFileLengthSum += files[i].Data.Length;
            }

            for (int i = 0; i < files.Count; i++)
            {
                bw.Write(files[i].Data);
            }

            File.WriteAllBytes(OutputPath, ms.ToArray());
        }
    }
}
