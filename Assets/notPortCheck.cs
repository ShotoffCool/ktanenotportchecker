using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using KModkit;
using System.ComponentModel;
using System.Security.Cryptography;

public class notPortCheck : MonoBehaviour {

	public class ModSettingsJSON
	{
		public string note;
	}

	public KMAudio Audio;
	public KMBombModule Module;
	public KMBombInfo Info;
	public KMModSettings ModSettings;
	public KMSelectable[] Button;
	public KMSelectable confirm;

	[SerializeField]
	bool[] Ports = new bool[6];

	[SerializeField]
	int stage = 1;

	int stage1Button = -1;
	int stage2Button = -1;
	int stage3Button = -1;

	public GameObject[] Sprites;

    private static int _moduleIdCounter = 1;
	private int _moduleId = 0;

	private bool _isSolved = false;

	public Animator stageAnim;

	int totalBatteries;
	int batteryHolders;
	int aaBatteries;
	int dBatteries;
	List<string> litIndicators;
    List<string> unlitIndicators;

    void Start ()
	{
		_moduleId = _moduleIdCounter++;

		totalBatteries = Info.GetBatteryCount();
		batteryHolders = Info.GetBatteryHolderCount();
		aaBatteries = (totalBatteries - batteryHolders) * 2;
		dBatteries = batteryHolders - (aaBatteries / 2);
		litIndicators = Info.GetOnIndicators().ToList<string>();
        unlitIndicators = Info.GetOffIndicators().ToList<string>();
    }

	void ModuleStart()
	{
		foreach (var item in Sprites)
		{
			item.SetActive(true);
		}
	}

	void Awake()
	{
		Module.OnActivate += delegate () { ModuleStart(); };
		confirm.OnInteract += delegate ()
		{
			HandleConfirm();
			return false;
		};
		Button[0].OnInteract += delegate ()
		{
			HandleButton(0);
			return false;
		};
        Button[1].OnInteract += delegate ()
        {
            HandleButton(1);
            return false;
        };
        Button[2].OnInteract += delegate ()
        {
            HandleButton(2);
            return false;
        }; 
		Button[3].OnInteract += delegate ()
        {
            HandleButton(3);
            return false;
        };
        Button[4].OnInteract += delegate ()
        {
            HandleButton(4);
            return false;
        };
        Button[5].OnInteract += delegate ()
        {
            HandleButton(5);
            return false;
        };
    }

