using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mygame;

public class FirstController : MonoBehaviour, ISceneController, UserAction {

    public bool IsWin;
    public CoastController fromCoast;
    public CoastController toCoast;
    public BoatController boat;
    //public GameObject boat;
    private CharController[] characters = new CharController[6];
    public bool IsMoving = false;
    UserGUI userGUI;
    public int tipsteps;
    enum ChooseMove {P,PP,PD,DD,D };

    void Awake()
    {
        tipsteps = 0;
        Director director = Director.GetInstance();
        director.CurrentScenceController = this;
        director.CurrentScenceController.LoadResources();
        userGUI = gameObject.AddComponent<UserGUI>() as UserGUI;
    }

    // 加载资源
    public void LoadResources()
    {
        GameObject water = Instantiate(Resources.Load("Perfabs/Water", typeof(GameObject)), new Vector3(0, 0.5F, 0), Quaternion.identity, null) as GameObject;
        water.name = "water";

        fromCoast = new CoastController(false);
        fromCoast.Coast = Instantiate(Resources.Load("Perfabs/Stone", typeof(GameObject)), new Vector3(0, 0, 0), Quaternion.identity, null) as GameObject;
        fromCoast.Coast.transform.position = fromCoast.CoastPosition;
        toCoast = new CoastController(true);
        toCoast.Coast = Instantiate(Resources.Load("Perfabs/Stone", typeof(GameObject)), new Vector3(0, 0, 0), Quaternion.identity, null) as GameObject;
        toCoast.Coast.transform.position = toCoast.CoastPosition;
        boat = new BoatController();
        this.boat.boat = Object.Instantiate(Resources.Load("Perfabs/Boat", typeof(GameObject)), new Vector3(0, 0, 0), Quaternion.identity, null) as GameObject;
        this.boat.boat.name = "boat";
        this.boat.boat.transform.position = boat.fromPosition;
        //this.boat.AddComponent(typeof(BoatController));
        this.boat.boat.AddComponent(typeof(ClickGUI));
        this.boat.boat.AddComponent(typeof(move));
        loadCharacter();
    }

    private void loadCharacter()
    {
        for (int i = 0; i < 3; i++)
        {
            CharController jiaose = new CharController(1,i);
            jiaose.Charater = Instantiate(Resources.Load("Perfabs/Priest", typeof(GameObject)), new Vector3(0, 0, 0), Quaternion.identity, null) as GameObject;
            jiaose.Charater.transform.name = "priest" + i;
            jiaose.coastController = fromCoast;
            jiaose.GoCoast();
            //jiaose.Charater.AddComponent(typeof(ClickGUI));
            jiaose.clickGUI = jiaose.Charater.AddComponent(typeof(ClickGUI)) as ClickGUI;
            jiaose.clickGUI.setController(jiaose);
            characters[i] = jiaose;
        }

        for (int i = 0; i < 3; i++)
        {
            CharController jiaose = new CharController(0,i);
            jiaose.Charater = Instantiate(Resources.Load("Perfabs/Devil", typeof(GameObject)), new Vector3(0, 0, 0), Quaternion.identity, null) as GameObject;
            jiaose.Charater.transform.name = "devil" + i;
            jiaose.coastController = fromCoast;
            jiaose.GoCoast();
            //jiaose.Charater.AddComponent(typeof(ClickGUI));
            jiaose.clickGUI = jiaose.Charater.AddComponent(typeof(ClickGUI)) as ClickGUI;
            jiaose.clickGUI.setController(jiaose);
            characters[i + 3] = jiaose;
        }
    }

    public void MoveBoat()
    {
        // 要判断船载人
        if (this.IsMoving == false  && (this.boat.empty[0] == 0 || this.boat.empty[1] == 0))
        {
            this.IsMoving = true;
        }
    }

    public void ChangeFrom()
    {
        this.boat.ChangeFrom();
    }

    public bool GetFrom()
    {
        return this.boat.IsFrom;
    }

    public bool GetMoving()
    {
        return this.IsMoving;
    }

    public void StopMoving()
    {
        this.IsMoving = false;
        if (this.boat.IsFrom == false)
        {
            if (this.boat.Passenger[0] != null)
            {
                this.boat.Passenger[0].coastController = toCoast;
            }
            if (this.boat.Passenger[1] != null)
            {
                this.boat.Passenger[1].coastController = toCoast;
            }
        }
        else
        {
            if (this.boat.Passenger[0] != null)
            {
                this.boat.Passenger[0].coastController = fromCoast;
            }
            if (this.boat.Passenger[1] != null)
            {
                this.boat.Passenger[1].coastController = fromCoast;
            }
        }
        CheckWin();
    }

