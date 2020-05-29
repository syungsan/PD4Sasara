using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


class GameDirector : MonoBehaviour
{
    [HeaderAttribute("TextLabel")]

    public Text recogText;
    public Text messageText;

    public Text wStatusText;
    public Text wValueText;

    public Text fStatusText;
    public Text fValueText;

    public Text vStatusText;
    public Text vValueText;

    [HeaderAttribute("Character")]

    public EmotePlayer targetPlayer;
    public AudioSource targetAudio;

    public bool showDebugButton = false;
    private int debugCount = 0;

    private Dictionary<string, string> EMOTION_LABELS = new Dictionary<string, string>()
    {
            { "angry", "怒"},
            { "disgust", "嫌" },
            { "fear", "恐" },
            { "happy", "喜" },
            { "neutral", "普" },
            { "sad", "悲" },
            { "surprise", "驚" }
    };

    private Dictionary<string, Color> EMOTION_COLORS = new Dictionary<string, Color>()
    {
            { "angry", Color.red},
            { "disgust", Color.green },
            { "fear", Color.cyan },
            { "happy", Color.yellow },
            { "neutral", Color.black },
            { "sad", Color.blue },
            { "surprise", Color.magenta }
    };

    private Dictionary<string, string[]> emotionSentences;

    private StreamWriter wordStreamWriter;
    private StreamReader wordStreamReader;

    private string sentence = "";

    private string wordEmotion = null;

    private SynchronizationContext _mainContext;

    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        var path = Application.dataPath;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                path += "/StreamingAssets/";
                break;

            case RuntimePlatform.WindowsPlayer:
                path += "/StreamingAssets/";
                break;