	void HandleConfirm()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, confirm.transform);
		confirm.AddInteractionPunch();

		switch (stage)
		{
			case 1: Stage1(); break;
            case 2: Stage2(); break;
			case 3: Stage3(); break;
			case 4: Stage4(); break;
        }

	}

	void HandleButton(int j)
	{
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, confirm.transform);
        confirm.AddInteractionPunch();

		if (_isSolved)
		{
			return ;
		}

        Ports[j] = !Ports[j];
		for(int i = 0; i < Ports.Length; i++)
		{
			if (i != j)
			{
				Ports[i] = false;
                Button[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
		}
		if (Ports[j] == true)
		{
			Button[j].GetComponent<MeshRenderer>().material.color = new Color32(187, 187, 187, 255);
		}
		else
		{
            Button[j].GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }


	void Stage1()
	{
        int value = 0;

		int[] serNum = Info.GetSerialNumberNumbers().ToArray<int>();
        char[] serLet = Info.GetSerialNumberLetters().ToArray<char>();

		for (int i = 0; i < serNum.Length; i++)
		{
			value += serNum[i];
		}
		//unfinished
		int index = char.ToUpper(serLet[0]) - 64;

		value *= index;

		value %= 6;

		if (Ports[value])
		{
            for (int i = 0; i < Ports.Length; i++)
            {
                Ports[i] = false;
                Button[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }

            int rng = Random.Range(0, 5);
            stage1Button = rng;
            StartCoroutine(TurnGreen(rng));

            stage = 2;
            stageAnim.SetTrigger("Next Stage");
        }
		else
		{
			Module.HandleStrike();
		}
	}

	void Stage2()
	{
		int value = 2;

		value += dBatteries;
		value *= (totalBatteries + batteryHolders);

        value %= 6;

        if (Ports[value])
        {
            for (int i = 0; i < Ports.Length; i++)
            {
                Ports[i] = false;
                Button[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }

            int rng = Random.Range(0, 5);
			stage2Button = rng;
            StartCoroutine(TurnGreen(rng));

            stage = 3;
            stageAnim.SetTrigger("Next Stage");
        }
		else
		{
			Module.HandleStrike();
		}
    }

	void Stage3()
	{
		int value = 50;

		List<char> litChars = new List<char>();
        List<char> unlitChars = new List<char>();

        for (int i = 0; i < litIndicators.Count; i++)
		{
			char[] curChars;
            curChars = litIndicators[i].ToCharArray();
            foreach(char c in curChars)
			{
				litChars.Add(c);
			}
        }

        for (int i = 0; i < unlitIndicators.Count; i++)
        {
            char[] curChars;
            curChars = unlitIndicators[i].ToCharArray();
            foreach (char c in curChars)
            {
                unlitChars.Add(c);
            }
        }

        foreach (char c in litChars)
		{
			value += char.ToUpper(c) - 64;
		}
        foreach (char c in unlitChars)
        {
            value -= char.ToUpper(c) - 64;
        }

        while (value < 0)
        {
            value += 6;
        }

		value %= 6;

		if (Ports[value])
		{
            for (int i = 0; i < Ports.Length; i++)
            {
                Ports[i] = false;
                Button[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }

            int rng = Random.Range(0, 5);
			stage3Button = rng;
			StartCoroutine(TurnGreen(rng));
			StartCoroutine("Whistle");

            stage = 4;
			stageAnim.SetTrigger("Next Stage");
		}
		else
		{
			Module.HandleStrike();
		}
    }

	void Stage4()
	{
		int value = stage2Button;

		value = Stage4Sol();

		if (Ports[value])
		{
            for (int i = 0; i < Ports.Length; i++)
            {
                Ports[i] = false;
                Button[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }

            stageAnim.SetTrigger("Next Stage");
			Module.HandlePass();
			_isSolved = true;
		}
		else
		{
			Module.HandleStrike();
		}
	}

	private int Stage4Sol()
	{
        if (stage1Button == 3 || stage2Button == 3 || stage3Button == 3)
        {
            if (stage2Button == 0 || stage3Button == 0)
            {
                return 1;
            }
            if (stage1Button == 2 || stage2Button == 2 || stage3Button == 2)
            {
                return 3;
            }
        }
        if (stage2Button == 1)
        {
            if (stage1Button != 5 && stage2Button != 5 && stage3Button != 5)
            {
                return 2;
            }
            if (stage3Button == 3 || stage3Button == 4)
            {
                return 4;
            }
        }
        if (stage1Button != 4 && stage2Button != 4 && stage3Button != 4)
        {
            if (stage1Button == 0)
            {
                return 0;
            }
            if (stage1Button == 1 || stage3Button == 1)
            {
                return 3;
            }
        }
        if ((stage1Button == 5 && stage2Button != 5 && stage3Button != 5) || (stage1Button != 5 && stage2Button == 5 && stage3Button != 5) || (stage1Button != 5 && stage2Button != 5 && stage3Button == 5))
        {
            if (stage1Button == 2 || stage2Button == 2 || stage3Button == 2)
            {
                return 2;
            }
            if (stage3Button == 5)
            {
                return 5;
            }
        }
        if (stage3Button != 2)
        {
            if (stage1Button == 4 || stage2Button == 4 || stage3Button == 4)
            {
                return 1;
            }
            if (stage1Button == 2)
            {
                return 0;
            }
        }
        if ((stage1Button == 0 && stage2Button == 0) || (stage1Button == 0 && stage3Button == 0) || (stage2Button == 0 && stage3Button == 0))
        {
            if (stage1Button == 0 && stage2Button == 0 && stage3Button == 0)
            {
                return 5;
            }
            if (stage1Button == 1)
            {

                return 4;
            }
        }
		return stage2Button;
    }

	IEnumerator TurnGreen(int button)
	{
        Button[button].GetComponent<MeshRenderer>().material.color = new Color32(0, 187, 0, 255);

        yield return new WaitForSeconds(1);

        Button[button].GetComponent<MeshRenderer>().material.color = Color.white;
    }

	IEnumerator Whistle()
	{
        yield return new WaitForSeconds(2.3f);

		Audio.PlaySoundAtTransform("Slide Whistle", this.transform);
    }



	KMSelectable[] ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant().Trim();

		if (command.Equals("confirm"))
		{
			return new[] { confirm };
		}



		return null; 
	}
}



