using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class caesarsMathsScript : MonoBehaviour {
    //Manual page: link

    public KMBombInfo Bomb;
	public KMAudio Audio;
    
    //Logging setup
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    // Declaration for single button handling:
    //public KMSelectable button1;
    // Declaration for identical button handling:
    public KMSelectable[] myButtons;

    // Arrays of the text objects
    public TextMesh[] buttonsText;
    public TextMesh screenText;
    
    // One way to hide an object (with disableWords.SetActive(false);) when
    // this object is assigned to something in Unity
    //public GameObject disableWords;
    
    // LED colours array and colour handling
    public Material[] ledOptions;
    private string[] ledOptionNames = { "Yellow", "Blue", "Red", "Green" };
    public Renderer[] myLEDs;
    // Store current colour of each LED
    private int[] ledIndices = { 0, 0, 0 };

    // Some potentially useful variables
    private int shiftKey;
    private String puzzle;
    private String encryptedPuzzle;

    private bool validPuzzle = false;
    private int correctAnswer;


    private String numberOneString;
    private String numberTwoString;

    public Material black;
    private bool flashing;


    private bool pickedLEDs;


    void Awake() {
		moduleId = moduleIdCounter++;
		
        // Method using a loop to iterate over the buttons:
		foreach (KMSelectable obj in myButtons) {
			KMSelectable pressedObject = obj;	
			obj.OnInteract += delegate () { buttonPress(pressedObject); return false; };
		}
        
    }

	void Start () {

        // Once LED colours are picked, they
        // stay the same for the whole module
        if (!pickedLEDs) {
            pickedLEDs = true;
            PickLedColours();
            DetermineShiftKey();
        }

        validPuzzle = false;

        while (!validPuzzle)
        {
            CreatePuzzle();
            EncipherPuzzle();
            if (encryptedPuzzle.Length < 30)
            {
                validPuzzle = true;
            }
        }

        // Log the generated valid puzzle
        Debug.LogFormat("[caesarsMaths #{0}] Generated puzzle: {1}", moduleId, puzzle);
        Debug.LogFormat("[caesarsMaths #{0}] Correct answer: {1}.", moduleId, correctAnswer);
        // Update the screen text with the encrypted puzzle
        screenText.text = encryptedPuzzle;

        AddButtonOptions();

	}

    void PickLedColours()
    {
        for (int i = 0; i < ledIndices.Length; i++)
        {
            ledIndices[i] = UnityEngine.Random.Range(0, 4);
            myLEDs[i].material = ledOptions[ledIndices[i]];
        }

        Debug.LogFormat("[Caesar's Maths #{0}] The LEDs are {1}, {2} and {3}.", moduleId, ledOptionNames[ledIndices[0]], ledOptionNames[ledIndices[1]], ledOptionNames[ledIndices[2]]);

    }

    void DetermineShiftKey()
    {
        // a = Sum of digits in the serial number
        int a = Bomb.GetSerialNumberNumbers().Sum();
        // b = Number of batteries on the bomb + 3
        int b = Bomb.GetBatteryCount() + 3;
        // c = Number of green LEDs on the module
        int c = 0;
        // d = number of red LEDs on the module
        int d = 0;
        for (int i = 0; i < 3; i++)
        {
            // If the LED is green, increment c
            if (ledIndices[i] == 3) { c++; }
            // If the LED is red, increment d
            if (ledIndices[i] == 2) { d++; }
        }

        // Using the formula from the manual:
        shiftKey = ((6 * a * ((int) Math.Pow(b, c))) - d) % 26;

        Debug.LogFormat("[Caesar's Maths #{0}] a={1}, b={2}, c={3}, d={4}.", moduleId, a, b, c, d);
        Debug.LogFormat("[Caesar's Maths #{0}] The shift key is {1}.", moduleId, shiftKey);
    }

    void CreatePuzzle()
    {
        // A puzzle consists of: <descriptor> <operatorA> [number] <operatorB> [number]
        // For example:            COMPUTE  THE PRODUCT OF   16       AND         8     
        // Note: Operators A and B are defined in pairs.
        
        String[] operatorA = { "ADD",    "MULTIPLY",   "DIVIDE",    "PERFORM",  "SUBTRACT",   "CALCULATE",  "FIND" };
        String[] operatorB = { "TO",     "BY",         "BY",        "TIMES",    "FROM",       "MINUS",      "PLUS"};

        int chosenPuzzle = UnityEngine.Random.Range(0, operatorA.Length);
;
        // Generate some random integers to use in the puzzle
        int[] generatedNumbers = generateNumbers(chosenPuzzle);
        
        correctAnswer = GetAnswer(chosenPuzzle, generatedNumbers[0], generatedNumbers[1]);
        
        // Ensure the answer is not 0
        while (correctAnswer == 0)
        {
            generatedNumbers = generateNumbers(chosenPuzzle);
            correctAnswer = GetAnswer(chosenPuzzle, generatedNumbers[0], generatedNumbers[1]);
        }

        numberOneString = convertNumberToLetter(generatedNumbers[0]);
        numberTwoString = convertNumberToLetter(generatedNumbers[1]);

        // Save the generated unencrypted puzzle
        puzzle = operatorA[chosenPuzzle] + " " + numberOneString + " " + operatorB[chosenPuzzle] + " " + numberTwoString;

    }

    String convertNumberToLetter(int number)
    {
        if (number < 0) { return ""; }

        String[] numberMap = { "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"};
        String[] tensMap = { "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

        if (number < 20)
        {
            return numberMap[number];
        }
        else if (number < 100 && (number % 10 == 0))
        {
            return tensMap[(number - 20) / 10];
        }
        else if (number < 100)
        {
            return tensMap[(number - 20) / 10] + " " + numberMap[number % 10];
        }
        else
        {
            return "error";
        }
        
    }

    int[] generateNumbers(int puzzleIndex)
    {
        int[] newNumbers = new int[2];

        switch (puzzleIndex)
        {
            case 0:
            case 6:
                // A + B
                newNumbers[0] = UnityEngine.Random.Range(4, 40);
                newNumbers[1] = UnityEngine.Random.Range(5, 40);
                break;
            case 1:
            case 3:
                // A * B
                newNumbers[0] = UnityEngine.Random.Range(3, 15);
                newNumbers[1] = UnityEngine.Random.Range(10, 25);
                break;
            case 2:
                // A / B
                newNumbers[0] = UnityEngine.Random.Range(50, 100);
                newNumbers[1] = UnityEngine.Random.Range(6, 15);
                break;
            case 4:
            case 5:
                // A - B
                newNumbers[0] = UnityEngine.Random.Range(50, 80);
                newNumbers[1] = UnityEngine.Random.Range(6, 50);
                break;
        }
        
        return newNumbers;
    }

    int GetAnswer(int puzzleIndex, int num1, int num2)
    {
        int answer = 0;
        switch (puzzleIndex)
        {
            case 0:
            case 6:
                // A + B
                answer = num1 + num2;
                break;
            case 1:
            case 3:
                // A * B
                answer = num1 * num2;
                break;
            case 2:
                // A / B
                answer = num1 / num2;
                break;
            case 4:
            case 5:
                // A - B
                answer = num1 - num2;
                break;
        }

        return answer;
    }

    void EncipherPuzzle()
    {
        encryptedPuzzle = "";
        foreach (char c in puzzle) {
            char convertedChar = c;
            if (Convert.ToInt16(c) > 64 && Convert.ToInt16(c) < 92)
            {
                convertedChar = Convert.ToChar(((Convert.ToInt16(c) - 65 + shiftKey) % 26) + 65);
                encryptedPuzzle = encryptedPuzzle + convertedChar;

                if ((encryptedPuzzle.Length - 2 * (encryptedPuzzle.Length / 8)) % 8 == 0)
                {
                    encryptedPuzzle += "\n";
                }
            }
        }
    }

    void AddButtonOptions()
    {
        int[] buttonText = new int[3];

        for (int i = 0; i < 3; i++)
        {
            buttonText[i] = UnityEngine.Random.Range(10, 90);

            while (buttonText[i] == correctAnswer) {
                buttonText[i] = UnityEngine.Random.Range(10, 90);
            }
        }

        buttonText[UnityEngine.Random.Range(0, 3)] = correctAnswer;

        for (int b = 0; b < 3; b++) {
            String buttonNumber = buttonText[b].ToString();
            myButtons[b].GetComponentInChildren<TextMesh>().text = buttonNumber;
        }
    }

    // Method for acting upon any button pressed
    void buttonPress(KMSelectable button)
    {
        if (moduleSolved || flashing)
        {
            //If the module is solved, or resetting, ignore any further button presses
            return;
        }

        Debug.LogFormat("[Caesar's Maths #{0}] Button {1} pressed.", moduleId, button.GetComponentInChildren<TextMesh>().text);

        // Interaction punch, and sound
        button.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);


        // When module is solved:
        if (button.GetComponentInChildren<TextMesh>().text == correctAnswer.ToString()) {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Caesar's Maths #{0}] Module solved.", moduleId);
            flashing = true;
            StartCoroutine(Solve());

        } else {
            //If module strikes:
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Caesar's Maths #{0}] Strike! That is not the correct answer.", moduleId);
            flashing = true;
            StartCoroutine(Strike());
        }



    }

    IEnumerator Strike()
    {
        yield return new WaitForSeconds(0.1f);
        

        screenText.text = "";

        for (int flash = 0; flash < 4; flash++) {

            foreach (Renderer led in myLEDs) {
                led.GetComponentInChildren<Light>().intensity = 0;
                led.material = black;
            }


            yield return new WaitForSeconds(0.1f);

            foreach (Renderer led in myLEDs) {
                led.GetComponentInChildren<Light>().intensity = 4;
                led.material = ledOptions[2];
            }
            yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < 3; i++)
        {
            myLEDs[i].material = ledOptions[ledIndices[i]];
        }
        
        flashing = false;
        
        Start();
    }


    IEnumerator Solve()
    {
        yield return new WaitForSeconds(0.1f);
        

        foreach (Renderer led in myLEDs)
        {
            led.GetComponentInChildren<Light>().intensity = 0;
            led.material = black;
        }
        
        screenText.text = "";

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < 3; i++)
        {
            myLEDs[i].GetComponentInChildren<Light>().intensity = 4;
            myLEDs[i].material = ledOptions[3];
            yield return new WaitForSeconds(0.2f);
        }
        
        yield return new WaitForSeconds(0.4f);

        foreach (Renderer led in myLEDs)
        {
            led.GetComponentInChildren<Light>().intensity = 0;
            led.material = black;
        }
        flashing = false;
    }
    
}
