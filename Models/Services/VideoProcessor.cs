using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

public class VideoProcessor
{
    public static List<string> SliceVideo(string inputPath, string outputDir, int segmentSeconds = 10)
    {
        var outputFiles = new List<string>();
        using var capture = new VideoCapture(inputPath);

        double fps = capture.Get(CapProp.Fps);
        double frameCount = capture.Get(CapProp.FrameCount);
        double duration = frameCount / fps;

        int segmentFrames = (int)(fps * segmentSeconds);
        int currentSegment = 0;

        Mat frame = new Mat();

        int frameIndex = 0;
        VideoWriter? writer = null;

        while (true)
        {
            if (!capture.Read(frame) || frame.IsEmpty)
                break;

            if (frameIndex % segmentFrames == 0)
            {
                writer?.Dispose();

                string fileName = Path.Combine(outputDir, $"segment_{currentSegment}.mp4");
                outputFiles.Add(fileName);

                writer = new VideoWriter(
                    fileName,
                    VideoWriter.Fourcc('H', '2', '6', '4'),
                    fps,
                    frame.Size,
                    true
                );

                currentSegment++;
            }

            writer?.Write(frame);
            frameIndex++;
        }

        writer?.Dispose();
        return outputFiles;
    }

    public static List<string> SliceWithFfmpeg(string inputPath, string outputDir, int segmentSeconds)
    {
        Directory.CreateDirectory(outputDir);
        var outputTemplate = Path.Combine(outputDir, "segment_%03d.mp4");

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{inputPath}\" -c copy -map 0 -segment_time {segmentSeconds} -f segment \"{outputTemplate}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = System.Diagnostics.Process.Start(psi);
        proc.WaitForExit();

        // Descobre os arquivos gerados
        var files = Directory.GetFiles(outputDir, "segment_*.mp4")
                            .OrderBy(f => f)
                            .ToList();

        return files;
    }

    public static List<string> ExtractFrames(string inputPath, string outputDir, int frameRate = 1)
    {
        Directory.CreateDirectory(outputDir);

        // Modelo do nome das imagens extraídas
        var outputTemplate = Path.Combine(outputDir, "frame_%04d.jpg");

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{inputPath}\" -vf fps={frameRate} \"{outputTemplate}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = System.Diagnostics.Process.Start(psi);
        proc.WaitForExit();

        var files = Directory.GetFiles(outputDir, "frame_*.jpg")
                             .OrderBy(f => f)
                             .ToList();

        return files;
    }
}
