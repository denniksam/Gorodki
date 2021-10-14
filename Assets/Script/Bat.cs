using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Bat : MonoBehaviour
{
    private const float  MIN_FORCE  =   800f;   // Сила броска
    private const float  MAX_FORCE  =  1500f;   //  биты
    private const float  MIN_TORQUE =  -1e3f;   // Вращательный
    private const float  MAX_TORQUE =   1e3f;   //  момент биты
    private const string GAMES_HISTORY_FILE = "history.xml";

    private Rigidbody Rigidbody;
    private GameObject bat;
    private GameObject batBase;

    private bool isBatMoving;
    private Vector3 BatStartPosition;
    private Vector3 BatPosition2;

    private Vector3 CameraPosition2;

    private Quaternion BatStartRotation;

    private Slider ForceSlider;    // ссылка на слайдер изменения силы броска
    private Slider RotationSlider; // ссылка на слайдер изменения силы вращения
    private Vector3 forceDirection;
    private float forceFactor; // начальная сила броска
    private float forceRotat;  // начальная сила вращения

    private Collider Gorod;    // Коллайдер объекта "Gorod" - границы города

    private int figure;        // номер фигуры (раунд)
    private List<GameObject> Figures;
    private GameObject FigurePlace;  // "Якорь" для установки фигуры

    private List<GameResult> GamesHistory;
    private int throws;  // кол-во бросков (за всю игру)


    void Start()
    {
        LoadHistory();
        throws = 0;
        FigurePlace = GameObject.Find("FigurePlace");

        figure = 1;
        Figures = new List<GameObject>();
        while(true)
        {
            GameObject fig = GameObject.Find("Figure" + figure);
            if (fig == null) break;
            if (figure > 1) fig.SetActive(false);
            Figures.Add(fig);
            figure++;
        }
        figure = 1;
        Figures[0].transform.position = FigurePlace.transform.TransformPoint(Vector3.zero);

        CameraPosition2 = new Vector3(0, 5, -3);

        batBase = GameObject.Find("BatBase");
        BatPosition2 = GameObject.Find("Position2")
                        .transform.position; //  TransformPoint(Vector3.zero);

        Gorod = GameObject.Find("Gorod").GetComponent<Collider>();

        bat = GameObject.Find("Bat");
        isBatMoving = false;
        BatStartPosition = bat.transform.position; // исходное положение биты
        BatStartRotation = bat.transform.rotation; // исходное положение биты

        ForceSlider = GameObject.Find("ForceSlider").GetComponent<Slider>(); // получение слайдера для изменения силы броска
        RotationSlider = GameObject.Find("RotationSlider").GetComponent<Slider>(); // получение слайдера для изменения силы вращения
        Rigidbody = GetComponent<Rigidbody>();
        forceDirection =          // направление броска
            Vector3.left  +       // лево +
            Vector3.forward +     //  вперед - по диагонали
            0.5f * Vector3.up;    // + вверх (0,5 ~ 30 градусов)        
    }

    void Update()
    {
        #region Полет и остановка биты
        if (isBatMoving)
        {
            if (Rigidbody.velocity.magnitude < 0.2f)       // если скорость достигнет 0
            {
                // обнуление остаточной скорости
                Rigidbody.velocity = Vector3.zero;         // поступательная
                Rigidbody.angularVelocity = Vector3.zero;  // вращательная
                isBatMoving = false;
                // bat.transform.position = BatStartPosition;
                batBase.transform.position = new Vector3(
                    BatPosition2.x,
                    batBase.transform.position.y,
                    BatPosition2.z
                );
                bat.transform.position = new Vector3(
                    BatPosition2.x,
                    BatStartPosition.y,
                    BatPosition2.z
                );

                bat.transform.rotation = BatStartRotation;

                GameObject.Find("Main Camera").transform.position = CameraPosition2;

                if(RemoveBars() == 0) ChangeFigure();

                // Debug.Log(BatPosition2);
            }
        }
        #endregion

        #region Бросок биты
        if (Input.GetKeyDown(KeyCode.Space))
        {
            forceFactor = MIN_FORCE + ForceSlider.value * (MAX_FORCE - MIN_FORCE);
            Rigidbody.AddForce(forceFactor  * forceDirection);  // бросок 
            Rigidbody.velocity = forceDirection * 0.2f;

            forceRotat = MIN_TORQUE + RotationSlider.value * (MAX_TORQUE - MIN_TORQUE);
            Rigidbody.AddTorque(Vector3.up * forceRotat );  // вращение
            isBatMoving = true;
            throws++;
        }
        #endregion

        #region Features
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveHistory();
            Debug.Log("Saved");
        }
        #endregion
    }

    private int RemoveBars()
    {
        // Задание: вывести координаты центров всех брусков
        // 1. Добавили ко всем брускам тег "Bar"
        // 2. Находим их по тегу
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        // 3. Обходим циклом и выводим
        int barsInGorod = 0;
        foreach(GameObject bar in bars)
        {
            if(Gorod.bounds.Contains(  // входит ли точка в границы коллайдера
                bar.transform.TransformPoint(Vector3.zero)  // пересчет позиции в "мировые" координаты
            ) ) { 
                // брусок не вышел за "город" - останавливаем его
                Rigidbody rb = bar.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                barsInGorod++;
            }
            else
            {
                // брусок вышел за "город" - деактивируем
                bar.SetActive(false);
            }           
        }
        // Debug.Log(Gorod.bounds.center);
        // Debug.Log(Gorod.bounds.size); // ! Размер больше, чем в редакторе ??
        return barsInGorod;
    }

    private bool ChangeFigure()
    {
        if (figure >= Figures.Count) return false;

        Figures[figure-1].SetActive(false);
        
        Figures[figure].SetActive(true);
        Figures[figure].transform.position = FigurePlace.transform.position;
        
        figure++;

        return true;
    }

    private void LoadHistory()
    {
        if (System.IO.File.Exists(GAMES_HISTORY_FILE))
        {
            using (var reader = new System.IO.StreamReader(GAMES_HISTORY_FILE))
            {
                var serializer = new XmlSerializer(typeof(List<GameResult>));
                GamesHistory = (List<GameResult>)
                    serializer.Deserialize(reader);
                Debug.Log("Deserialized: " + GamesHistory.Count);
            }
        }
    }

    private void SaveHistory()
    {
        if(GamesHistory == null)
        {
            GamesHistory = new List<GameResult>();
        }
        GamesHistory.Add(new GameResult
        {
            Figures = figure,
            Throws = throws,
            Time = 0
        });
        using(var writer = new System.IO.StreamWriter(GAMES_HISTORY_FILE))
        {
            var serializer = new XmlSerializer(GamesHistory.GetType());
            serializer.Serialize(writer, GamesHistory);
        }
        
    }
}

public class GameResult
{
    public int Figures { get; set; }  // сбито фигур
    public int Throws { get; set; }   // совершено бросков
    public float Time { get; set; }   // затрачено времени
}