    public void restart()
    {
        this.boat.OffBoat(0);
        this.boat.OffBoat(1);
        this.boat.boat.transform.position = this.boat.fromPosition;
        this.boat.IsFrom = true;
        tipsteps = 0;
        for (int i = 0; i < 6; i++)
        {
            characters[i].coastController = fromCoast;
            characters[i].GoCoast();
            characters[i].OnBoat = 2;
        }
        userGUI.step = 0;
        IsMoving = false;
    }

    public void characterIsClicked(CharController character)
    {
        // 若角色可以移动, 点击则上下船
        //Debug.Log(this.IsMoving);
        if (this.IsMoving == false)
        {
            userGUI.step++;
            if (character.OnBoat != 2)
            {
                this.boat.empty[character.OnBoat] = 1;
                //BoatController t = this.boat.boat.transform.GetComponent(typeof(BoatController)) as BoatController;
                this.boat.OffBoat(character.OnBoat);
                character.GoCoast();
                character.OnBoat = 2;
                CheckWin();
                Debug.Log("here");
            }
            else
            {
                //BoatController t = this.boat.transform.GetComponent(typeof(BoatController)) as BoatController;
                character.OnBoat = this.boat.SetPassenger(character);

                Debug.Log("there"+ " " + character.OnBoat);
            }
        }
    }

    public bool CheckWin()
    {
        int DevilsInFrom = 0;
        int PriestsInFrom = 0;
        for (int i = 0; i < 3; i++)
        {
            if (characters[i].coastController == fromCoast)
                PriestsInFrom++;
        }
        for (int i = 3; i < 6; i++)
        {
            if (characters[i].coastController == fromCoast)
                DevilsInFrom++;
        }
        if (PriestsInFrom + DevilsInFrom == 0 && this.boat.empty[0] == 1 && this.boat.empty[1] == 1)
        {
            userGUI.status = 1;
            return true;
        }
        if ((DevilsInFrom > PriestsInFrom && PriestsInFrom != 0)||(DevilsInFrom < PriestsInFrom && PriestsInFrom != 3))
        {
            userGUI.status = 2;
            return false;
        }
        return false;
    }
    public void autoMove() {
        if (tipsteps == 0)
        {
            this.restart();
            characterIsClicked(characters[4]);
            Debug.Log(characters[4].OnBoat);

            tipsteps++;
        }
        else if (tipsteps == 1)
        {
            characterIsClicked(characters[5]);
            Debug.Log(characters[5].OnBoat);
            MoveBoat();
            tipsteps++;
        }
        else if (tipsteps == 2)
        {
            characterIsClicked(characters[5]);
            Debug.Log(characters[5].OnBoat);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 3)
        {
            characterIsClicked(characters[3]);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 4)
        {
            characterIsClicked(characters[4]);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 5)
        {
            characterIsClicked(characters[3]);
            tipsteps++;
        }
        else if(tipsteps == 6)
        {
            characterIsClicked(characters[0]);
            tipsteps++;
        }
        else if (tipsteps == 7)
        {
            characterIsClicked(characters[1]);
            MoveBoat();
            tipsteps++;
        }
        else if (tipsteps == 8)
        {
            characterIsClicked(characters[1]);
       
            tipsteps++;
        }
        else if (tipsteps == 9)
        {
            characterIsClicked(characters[4]);
            MoveBoat();
            tipsteps++;
        }
        else if (tipsteps == 10)
        {
            characterIsClicked(characters[4]);
            tipsteps++;
        }
        else if (tipsteps == 11)
        {
            characterIsClicked(characters[2]);
            MoveBoat();
            tipsteps++;
        }
        else if (tipsteps == 12)
        {
            characterIsClicked(characters[0]);
            tipsteps++;
        }
        else if (tipsteps == 13)
        {
            characterIsClicked(characters[2]);
            tipsteps++;
        }
        else if (tipsteps == 14)
        {
            characterIsClicked(characters[5]);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 15)
        {
            characterIsClicked(characters[3]);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 16)
        {
            characterIsClicked(characters[3]);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 17)
        {
            characterIsClicked(characters[4]);
            MoveBoat();
            tipsteps++;
        }
        else if(tipsteps == 18)
        {
            characterIsClicked(characters[4]);
            tipsteps++;
        }
        else if(tipsteps == 19)
        {
            characterIsClicked(characters[5]);
            tipsteps = 0;
        }

        return;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
