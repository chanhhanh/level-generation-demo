using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    #region Singleton
    public static Player instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask stopMovement;
    [SerializeField]
    private ClassData classData;
    public int playerLevel;
    public float experience;
    public float experienceLevel;
    private readonly string CURR_LEVEL = "Level";
    private readonly string CURR_EXPERIENCE = "Experience";
    private readonly string CURR_HP = "Hp";
    private readonly string CURR_MANA = "Mana";

    public override void Start()
    {
        movePoint.localPosition = movePoint.position;
        LoadPlayerLevel();
        CalculateStats();
        base.Start();
        LoadPlayerResourses();
        SavePlayerStats();
    }
    private void Update()
    {
        PlayerMove();
    }
    void CalculateStats()
    {
        strength = Mathf.Round((classData.strength + bonusStr + playerLevel) * classData.strengthModifier);
        dexterity = Mathf.Round((classData.dexterity + bonusDex + playerLevel) * classData.dexterityModifier);
        intelligence = Mathf.Round((classData.intelligence + bonusInt + playerLevel) * classData.intelligenceModifier);
        constitution = Mathf.Round((classData.constitution + bonusCon + playerLevel) * classData.constitutionModifier);
        perception = Mathf.Round((classData.perception + bonusPer + playerLevel) * classData.perceptionModifier);
        speed = Mathf.Round((classData.dexterity + bonusSpd + playerLevel) * classData.speedModifier);
        experienceLevel = playerLevel * 20.5f;
        maxHealth = constitution * 1.5f;
        maxMana = intelligence * 1.5f;
    }
    void LoadPlayerLevel()
    {
        if (PlayerPrefs.HasKey(CURR_LEVEL))
        {
            playerLevel = PlayerPrefs.GetInt(CURR_LEVEL);
            experience = PlayerPrefs.GetFloat(CURR_EXPERIENCE);
        }
    }
    void LoadPlayerResourses()
    {
        if (PlayerPrefs.HasKey(CURR_LEVEL))
        {
            health = PlayerPrefs.GetFloat(CURR_HP);
            mana = PlayerPrefs.GetFloat(CURR_MANA);
        }
    }
    void SavePlayerStats()
    {
        PlayerPrefs.SetInt(CURR_LEVEL, playerLevel);
        PlayerPrefs.SetFloat(CURR_EXPERIENCE, experience);
        PlayerPrefs.SetFloat(CURR_HP, health);
        PlayerPrefs.SetFloat(CURR_MANA, mana);
    }
    void PlayerMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.localPosition, moveSpeed * Time.deltaTime);
        if ((transform.position - movePoint.localPosition).sqrMagnitude <= 0f)
        {
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                if (!Physics2D.OverlapCircle(movePoint.localPosition + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f), .2f, stopMovement))
                {
                    movePoint.localPosition += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                }
            }
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                if (!Physics2D.OverlapCircle(movePoint.localPosition + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f), .2f, stopMovement))
                {
                    movePoint.localPosition += new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                }
            }
        }
    }
}
