
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirectionInput
{
    Null,
    Up,
    Left,
    Right,
    Down
}

public class PlayerController : MonoBehaviour
{
    [Header ("Configuracion player")]
    [SerializeField] private float speedMove;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float gravity = 20f;

    [Header ("Configuracion carril")]
    [SerializeField] private float leftLanePos = -2;
    [SerializeField] private float rightLanePos = 2;

    public bool isJumping { get; private set;}
    public bool isSliding { get; private set;}
    private DirectionInput directionInput;
    private Coroutine coroutineSlide;
    private CharacterController characterController;
    private float verticalPos;
    private int actualLane;
    private Vector3 desiredLane;

    //Para el deslizamiento voy a modificar el Collider del Character Controller
    //para eso necesito crear las variables de radio, posicion y centro y.
    private float controllerRadius;
    private float controllerHeight;
    private float controllerPosY;



    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //Para el deslizamiento, defino los valores del Collider cuando NO me estoy deslizando 
        //esto me servirá para reestablecer el Collider 
        controllerRadius = characterController.radius;
        controllerHeight = characterController.height;
        controllerPosY = characterController.center.y;
    }

    // Update is called once per frame
    void Update()
    {
        DetectInput();
        LaneControl();
        CalculateVerticalMove();
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 newPos = new Vector3(desiredLane.x, verticalPos, speedMove);
        characterController.Move(newPos * Time.deltaTime);
    }

    private void CalculateVerticalMove()
    {
        if(characterController.isGrounded)
        {
            isJumping = false;
            verticalPos = 0f;

            //Si detecto que cambia la dirección del input a Up cambio el valor por el de jumpForce
            if (directionInput == DirectionInput.Up)
            {
                verticalPos = jumpForce;
                isJumping = true;
                //reestablezco el collider para el salto 
                if (coroutineSlide != null)
                {
                    StopCoroutine(coroutineSlide);
                    isSliding = false;
                    ModifyCollider(false);
                }
            }

            //Si cambia la dirección del input a Down me delizo
            if (directionInput == DirectionInput.Down)
            {
                //verifico que ya no me este deslizando
                if(isSliding)
                {
                    return;
                } 

                Slide();
            }
        }
        else
        {
            //si estoy en el aire e intento deslizarme "caigo" y realizo el deslizamiento
            if (directionInput == DirectionInput.Down)
            {
                verticalPos -= jumpForce;
                Slide();
            }
        }

        verticalPos -= gravity * Time.deltaTime;
    }

    //utillizo un switch para controlar en que carril me encuentro
    private void LaneControl()
    {
        switch (actualLane)
        {
            case -1:
                //Mover izq
                LeftLaneLogic();
                break;
            case 0:
                MidLaneLogic();
                break;
            case 1:
                //Mover der
                RightLaneLogic();
                break;
        }
    }

    private void MidLaneLogic()
    {
        //primero tengo que saber desde donde vengo
        //me fijo si estoy en el carril derecho para moverme hacia el centro
        if(transform.position.x > 0.1f)
        {
            HorizontalMove(0f, Vector3.left);
        }
        //lo mismo hacia el otro lado
        else if (transform.position.x < -0.1f)
        {
            HorizontalMove(0f, Vector3.right);
        }
        else
        {
            desiredLane = Vector3.zero;
        }
    }
    private void LeftLaneLogic()
    {
        HorizontalMove(leftLanePos, Vector3.left);
    }

    private void RightLaneLogic()
    {
        HorizontalMove(rightLanePos, Vector3.right);
    }

    private void HorizontalMove(float posX, Vector3 directionMove)
    {
        //utilizo valor absoluto porque trabajo con numeros negativos
        float horizontalPos = Mathf.Abs(transform.position.x - posX);
        if (horizontalPos > 0.1f)
        {
            desiredLane = Vector3.Lerp(desiredLane, directionMove * 20f, Time.deltaTime * 500f);
        }
        else
        {
            //detengo el movimiento una vez que llegue a donde quería
            desiredLane = Vector3.zero;
            //reseteo la posición
            transform.position = new Vector3(posX, transform.position.y, transform.position.z);
        }
    }

    //Creo un método para el deslizamiento
    private void Slide()
    {
        coroutineSlide = StartCoroutine(COSlidePlayer());
    }

    //Creo una corrutina para el deslizamiento
    private IEnumerator COSlidePlayer()
    {
        isSliding = true;
        ModifyCollider(true);
        yield return new WaitForSeconds(2f);
        ModifyCollider(false);
    }

    //Creo un método para modificar el Collider en el deslizamiento
    private void ModifyCollider(bool modify)
    {
        if(modify)
        {
            //modifico el collider
            characterController.radius = 0.5f;
            characterController.height = 1f;
            characterController.center = new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            //reestablezco los valores
            characterController.radius = controllerRadius;
            characterController.height = controllerHeight;
            characterController.center = new Vector3(0f, controllerPosY, 0f);
        }
    }

    //Creo un método para verificar si se está presionando alguna tecla
    private void DetectInput()
    {
        //En cada frame reseteo el valor
        directionInput = DirectionInput.Null;
        //Si presiono A o flecha derecha le resto un valor al carril actual e indico que la dirección es Left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            directionInput = DirectionInput.Left;
            actualLane--;
        }
        //Si presiono D o flecha derecha le sumo un valor al carril actual e indico que la dirección es Right
        if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            directionInput = DirectionInput.Right;
            actualLane++;
        }
        //Si presiono W o flecha arriba indico que la dirección es Up
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            directionInput = DirectionInput.Up;
        }
        //Si presiono S o flecha abajo indico que la dirección es Down
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            directionInput = DirectionInput.Down;
        }

        //utilizo Mathf para evitar que el valor de carril actual sea superior a 1 o menor a -1
        actualLane = Mathf.Clamp(actualLane,-1,1);
    }
}
