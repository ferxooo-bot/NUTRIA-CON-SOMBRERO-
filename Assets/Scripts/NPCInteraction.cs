using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField]public GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialoguetext;
    [SerializeField] private GameObject dialogueMark;
    private bool didDialogueStart;
    private int lineIndex;
    private float typeTime = 0.05f;
    [SerializeField, TextArea(4,6)]private string[] dialoguelines;
     private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!didDialogueStart){
                Startdialogue();
            }
            else if(dialoguetext.text == dialoguelines[lineIndex]){
                NextDialogueLine();
            }
            else{
                StopAllCoroutines();
                dialoguetext.text = dialoguelines[lineIndex];
               
            }
        }
    }
    void Startdialogue(){
        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        dialogueMark.SetActive(false);
        lineIndex = 0;
        StartCoroutine(Showline());
        Time.timeScale = 0f;

    }
    private IEnumerator Showline(){
        dialoguetext.text = String.Empty;
        foreach(char ch in dialoguelines[lineIndex]){
            dialoguetext.text += ch;
            yield return new WaitForSecondsRealtime(typeTime);
        }
    }
    void NextDialogueLine(){
        lineIndex++;
        if(lineIndex< dialoguelines.Length){
            StartCoroutine(Showline());
        }
        else{
            didDialogueStart = false;
            dialoguePanel.SetActive(false);
            dialogueMark.SetActive(true);
            Time.timeScale= 1f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            dialogueMark.SetActive(true); // Mostrar "E"
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueMark.SetActive(false); // Ocultar "E"
            dialoguePanel.SetActive(false); // Ocultar diálogo si está activo
        }
    }
}

