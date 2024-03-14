using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInputBox : SingletonMonobehaviour<PlayerInputBox>
{
    private Player player;
    private TMP_InputField inputField;
    private MessageManager messageManager;

    //protected override void Awake()
    //{
    //    base.Awake();
    //}

    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
        messageManager = MessageManager.Instance;
        inputField = gameObject.GetComponent<TMP_InputField>();
        TMP_Text placeholderText = inputField.placeholder as TMP_Text;
        placeholderText.text = "Enter Text... Enter empty string is you want to cancel...";
    }

    private void OnEnable()
    {
        if (player)
        {
            player.DisablePlayerInputAndResetMovement();
            inputField.ActivateInputField();
        }
    }

    private void OnDisable()
    {
        if (player)
        {
            player.EnablePlayerInput();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            string text = inputField.text;
            if (text.Length > 1)
            {
                messageManager.UpdatePlayerInput(text);
            }
            gameObject.SetActive(false);
        }
    }
}
