using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Salva tutti i valori impostati nel menu e fa partire la scena dell'esercizio. Le variabili statiche vengono viste anche da quella scena
public class SaveStateAndLoadScene : MonoBehaviour {

    //dropdown che mi permette di capire che esercizio vuole fare l'utente
    public Dropdown Excercise;

    //VARIABILI DEL REACHING
    public Dropdown SphereDimension;
    public Slider NumberOfRepetitions;
    public Slider PathLength;
    public Slider TimeAllowed;
    public Dropdown Sphere1, Sphere2;
    public Dropdown SphereOrientation;
    public Dropdown ForceFeedback;
    public Toggle Audio;

    //VARIABILI DEL CONTOURING
    public Dropdown PathDimension;
    public Slider NumberOfRepetitions_CONTOURING;
    public Slider PathWidth;
    public Slider TimeAllowed_CONTOURING;
    public Dropdown PathColor;
    public Dropdown PathOrientation;
    public Dropdown ForceFeedback_CONTOURING;
    public Toggle Audio_CONTOURING;

    //VARIABILI DEL CHASING
    public Dropdown ChaseCircleWidth;
    public Slider ChasePeriod;
    public Slider TimeAllowed_CHASING;
    public Dropdown ChaseSphereColor;
    public Dropdown ForceFeedback_CHASING;
    public Toggle Audio_CHASING;

    //messaggio d'errore se non è collegato l'hololens e cerco di far partire l'esercizio
    public Text errorMessage;

    //variabile statica che tiene conto dell'esercizio
    public static int _excercise;

    //variabili STATICHE per il reaching;
    public static int  _sphDim, _numRep, _timAll, _sph1, _sph2, _sphOri, _forFeed, _audio;         
    public static float _pathLen;

    //variabili STATICHE per il contouring;
    public static int  _pathDim, _numRep_Cont, _timAll_Cont, _pathCol, _pathOri, _forFeed_Cont, _audio_Cont;
    public static float _pathWid;

    //variabili STATICHE per il chasing
    public static int _chaseWid, _timAll_Chase, _chaseCol, _forFeed_Chase, _audio_Chase;
    public static float _chasePeriod;

    private int excerciseScene = 1;         //è il numero che serve per far partire la scena dell'esercizio, questo numero è impostato da "build settings" ed indica la scena  

    //si salvano le impostazioni degli esercizi nella variabili statiche
    private void SaveState()
    {
        switch (Excercise.value)
        {
            case 1:

                _excercise = 1;
                _sphDim = SphereDimension.value;
                _numRep = (int)NumberOfRepetitions.value;
                _pathLen = 3 + PathLength.value * 0.2f;
                _timAll = (int)TimeAllowed.value;
                _sph1 = Sphere1.value;
                _sph2 = Sphere2.value;
                _sphOri = SphereOrientation.value;
                _forFeed = ForceFeedback.value;
                if (Audio.isOn)
                {
                    _audio = 1;
                }
                else
                {
                    _audio = 0;
                }
                break;

            case 2:

                _excercise = 2;
                _pathDim = PathDimension.value;
                _numRep_Cont = (int)NumberOfRepetitions_CONTOURING.value;
                _pathWid = 1.2f + PathWidth.value * 0.2f;
                _timAll_Cont = (int)TimeAllowed_CONTOURING.value;
                _pathCol = PathColor.value;
                _forFeed_Cont = ForceFeedback_CONTOURING.value;
                _pathOri = PathOrientation.value;
                if (Audio_CONTOURING.isOn)
                {
                    _audio_Cont = 1;
                }
                else
                {
                    _audio_Cont = 0;
                }
                break;

            case 3:

                _excercise = 3;
                _chaseWid = ChaseCircleWidth.value;
                _chasePeriod = 4 + ChasePeriod.value * 0.2f;
                _timAll_Chase = (int)TimeAllowed_CHASING.value;
                _chaseCol = ChaseSphereColor.value;
                _forFeed_Chase = ForceFeedback_CHASING.value;
                if (Audio_CHASING.isOn)
                {
                    _audio_Chase = 1;
                }
                else
                {
                    _audio_Chase = 0;
                }
                break;
        }
    }

    //carica la scena controllando se l'hololens è connesso. Se non è connesso
    //si mostra un messaggio di errore
    public void LoadScene()
    {
        if (CheckHololens.flag_Hololens ==1)
        {
            SaveState();
            SceneManager.LoadScene(excerciseScene);
        }
        else
        {
            StartCoroutine(Error());
        }
    }

    //mostra un messaggio d'errore se l'hololens non è connesso
    IEnumerator Error()
    {
        int count = 0;
        while(count < 3)
        {
            count++;
            errorMessage.enabled = true;
            yield return new WaitForSeconds(1);
        }
        errorMessage.enabled = false;
        yield break;
    }
}
