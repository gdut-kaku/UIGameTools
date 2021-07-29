using UnityEngine;
using UnityEditor;

public class AssetsImportMyTools : AssetPostprocessor
{
    public void OnPreprocessAudio()
    {
        AudioImporter audioImporter = (AudioImporter)assetImporter;
        //Ambisonic，这个是环境音，一般手机项目都不勾选的。我们项目所有的文件一律不勾选。
        audioImporter.ambisonic = false;
    }

    public void OnPostprocessAudio(AudioClip clip)
    {
        AudioImporter audioImporter = (AudioImporter)assetImporter;
        float audioLength = clip.length;
        //大于2秒的音效文件全部关闭双声道。
        audioImporter.forceToMono = audioLength > 2;
        //大于10秒的文件，一般是背景声音、对话语音等，全部不勾选，其他一律勾选。
        audioImporter.loadInBackground = audioLength < 10;
        //一般对大于10秒的文件都不进行预加载，除非有特殊情况。
        audioImporter.preloadAudioData = audioLength < 10;
        var setting = audioImporter.defaultSampleSettings;
        if (audioLength > 10)
        {
            //Streaming 播放音频的时候流式加载，好处是文件不占用内存，坏处是加载的时候对IO、CPU都会有开销。我们项目一般对大于10秒的文件才会勾选此选项。
            setting.loadType = AudioClipLoadType.Streaming;
        }
        else if (audioLength > 2 && audioLength <= 10)
        {
            //Compress In Memory 表示加载完音频文件之后，以压缩的方式放到内存中，这样做的好处是节省了内存，坏处是播放的时候会消耗CPU进行解压处理。我们项目一般大于2秒，小于10秒的文件选择这个选项。
            setting.loadType = AudioClipLoadType.CompressedInMemory;
        }
        else
        {
            //Decompress On Load 表示加载完音频文件之后，无压缩的释放到内存内，这样做的好处是播放的时候无需解压，速度快，减少CPU的开销，坏处是占用较多的内存。我们项目的设置是，小于2秒的选用此选项。
            setting.loadType = AudioClipLoadType.DecompressOnLoad;
        }
        if (audioLength < 2)
        {
            setting.compressionFormat = AudioCompressionFormat.ADPCM;
            setting.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
        }
        else
        {
            setting.compressionFormat = AudioCompressionFormat.Vorbis;
            setting.quality = 70;
            setting.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            setting.sampleRateOverride = 44100;
        }
        audioImporter.defaultSampleSettings = setting;
        audioImporter.SetOverrideSampleSettings("Standalone", setting);
        audioImporter.SetOverrideSampleSettings("Android", setting);
        audioImporter.SetOverrideSampleSettings("iPhone", setting);
    }
}
