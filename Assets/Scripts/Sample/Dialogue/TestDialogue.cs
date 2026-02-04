using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestDialogue : MonoBehaviour
{
    [Header("UI Component")]
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("CSV File Name")]
    public string csvFileName = "TestDialogue";

    // 파싱된 데이터를 담을 리스트
    private List<Dictionary<string, object>> dialogueData;
    private int currentLineIndex = 0; // 현재 읽고 있는 줄 번호

    void Start()
    {
        dialogueData = CSVParser.Read(csvFileName);
        nextButton.onClick.AddListener(ShowNextLine);
        ShowNextLine();
    }

    void ShowNextLine()
    {
        // 더 이상 읽을 데이터가 없으면 종료
        if (currentLineIndex >= dialogueData.Count)
        {
            Debug.Log("대화 끝!");
            return;
        }

        // 현재 순서의 데이터 가져오기
        Dictionary<string, object> row = dialogueData[currentLineIndex];

        nameText.text = row[TestDialogueConfig.COL_SPEAKER].ToString();

        dialogueText.text = row[TestDialogueConfig.COL_DIALOGUE].ToString();
        StopAllCoroutines();
        StartCoroutine(TypingEffect(dialogueText, 0.05f));

        string portraitName = row[TestDialogueConfig.COL_PORTRAIT].ToString();

        // Resources.Load로 이미지 불러오기
        Sprite loadedSprite = Resources.Load<Sprite>("Portraits/" + portraitName);

        if (loadedSprite != null)
        {
            portraitImage.sprite = loadedSprite;
            portraitImage.enabled = true; // 이미지가 있으면 보이기
        }
        else
        {
            // 이미지가 없거나 이름이 틀렸으면 숨기기
            portraitImage.enabled = false;
            Debug.LogWarning($"이미지를 찾을 수 없음: {portraitName}");
        }

        // 다음 줄로 넘어가기 위해 인덱스 증가
        currentLineIndex++;
    }

    IEnumerator TypingEffect(TextMeshProUGUI textComp, float speed)
    {
        textComp.ForceMeshUpdate();
        int totalVisibleCharacters = textComp.textInfo.characterCount; // 전체 글자 수
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            textComp.maxVisibleCharacters = counter; // 보여줄 글자 수 조절
            counter++;
            yield return new WaitForSeconds(speed); // 타자 속도 대체로 0.05f? 정도 쓰는듯
        }
    }
}