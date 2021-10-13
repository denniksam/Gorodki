using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bat : MonoBehaviour
{
    private const float MIN_FORCE  =   800f;   // Сила броска
    private const float MAX_FORCE  =  1500f;   //  биты
    private const float MIN_TORQUE =   150f;   // Вращательный
    private const float MAX_TORQUE =   1e5f;   //  момент биты

    private Rigidbody Rigidbody;
    private GameObject bat;
    private GameObject batBase;

    private bool isBatMoving;
    private Vector3 BatStartPosition;
    private Vector3 BatPosition2;

    private Vector3 CameraPosition2;

    private Quaternion BatStartRotation;

    private Slider ForceSlider; // ссылка на слайдер изменения силы броска
    private Slider RotationSlider; // ссылка на слайдер изменения силы вращения
    private Vector3 forceDirection;
    private float forceFactor; // начальная сила броска
    private float forceRotat; // начальная сила вращения

    private Collider Gorod;  // Коллайдер объекта "Gorod" - границы города

    void Start()
    {
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

                RemoveBars();
                Debug.Log(BatPosition2);
            }
        }
        #endregion

        #region Бросок биты
        if (Input.GetKeyDown(KeyCode.Space))
        {
            forceFactor = MIN_FORCE + ForceSlider.value * (MAX_FORCE - MIN_FORCE);
            Rigidbody.AddForce(forceFactor  * forceDirection); // бросок 
            Rigidbody.velocity = forceDirection * 0.2f;

            forceRotat = MIN_TORQUE + RotationSlider.value * (MAX_TORQUE - MIN_TORQUE);
            Rigidbody.AddTorque(Vector3.up * forceRotat ); // вращение
            isBatMoving = true;
        }
        #endregion
    }

    private void RemoveBars()
    {
        // Задание: вывести координаты центров всех брусков
        // 1. Добавили ко всем брускам тег "Bar"
        // 2. Находим их по тегу
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        // 3. Обходим циклом и выводим
        foreach(GameObject bar in bars)
        {
            if(Gorod.bounds.Contains(  // входит ли точка в границы коллайдера
                bar.transform.TransformPoint(Vector3.zero)  // пересчет позиции в "мировые" координаты
            ) ) { 
                // брусок не вышел за "город" - останавливаем его
                Rigidbody rb = bar.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // брусок вышел за "город" - деактивируем
                bar.SetActive(false);
            }
           
        }
        // Debug.Log(Gorod.bounds.center);
        // Debug.Log(Gorod.bounds.size); // ! Размер больше, чем в редакторе ??
    }
}