            case RuntimePlatform.IPhonePlayer:
                path += "/Raw/";
                break;
        }
        path += "dictation-kit-v4.4/run-win-gmm.jconf";
        // path += "dictation-kit-v4.4/run-win-dnn.jconf";

        Julius.ResultReceived += OnResultReceived;
        Julius.Begin(path);

        this.recogText.text = "";
        this.messageText.text = "";

        this.wStatusText.text = EMOTION_LABELS["neutral"];
        this.wStatusText.color = EMOTION_COLORS["neutral"];
        this.wValueText.text = "0%";

        this.fStatusText.text = EMOTION_LABELS["neutral"];
        this.fStatusText.color = EMOTION_COLORS["neutral"];
        this.fValueText.text = "0%";

        this.vStatusText.text = EMOTION_LABELS["neutral"];
        this.vStatusText.color = EMOTION_COLORS["neutral"];
        this.vValueText.text = "0%";

        if (targetPlayer == null)
        {
            targetPlayer = this.GetComponent(typeof(EmotePlayer)) as EmotePlayer;
        }

        if (targetAudio == null)
            targetAudio = this.GetComponent(typeof(AudioSource)) as AudioSource;

        string[] angry = { "まったくですね。\n腹が立ちますよね。プンプン！", "angry_00", "sample_怒00" };
        string[] disgust = { "うわぁ～！\n嫌ですね～吐き気がします！", "disgust_00", "sample_哀00" };
        string[] fear = { "キャー！！恐ろしい！\n怖すぎます！！", "fear_00", "sample_驚00" };
        string[] happy = { "良かったですね！\nその調子でいきましょう！", "happy_00", "sample_喜00" };
        string[] neutral = { "他に何か言いたいことはありませんか？", "neutral_00", "背伸び" };
        string[] sad = { "悲しいですね…\n私も泣きたくなります…", "sad_00", "sample_哀01" };
        string[] surprise = { "ワァー！マジですか！\n本当にびっくりですね！", "surprise_00", "sample_驚01" };

        this.emotionSentences = new Dictionary<string, string[]>()
        {
            { "angry", angry },
            { "disgust", disgust },
            { "fear", fear },
            { "happy", happy },
            { "neutral", neutral },
            { "sad", sad },
            { "surprise", surprise }
        };

        this.StartWordRecogProcess();

        this._mainContext = SynchronizationContext.Current;
    }

    void OnResultReceived(string result)
    {
        if (result.Contains("on_speech_start"))
        {
            print("表情・声色データ採取スタート…");
        }

        if (result.Contains("sentence,,"))
        {
            // lastResult = "<FinalResult>\n";
            string[] results = result.Split('\n')[0].Split(',');

            var resultList = new List<string>();
            resultList.AddRange(results);

            resultList.RemoveAt(0);
            sentence = string.Join("", resultList);
        }
        else
        {
            // lastResult = "<First Pass Progress>\n";
            // sentence = "";
        }

        // lastResult += result;

        this.recogText.text = sentence;

        if (result.Contains("on_recognition_end"))
        { 
            this.wordStreamWriter.WriteLine(this.sentence);
        }
    }

    void OnGUI()
    {
        // GUI.Label(new Rect(0, 0, Screen.width, Screen.height), sentence);

        if (this.showDebugButton)
        {
            string[] emotions = { "angry", "disgust", "fear", "happy", "neutral", "sad", "surprise" };

            if (GUI.Button(new Rect(Screen.width - 80, Screen.height - 40, 80, 40), "Debug"))
            {
                this.OnChangeCharacterMotion(emotions[this.debugCount]);

                if (this.debugCount >= emotions.Length - 1)
                {
                    this.debugCount = 0;
                }
                else
                {
                    this.debugCount += 1;
                }
            }
        }
    }

    void OnDestroy()
    {
        Julius.Finish();
    }

    void OnChangeCharacterMotion(string emotion)
    {
        string timelineLabel = this.emotionSentences[emotion][2];
        this.targetPlayer.mainTimelineLabel = timelineLabel;

        string message = this.emotionSentences[emotion][0];
        this.messageText.text = message;

        string voice = this.emotionSentences[emotion][1];
        AudioClip clip = Resources.Load<AudioClip>("Sound/Voice/" + voice);

        this.targetAudio.clip = clip;
        this.targetAudio.Play();
    }

    void StartWordRecogProcess()
    {
        // Application.streamingAssetsPath

        // pythonがある場所
        string pyExePath = Application.dataPath + "/../python/python.exe";

        // 実行したいスクリプトがある場所
        string pyCodePath = Application.dataPath + "/StreamingAssets/sentiment.py";

        // 外部プロセスの設定
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            FileName = pyExePath, // 実行するファイル(python)
            UseShellExecute = false, // シェルを使うかどうか
            CreateNoWindow = true, // ウィンドウを開くかどうか
            RedirectStandardOutput = true, //テキスト出力をStandardOutputストリームに書き込むかどうか
            RedirectStandardInput = true,
            Arguments = pyCodePath, // 実行するスクリプト 引数(複数可)
            StandardOutputEncoding = Encoding.UTF8,
            // WorkingDirectory = Application.dataPath + "/../python",
        };

        var initThread = new Thread(new ThreadStart(() =>
        {
            //外部プロセスの開始
            Process process = Process.Start(processStartInfo);

            // this._mainContext.Post(_ => this.DebugMessageInMainThread(process.Id.ToString()), null);

            this.wordStreamWriter = process.StandardInput;

            //ストリームから出力を得る
            this.wordStreamReader = process.StandardOutput;

            //外部プロセスの終了
            process.WaitForExit();
            process.Close();
        }));
        initThread.Start();

        var getThread = new Thread(new ThreadStart(() =>
        {
            while (true)
            {
                if (this.wordStreamReader != null)
                {
                    string result = this.wordStreamReader.ReadLine();
                    string[] results = result.Split(',');

                    // メインスレッドへの委譲
                    this._mainContext.Post(_ => this.SetFromWordRecogProcess(results), null);
                }
            }
        }));
        getThread.Start();
    }

    void SetFromWordRecogProcess(string[] results)
    {
        this.wStatusText.text = this.EMOTION_LABELS[results[1]];
        this.wStatusText.color = this.EMOTION_COLORS[results[1]];

        float probability = float.Parse(results[2]);
        this.wValueText.text = probability.ToString("F2") + "%";

        this.OnChangeCharacterMotion(results[1]);
    }

    /*
    void DebugMessageInMainThread(string message)
    {
        messageText.text = message;
    }
    */
}
